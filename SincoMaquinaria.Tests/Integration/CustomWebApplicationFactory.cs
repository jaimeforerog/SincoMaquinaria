using SincoMaquinaria.Extensions;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Projections;
using JasperFx.Events.Projections;
using Marten;
using Marten.Events.Projections;
using Marten.Schema;
using Marten.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using System.Text.Json;
using System.Text.Json.Serialization;
using Weasel.Core;
using Xunit;

namespace SincoMaquinaria.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory that uses local PostgreSQL with isolated schemas per test run.
/// Each factory instance gets a unique schema (test_{guid}) for complete data isolation.
/// Authentication is bypassed for integration tests.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Use a unique schema per factory instance for complete isolation
    private readonly string _schema = $"test_{Guid.NewGuid():N}";
    private const string TestDatabaseName = "SincoMaquinaria_Test";
    private readonly string _testConnectionString;
    private const string PostgresConnectionString =
        "host=localhost;database=postgres;password=postgres;username=postgres";

    public CustomWebApplicationFactory()
    {
        // Build connection string with unique schema
        _testConnectionString = $"host=localhost;database={TestDatabaseName};password=postgres;username=postgres";

        // Environment variables override appsettings.json and are available when
        // WebApplication.CreateBuilder reads config (before ConfigureWebHost runs).
        // This ensures Marten and DatabaseSetup get the test connection string.
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _testConnectionString);
        Environment.SetEnvironmentVariable("Jwt__Key", "SUPER-SECRET-TEST-KEY-FOR-INTEGRATION-TESTS-1234567890");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "SincoMaquinariaTest");
        Environment.SetEnvironmentVariable("Jwt__Audience", "SincoMaquinariaTestApp");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Override configuration to use test database with unique schema
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _testConnectionString,
                ["Jwt:Key"] = "SUPER-SECRET-TEST-KEY-FOR-INTEGRATION-TESTS-1234567890",
                ["Jwt:Issuer"] = "SincoMaquinariaTest",
                ["Jwt:Audience"] = "SincoMaquinariaTestApp",
                ["Caching:Enabled"] = "false",
                ["Hangfire:DashboardEnabled"] = "false",
                ["Security:EnableAdminEndpoints"] = "true"
            });
        });

        builder.UseEnvironment("Testing");

        // Configure services for testing
        builder.ConfigureServices(services =>
        {
            // Remove existing Marten services
            services.RemoveAll<IDocumentStore>();
            services.RemoveAll<IDocumentSession>();
            services.RemoveAll<IQuerySession>();

            // Re-add Marten with unique schema configuration
            var jsonOptions = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() }
            };

            services.AddMarten(opts =>
            {
                opts.Connection(_testConnectionString);
                opts.DatabaseSchemaName = _schema; // Use unique schema per test run

                // Use custom JSON serializer that handles enums as strings
                var serializer = new SystemTextJsonSerializer(jsonOptions);
                serializer.EnumStorage = EnumStorage.AsString;
                opts.Serializer(serializer);

                // Projections
                opts.Projections.Snapshot<OrdenDeTrabajo>(SnapshotLifecycle.Inline);
                opts.Projections.Snapshot<ConfiguracionGlobal>(SnapshotLifecycle.Inline);
                opts.Projections.Snapshot<Equipo>(SnapshotLifecycle.Inline);
                opts.Projections.Snapshot<RutinaMantenimiento>(SnapshotLifecycle.Inline);
                opts.Projections.Snapshot<Empleado>(SnapshotLifecycle.Inline);
                opts.Projections.Snapshot<Usuario>(SnapshotLifecycle.Inline);

                // Unique indexes
                opts.Schema.For<Equipo>().Index(x => x.Placa, x =>
                {
                    x.IsUnique = true;
                    x.Name = "idx_equipo_placa_unique";
                });

                // Projections
                opts.Projections.Add(new AuditoriaProjection(), ProjectionLifecycle.Inline);
            });

            // Keep real authentication and authorization for tests
            // Tests will use actual JWT tokens obtained from login endpoints
        });
    }

    public async Task InitializeAsync()
    {
        // Ensure the test database exists and create our unique schema
        await EnsureTestDatabaseAsync();
        await CreateSchemaAsync();

        // Force Marten to apply all schema changes to the database
        // This creates all necessary tables in the unique schema
        using (var scope = Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            await store.Storage.ApplyAllConfiguredChangesToDatabaseAsync();
        }

        // Seed test admin user for auth tests
        await SeedAdminUserAsync();
    }

    private async Task SeedAdminUserAsync()
    {
        using var scope = Services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();

        await using var session = store.LightweightSession();

        // Check if admin already exists
        var existingAdmin = await session.Query<Usuario>()
            .FirstOrDefaultAsync(u => u.Email == "admin@test.com");

        if (existingAdmin == null)
        {
            var adminId = Guid.NewGuid();
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");

            var events = new object[]
            {
                new SincoMaquinaria.Domain.Events.Usuario.UsuarioCreado(
                    adminId,
                    "admin@test.com",
                    passwordHash,
                    "Test Admin",
                    RolUsuario.Admin,
                    DateTime.UtcNow
                )
            };

            session.Events.StartStream<Usuario>(adminId, events);
            await session.SaveChangesAsync();
        }
    }

    public new async Task DisposeAsync()
    {
        // Clean up the unique schema after tests complete
        await DropSchemaAsync();
        await base.DisposeAsync();
    }

    private async Task EnsureTestDatabaseAsync()
    {
        try
        {
            await using var conn = new NpgsqlConnection(PostgresConnectionString);
            await conn.OpenAsync();

            // Check if test database exists
            await using var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = $"SELECT 1 FROM pg_database WHERE datname = '{TestDatabaseName}'";
            var exists = await checkCmd.ExecuteScalarAsync();

            if (exists == null)
            {
                // Create test database if it doesn't exist
                await using var createCmd = conn.CreateCommand();
                createCmd.CommandText = $"CREATE DATABASE {TestDatabaseName}";
                await createCmd.ExecuteNonQueryAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not ensure test database exists: {ex.Message}");
        }
    }

    private async Task CreateSchemaAsync()
    {
        try
        {
            await using var conn = new NpgsqlConnection(_testConnectionString);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $"CREATE SCHEMA IF NOT EXISTS {_schema}";
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not create schema {_schema}: {ex.Message}");
        }
    }

    private async Task DropSchemaAsync()
    {
        try
        {
            await using var conn = new NpgsqlConnection(_testConnectionString);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $"DROP SCHEMA IF EXISTS {_schema} CASCADE";
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not drop schema {_schema}: {ex.Message}");
        }
    }
}
