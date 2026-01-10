using Npgsql;

namespace SincoMaquinaria.Infrastructure;

public static class DatabaseSetup
{
    public static void EnsureDatabaseExists(string connectionString)
    {
        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            var dbName = builder.Database;

            // Validar nombre de base de datos para prevenir SQL injection
            if (string.IsNullOrWhiteSpace(dbName) ||
                !System.Text.RegularExpressions.Regex.IsMatch(dbName, @"^[a-zA-Z0-9_]+$"))
            {
                throw new InvalidOperationException($"Nombre de base de datos inválido: {dbName}");
            }

            builder.Database = "postgres"; // Conectar a postgres para mantenimiento

            using var conn = new NpgsqlConnection(builder.ToString());
            conn.Open();
            using var cmd = conn.CreateCommand();

            // Usar parámetros para prevenir SQL injection
            cmd.CommandText = "SELECT 1 FROM pg_database WHERE datname = @dbName";
            cmd.Parameters.AddWithValue("@dbName", dbName);

            if (cmd.ExecuteScalar() == null)
            {
                Console.WriteLine($"[Setup] Creando DB '{dbName}'...");
                // CREATE DATABASE no soporta parámetros, pero ya validamos el nombre
                cmd.CommandText = $"CREATE DATABASE \"{dbName}\"";
                cmd.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Warning] DB Setup ignorado: {ex.Message}");
        }
    }
}
