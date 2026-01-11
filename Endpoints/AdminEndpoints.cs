using ExcelDataReader;
using Marten;
using SincoMaquinaria.Domain;

namespace SincoMaquinaria.Endpoints;

public static class AdminEndpoints
{
    public static WebApplication MapAdminEndpoints(this WebApplication app, IConfiguration configuration)
    {
        var enableAdminEndpoints = configuration.GetValue<bool>("Security:EnableAdminEndpoints", false);

        // Admin group requiere rol de Admin
        var adminGroup = app.MapGroup("/admin")
            .WithTags("Admin")
            .RequireAuthorization("Admin");

        if (enableAdminEndpoints)
        {
            adminGroup.MapGet("/logs", ListarLogs);
            adminGroup.MapPost("/reset-db", ResetDatabase);
        }

        // Debug endpoint (remueve si no es desarrollo, o mantén sin autorización para debugging)
        app.MapGet("/debug/excel-headers", InspeccionarExcel)
            .WithTags("Debug");

        return app;
    }

    private static async Task<IResult> ListarLogs(IQuerySession session)
    {
        var logs = await session.Query<ErrorLog>()
            .OrderByDescending(x => x.Fecha)
            .Take(50)
            .ToListAsync();
        return Results.Ok(logs);
    }

    private static async Task<IResult> ResetDatabase(IDocumentStore store)
    {
        await store.Advanced.Clean.CompletelyRemoveAllAsync();
        return Results.Ok(new { Message = "Base de datos reseteada completamente." });
    }

    private static IResult InspeccionarExcel(string? file)
    {
        var fileName = file ?? "fichaEq.xlsx";
        var path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
        
        if (!File.Exists(path)) 
            return Results.BadRequest($"File not found at {path}");

        using var stream = File.OpenRead(path);
        using var reader = ExcelReaderFactory.CreateReader(stream);
        var result = reader.AsDataSet(new ExcelDataSetConfiguration()
        {
            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
            {
                UseHeaderRow = true
            }
        });
        
        var table = result.Tables[0];
        var columns = new List<string>();
        foreach (System.Data.DataColumn col in table.Columns)
        {
            columns.Add(col.ColumnName);
        }
        
        var dataRow = table.Rows.Count > 0 ? table.Rows[0].ItemArray : new object[0];
        
        return Results.Ok(new { FileName = fileName, Columns = columns, Rows = table.Rows.Count, FirstRow = dataRow });
    }
}
