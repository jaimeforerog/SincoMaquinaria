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

    public async Task<int> ImportarRutinas(Stream fileStream, Guid? userId = null, string? userName = null)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage(fileStream);
        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
        if (worksheet == null) return 0;

        // --- VALIDACIÓN DE DEPENDENCIAS ---
        var config = await _session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);
        if (config == null)
            throw new InvalidOperationException("No se ha inicializado la Configuración Global. Por favor cree unidades y grupos primero.");

        var validGrupos = new HashSet<string>(config.GruposMantenimiento.Where(g => g.Activo).Select(g => g.Nombre), StringComparer.OrdinalIgnoreCase);
        // Validamos por Unidad (Código/Símbolo) que es lo que suele venir en el excel (ej: hr, km)
        var validUnidades = new HashSet<string>(config.TiposMedidor.Where(t => t.Activo).Select(t => t.Unidad), StringComparer.OrdinalIgnoreCase);

        var rowCount = worksheet.Dimension.Rows;
        var validationErrors = new List<string>();

        // Primera pasada: Validación
        for (int row = 2; row <= rowCount; row++)
        {
            var grupo = worksheet.Cells[row, 1].Text?.Trim();
            var rutinaDesc = worksheet.Cells[row, 2].Text?.Trim();
            
            // Si no hay grupo ni descripción, asumimos fin de archivo o fila vacía
            if (string.IsNullOrEmpty(grupo) && string.IsNullOrEmpty(rutinaDesc)) continue; 

            if (!string.IsNullOrEmpty(grupo) && !validGrupos.Contains(grupo))
            {
                validationErrors.Add($"Fila {row}: El Grupo de Mantenimiento '{grupo}' no existe o no está activo.");
            }

            // Validar unidades (Cols 7 y 10)
            var unidad1 = worksheet.Cells[row, 7].Text?.Trim();
            var unidad2 = worksheet.Cells[row, 10].Text?.Trim();

            if (!string.IsNullOrEmpty(unidad1) && !validUnidades.Contains(unidad1))
            {
                validationErrors.Add($"Fila {row}: La Unidad de Medida '{unidad1}' no existe o no está activa.");
            }

            if (!string.IsNullOrEmpty(unidad2) && !validUnidades.Contains(unidad2))
            {
                validationErrors.Add($"Fila {row}: La Unidad de Medida '{unidad2}' no existe o no está activa.");
            }
        }

        if (validationErrors.Any())
        {
            throw new InvalidOperationException($"Errores de validación:\n{string.Join("\n", validationErrors)}");
        }
        // ----------------------------------

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
                
                // Evento RutinaMigrada con Auditoría
                rutinasMap[rutinaKey].Events.Add(new RutinaMigrada(rutinaId, rutinaDesc, grupo, userId, userName));
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
