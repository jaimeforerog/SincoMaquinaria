using Npgsql;
using System;
using System.Threading.Tasks;

class ResetTestDatabase
{
    private const string TestDatabaseName = "SincoMaquinaria_Test";
    private const string PostgresConnectionString =
        "host=localhost;database=postgres;password=postgres;username=postgres";

    static async Task Main(string[] args)
    {
        try
        {
            await using var conn = new NpgsqlConnection(PostgresConnectionString);
            await conn.OpenAsync();

            Console.WriteLine("Terminating existing connections...");
            await using var cmd1 = conn.CreateCommand();
            cmd1.CommandText = $@"
                SELECT pg_terminate_backend(pid)
                FROM pg_stat_activity
                WHERE datname = '{TestDatabaseName}'
                AND pid <> pg_backend_pid();";
            await cmd1.ExecuteNonQueryAsync();

            Console.WriteLine($"Dropping database {TestDatabaseName}...");
            await using var cmd2 = conn.CreateCommand();
            cmd2.CommandText = $"DROP DATABASE IF EXISTS {TestDatabaseName};";
            await cmd2.ExecuteNonQueryAsync();

            Console.WriteLine($"Creating database {TestDatabaseName}...");
            await using var cmd3 = conn.CreateCommand();
            cmd3.CommandText = $"CREATE DATABASE {TestDatabaseName};";
            await cmd3.ExecuteNonQueryAsync();

            Console.WriteLine("✓ Test database reset successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
