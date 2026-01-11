using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;

namespace SincoMaquinaria.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory that uses local PostgreSQL with a test database.
/// This ensures tests run against a real PostgreSQL instance with isolated data.
/// Authentication is bypassed for integration tests.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Use local PostgreSQL with a dedicated test database
    private const string TestDatabaseName = "SincoMaquinaria_Test";
    private const string TestConnectionString = 
        $"host=localhost;database={TestDatabaseName};password=postgres;username=postgres";
    private const string PostgresConnectionString = 
        "host=localhost;database=postgres;password=postgres;username=postgres";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Override configuration to use test database
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = TestConnectionString
            });
        });

        builder.UseEnvironment("Testing");

        // Override authorization to allow anonymous access in tests
        builder.ConfigureServices(services =>
        {
            // Add a fallback authorization policy that allows anonymous
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAssertion(_ => true) // Always pass authorization
                    .Build();

                // Override Admin policy for tests
                options.AddPolicy("Admin", policy => 
                    policy.RequireAssertion(_ => true));
            });
        });
    }

    public async Task InitializeAsync()
    {
        // Drop and recreate the test database to ensure clean state
        await ResetTestDatabaseAsync();
    }

    public new Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private async Task ResetTestDatabaseAsync()
    {
        try
        {
            await using var conn = new NpgsqlConnection(PostgresConnectionString);
            await conn.OpenAsync();

            // Terminate all connections to the test database
            await using var cmd1 = conn.CreateCommand();
            cmd1.CommandText = $@"
                SELECT pg_terminate_backend(pid) 
                FROM pg_stat_activity 
                WHERE datname = '{TestDatabaseName}' 
                AND pid <> pg_backend_pid();";
            await cmd1.ExecuteNonQueryAsync();

            // Drop and recreate the database
            await using var cmd2 = conn.CreateCommand();
            cmd2.CommandText = $"DROP DATABASE IF EXISTS {TestDatabaseName};";
            await cmd2.ExecuteNonQueryAsync();

            await using var cmd3 = conn.CreateCommand();
            cmd3.CommandText = $"CREATE DATABASE {TestDatabaseName};";
            await cmd3.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            // Log but don't fail - database might already be clean
            Console.WriteLine($"Warning: Could not reset test database: {ex.Message}");
        }
    }
}
