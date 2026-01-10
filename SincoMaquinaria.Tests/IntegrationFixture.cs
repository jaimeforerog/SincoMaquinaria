using Marten;
using Npgsql;
using Xunit;

namespace SincoMaquinaria.Tests;

/// <summary>
/// Integration test fixture following Dometrain patterns:
/// - Unique schema per test run (UUID-based) for isolation
/// - Schema cleanup on dispose
/// </summary>
public class IntegrationFixture : IAsyncLifetime
{
    private readonly string _schema = $"test_{Guid.NewGuid().ToString().Replace("-", "")}";
    private const string ConnectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres";
    
    public IDocumentStore Store { get; private set; } = null!;
    
    public async Task InitializeAsync()
    {
        // Create isolated schema for this test run
        await CreateSchemaAsync();
        
        Store = DocumentStore.For(opts =>
        {
            opts.Connection(ConnectionString);
            opts.DatabaseSchemaName = _schema;
            
            // Enable Inline Projections for Tests
            opts.Projections.Snapshot<SincoMaquinaria.Domain.Empleado>(Marten.Events.Projections.SnapshotLifecycle.Inline);
            opts.Projections.Snapshot<SincoMaquinaria.Domain.Equipo>(Marten.Events.Projections.SnapshotLifecycle.Inline);
            opts.Projections.Snapshot<SincoMaquinaria.Domain.OrdenDeTrabajo>(Marten.Events.Projections.SnapshotLifecycle.Inline);
            opts.Projections.Snapshot<SincoMaquinaria.Domain.RutinaMantenimiento>(Marten.Events.Projections.SnapshotLifecycle.Inline);
            opts.Projections.Snapshot<SincoMaquinaria.Domain.ConfiguracionGlobal>(Marten.Events.Projections.SnapshotLifecycle.Inline);
        });
    }

    public async Task DisposeAsync()
    {
        if (Store != null)
        {
            await Store.DisposeAsync();
        }
        // Drop schema to clean up
        await DropSchemaAsync();
    }
    
    private async Task CreateSchemaAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE SCHEMA IF NOT EXISTS {_schema}";
        await command.ExecuteNonQueryAsync();
    }
    
    private async Task DropSchemaAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"DROP SCHEMA IF EXISTS {_schema} CASCADE";
        await command.ExecuteNonQueryAsync();
    }
}
