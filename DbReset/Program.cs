using Npgsql;

const string TestDatabaseName = "SincoMaquinaria_Test";
const string PostgresConnectionString = "Host=localhost;Database=postgres;Username=postgres;Password=postgres";

try
{
    await using var conn = new NpgsqlConnection(PostgresConnectionString);
    await conn.OpenAsync();

    Console.WriteLine("Terminating existing connections...");
    await using (var cmd1 = conn.CreateCommand())
    {
        cmd1.CommandText = $@"
            SELECT pg_terminate_backend(pid)
            FROM pg_stat_activity
            WHERE datname = '{TestDatabaseName}'
            AND pid <> pg_backend_pid();";
        await cmd1.ExecuteNonQueryAsync();
    }

    Console.WriteLine($"Dropping database {TestDatabaseName}...");
    await using (var cmd2 = conn.CreateCommand())
    {
        cmd2.CommandText = $"DROP DATABASE IF EXISTS \"{TestDatabaseName}\";";
        await cmd2.ExecuteNonQueryAsync();
    }

    Console.WriteLine($"Creating database {TestDatabaseName}...");
    await using (var cmd3 = conn.CreateCommand())
    {
        cmd3.CommandText = $"CREATE DATABASE \"{TestDatabaseName}\";";
        await cmd3.ExecuteNonQueryAsync();
    }

    Console.WriteLine("✅ Test database reset successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    Environment.Exit(1);
}
