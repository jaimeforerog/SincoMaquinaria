using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Marten;
using OfficeOpenXml;
using SincoMaquinaria.Domain;

namespace SincoMaquinaria.Services;

public class ExcelImportService
{
    private readonly IDocumentSession _session;

    public ExcelImportService(IDocumentSession session)
    {
        _session = session;
    }

    public async Task<int> ImportarRutinas(Stream fileStream)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage(fileStream);
        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
        if (worksheet == null) return 0;

        var rowCount = worksheet.Dimension.Rows;
        var rutinasMap = new Dictionary<string, (Guid Id, List<object> Events, Dictionary<string, Guid> PartesMap)>();

        for (int row = 2; row <= rowCount; row++)
        {
            var grupo = worksheet.Cells[row, 1].Text?.Trim();
            var rutinaDesc = worksheet.Cells[row, 2].Text?.Trim();
            var parteDesc = worksheet.Cells[row, 3].Text?.Trim();
            var actividadDesc = worksheet.Cells[row, 4].Text?.Trim();
            
            if (string.IsNullOrEmpty(grupo) || string.IsNullOrEmpty(rutinaDesc)) continue;

            // Identificador único de Rutina (Grupo + Descripción)
            var rutinaKey = $"{grupo}|{rutinaDesc}";

            if (!rutinasMap.ContainsKey(rutinaKey))
            {
                var rutinaId = Guid.NewGuid();
                rutinasMap[rutinaKey] = (rutinaId, new List<object>(), new Dictionary<string, Guid>());
                
                // Evento RutinaMigrada
                rutinasMap[rutinaKey].Events.Add(new RutinaMigrada(rutinaId, rutinaDesc, grupo));
            }

            var (rId, rEvents, rPartes) = rutinasMap[rutinaKey];

            if (string.IsNullOrEmpty(parteDesc)) continue;

            // Identificar Parte
            if (!rPartes.ContainsKey(parteDesc))
            {
                var parteId = Guid.NewGuid();
                rPartes[parteDesc] = parteId;
                rEvents.Add(new ParteDeRutinaMigrada(parteId, parteDesc, rId));
            }

            var pId = rPartes[parteDesc];

            if (string.IsNullOrEmpty(actividadDesc)) continue;

            // Leer datos de Actividad
            var clase = worksheet.Cells[row, 5].Text?.Trim() ?? "";
            
            int.TryParse(worksheet.Cells[row, 6].Text, out var freq);
            var frecUm = worksheet.Cells[row, 7].Text?.Trim() ?? "";
            int.TryParse(worksheet.Cells[row, 8].Text, out var alerta);
            
            int.TryParse(worksheet.Cells[row, 9].Text, out var freq2);
            var frecUm2 = worksheet.Cells[row, 10].Text?.Trim() ?? "";
            int.TryParse(worksheet.Cells[row, 11].Text, out var alerta2);
            
            var insumo = worksheet.Cells[row, 12].Text?.Trim();
            double.TryParse(worksheet.Cells[row, 13].Text, out var cantidad);

            // Crear Actividad
            var actividadId = Guid.NewGuid();
            var evt = new ActividadDeRutinaMigrada(
                actividadId,
                actividadDesc,
                clase,
                freq,
                frecUm,
                "", // NombreMedidor (No en plantilla, dejar vacío)
                alerta,
                freq2,
                frecUm2,
                "", // NombreMedidor2
                alerta2,
                insumo,
                cantidad,
                pId
            );

            rEvents.Add(evt);
        }

        // Persistir todos los streams
        foreach (var kvp in rutinasMap)
        {
            var (rutinaId, events, _) = kvp.Value;
            // Verificar si ya existe? Marten StartStream sobrescribe o lanza excepción si existe?
            // StartStream para nuevo. Append para existente.
            // Asumimos migración limpia o nuevas rutinas.
            
            // Verificamos si existe (opcional, costoso). 
            // Si usamos StartStream con un ID random (Guid.NewGuid()), siempre es nuevo.
            // Si el Excel tiene duplicados de NOMBRE, aquí los estamos agrupando.
            
            _session.Events.StartStream<RutinaMantenimiento>(rutinaId, events);
        }

        await _session.SaveChangesAsync();

        return rutinasMap.Count;
    }
}
