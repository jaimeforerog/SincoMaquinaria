using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExcelDataReader;
using Marten;
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
        // Necesario para leer encoding legacy de Excel si fuera .xls, aunque .xlsx es zip
        // Se debe registrar el provider en Program.cs
        
        using var reader = ExcelReaderFactory.CreateReader(fileStream);
        var result = reader.AsDataSet(new ExcelDataSetConfiguration()
        {
            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
            {
                UseHeaderRow = false
            }
        });

        var table = result.Tables[0];
        var logLines = new List<string>();
        logLines.Add($"[Import] Leyendo hoja: {table.TableName}. Filas: {table.Rows.Count}");
        // Console.WriteLine($"[Import] Leyendo hoja: {table.TableName}. Filas: {table.Rows.Count}");

        // Find Header Row
        int headerRowIdx = -1;
        var colMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // 1. Cargar unidades válidas (Diccionario Codigo -> Nombre)
        var config = await _session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);
        var catalogoMedidores = config?.TiposMedidor
                                    .Where(t => t.Activo)
                                    // Usamos StringComparer para el Key del diccionario
                                    .ToDictionary(t => t.Unidad, t => t.Nombre, StringComparer.OrdinalIgnoreCase)
                               ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // 2. Cargar rutinas existentes para validación de unicidad
        // Usamos los EVENTOS directamente porque la proyección puede estar vacía si se registró tarde.
        var eventosRutina = await _session.Events.QueryRawEventDataOnly<RutinaMigrada>().ToListAsync();
        var rutinasExistentes = eventosRutina
                                        .Select(r => r.Descripcion)
                                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Console.WriteLine($"[DEBUG] Validation Load (Events): Found {rutinasExistentes.Count} existing routines in EventStore.");
        if (rutinasExistentes.Count > 0) 
        {
             Console.WriteLine($"[DEBUG] Sample Event Routine: '{rutinasExistentes.First()}'");
        }

        for (int r = 0; r < table.Rows.Count; r++)
        {
            var row = table.Rows[r];
            // Check for key columns in this row
            bool isHeader = false;
            for (int c = 0; c < table.Columns.Count; c++)
            {
                 var val = row[c]?.ToString()?.Trim();
                 if (!string.IsNullOrEmpty(val) && (val.Equals("Rutina", StringComparison.OrdinalIgnoreCase) || val.Equals("Equipo", StringComparison.OrdinalIgnoreCase)))
                 {
                     isHeader = true;
                     break;
                 }
            }

            if (isHeader)
            {
                headerRowIdx = r;
                logLines.Add($"[Import] Header found at row {r}");
                for (int c = 0; c < table.Columns.Count; c++)
                {
                    var colName = row[c]?.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(colName) && !colMap.ContainsKey(colName))
                    {
                        colMap[colName] = c;
                        logLines.Add($"[DEBUG-COL] Name:'{colName}' Idx:{c}");
                    }
                }
                break;
            }
        }

        if (headerRowIdx == -1)
        {
             logLines.Add("[Import Error] No header row found. Defaulting to index-based.");
             // Fallback: assume row 0 is header? or specific indexes
        }

        // Helper to get value loosely
        string? GetVal(System.Data.DataRow r, params string[] possibleNames)
        {
            foreach (var name in possibleNames)
            {
                if (colMap.TryGetValue(name, out int index))
                     return r[index]?.ToString()?.Trim();
                // Búsqueda aproximada (Starts with + trim) para manejar "Frecuencia "
                var fuzzy = colMap.Keys.FirstOrDefault(k => k.Trim().StartsWith(name, StringComparison.OrdinalIgnoreCase));
                if (fuzzy != null) return r[colMap[fuzzy]]?.ToString()?.Trim();
            }
            return null;
        }
        
        
        var rutinasCache = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase); 
        var partesCache = new Dictionary<string, (Guid Id, Guid RutinaId)>(StringComparer.OrdinalIgnoreCase); 

        int count = 0;
        var validationErrors = new List<string>();
        var rutinasDuplicadasReportadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (System.Data.DataRow row in table.Rows)
        {
            if (table.Rows.IndexOf(row) <= headerRowIdx) continue; // Skip header and previous rows
            // Mapeo exacto según lo indicado por el usuario
            var nombreRutina = GetVal(row, "Rutina") ?? row[1]?.ToString()?.Trim();
            var grupo = GetVal(row, "Grupo") ?? "General";
            var nombreParte = GetVal(row, "Parte") ?? row[2]?.ToString()?.Trim();
            var descActividad = GetVal(row, "Actividad") ?? row[3]?.ToString()?.Trim();
            var claseActividad = GetVal(row, "Clase Actividad", "Clase") ?? "General";
            
            if (string.IsNullOrEmpty(nombreRutina)) continue;
            
            var rowNum = table.Rows.IndexOf(row) + 1;

            // Debug check
            // Console.WriteLine($"[DEBUG] Checking Row {rowNum}: Routine '{nombreRutina}' vs DB Set ({rutinasExistentes.Count})");

            // Validar Unicidad de Rutina (No debe existir en DB)
            if (rutinasExistentes.Contains(nombreRutina))
            {
                // Solo reportar el error UNA vez por rutina para no llenar el log
                if (!rutinasDuplicadasReportadas.Contains(nombreRutina))
                {
                    Console.WriteLine($"[DEBUG] Duplicate Found! Row {rowNum}: '{nombreRutina}'");
                    validationErrors.Add($"La Rutina '{nombreRutina}' ya existe en el sistema (detectado en fila {rowNum}).");
                    rutinasDuplicadasReportadas.Add(nombreRutina);
                }
                // Aunque no reportemos, NO procesamos la fila
                continue; 
            }

            // --- VALIDACIÓN ---

            if (string.IsNullOrEmpty(nombreParte))
            {
               validationErrors.Add($"Fila {rowNum}: El campo 'Parte' es obligatorio.");
            }
            if (string.IsNullOrEmpty(descActividad))
            {
               validationErrors.Add($"Fila {rowNum}: El campo 'Actividad' es obligatorio.");
            }
             
            // --- Helper Local ---
            string CleanNumericInput(string input)
            {
                if (string.IsNullOrEmpty(input)) return input;
                // Correct common OCR/Typo errors: 'O' or 'o' -> '0'
                return input.Replace("O", "0").Replace("o", "0");
            }

            // Validar Frecuencia
            var frecuenciaStr = CleanNumericInput(GetVal(row, "Frecuencia") ?? "0");
            int frecuencia = 0;
            if (!int.TryParse(frecuenciaStr, out frecuencia))
            {
                if (double.TryParse(frecuenciaStr, out double dFreq))
                    frecuencia = (int)Math.Round(dFreq);
            }
            if (frecuencia <= 0)
            {
                validationErrors.Add($"Fila {rowNum}: La 'Frecuencia' debe ser mayor a 0 (Valor original: {GetVal(row, "Frecuencia") ?? "0"}).");
            }

            // Validar Frecuencia UM 1
            var um = (GetVal(row, "Frec UM", "Frec. UM", "Unidad") ?? string.Empty).ToUpper();
            string nombreMedidor = "";

            if (string.IsNullOrEmpty(um))
            {
                validationErrors.Add($"Fila {rowNum}: La 'Unidad de Medida' es obligatoria.");
            }
            else if (!catalogoMedidores.TryGetValue(um, out nombreMedidor))
            {
                // Validación contra configuración
                 validationErrors.Add($"Fila {rowNum}: La unidad '{um}' no es válida (no existe en Configuración). Unidades permitidas: {string.Join(", ", catalogoMedidores.Keys)}");
            }

            // Si hay errores en esta fila, o ya traemos errores previos, NO procesamos para evitar guardar basura.
            if (validationErrors.Any()) continue;
            
            // --- FIN VALIDACIÓN ---

            // 1. Procesar Rutina
            if (!rutinasCache.TryGetValue(nombreRutina, out var rutinaId))
            {
                rutinaId = Guid.NewGuid();
                rutinasCache[nombreRutina] = rutinaId;
                _session.Events.StartStream<RutinaMantenimiento>(rutinaId, 
                    new RutinaMigrada(rutinaId, nombreRutina, grupo)
                );
                count++; // Solo contamos rutinas nuevas
            }

            // 2. Procesar Parte (Validation handled above)
            
            var parteKey = $"{nombreRutina}|{nombreParte}";
            if (!partesCache.TryGetValue(parteKey, out var parteInfo))
            {
                var parteId = Guid.NewGuid();
                parteInfo = (parteId, rutinaId);
                partesCache[parteKey] = parteInfo;

                _session.Events.Append(rutinaId, new ParteDeRutinaMigrada(parteId, nombreParte, rutinaId));
            }

            // 3. Procesar Actividad
            if (string.IsNullOrEmpty(descActividad)) continue;


            Console.WriteLine($"[Debug] FrecuenciaRaw: '{frecuenciaStr}' -> Int: {frecuencia} for Col: Frecuencia");
            

            
            // Alerta Faltando
            var alertaStr = CleanNumericInput(GetVal(row, "Alerta Faltando", "Alerta", "AlertaFaltando") ?? "0");
            if (!int.TryParse(alertaStr, out int alertaFaltando))
            {
               if (double.TryParse(alertaStr, out double dAlert))
                   alertaFaltando = (int)Math.Round(dAlert);
               else alertaFaltando = 0;
            }

            // --- Frecuencia II ---
            var frecuenciaIIStr = CleanNumericInput(GetVal(row, "Frecuencia II", "Frecuencia 2", "Frecuencia2") ?? "0");
            int frecuencia2 = 0;
            if (int.TryParse(frecuenciaIIStr, out int freq2)) frecuencia2 = freq2;
            else if (double.TryParse(frecuenciaIIStr, out double dFreq2)) frecuencia2 = (int)Math.Round(dFreq2);

             var um2 = (GetVal(row, "Frec UM II", "Frec. UM II", "Unidad II", "Unidad 2") ?? string.Empty).ToUpper();
             string nombreMedidor2 = "";
             if (!string.IsNullOrEmpty(um2))
             {
                 if (!catalogoMedidores.TryGetValue(um2, out nombreMedidor2))
                 {
                     validationErrors.Add($"Fila {rowNum}: La unidad secundaria '{um2}' no es válida. Unidades permitidas: {string.Join(", ", catalogoMedidores.Keys)}");
                 }
             }

            var alertaIIStr = CleanNumericInput(GetVal(row, "Alerta Faltando II", "Alerta II", "Alerta 2") ?? "0");
             int alertaFaltando2 = 0;
             if (int.TryParse(alertaIIStr, out int alert2)) alertaFaltando2 = alert2;
             else if (double.TryParse(alertaIIStr, out double dAlert2)) alertaFaltando2 = (int)Math.Round(dAlert2);


            // Insumo
            var insumoRaw = GetVal(row, "Insumo") ?? (table.Columns.Count > 11 ? row[11]?.ToString()?.Trim() : null);
            var insumo = CleanNumericInput(insumoRaw);
            
            if (!string.IsNullOrEmpty(insumo) && !double.TryParse(insumo, out _))
            {
                 validationErrors.Add($"Fila {rowNum} (Rutina: {nombreRutina}, Parte: {nombreParte}, Actividad: {descActividad}): El campo 'Insumo' debe ser vacío o un valor numérico. Valor encontrado: '{insumo}' (Original: '{insumoRaw}')");
            }

            // Cantidad
            var cantStr = CleanNumericInput(GetVal(row, "Cantidad") ?? (table.Columns.Count > 12 ? row[12]?.ToString()?.Trim() : "0"));
            double.TryParse(cantStr, out double cantidad);

            _session.Events.Append(rutinaId, new ActividadDeRutinaMigrada(
                Guid.NewGuid(),
                descActividad,
                claseActividad,
                frecuencia,
                um,
                nombreMedidor, // Pass desc
                alertaFaltando,
                frecuencia2,
                um2,
                nombreMedidor2, // Pass desc
                alertaFaltando2,
                insumo,
                cantidad,
                parteInfo.Id
            ));

            if (count < 50) 
            {
                 logLines.Add($"[DEBUG-ROW-{count}] Rutina:'{nombreRutina}' Parte:'{nombreParte}' ColFrec:'{colMap.Keys.FirstOrDefault(k => k.StartsWith("Frecuencia"))}' FrecRaw:'{frecuenciaStr}'->{frecuencia} AlertRaw:'{alertaStr}'->{alertaFaltando} Frec2:'{frecuencia2}' Alert2:'{alertaFaltando2}' Insumo:'{insumo}' Cant:'{cantStr}'->{cantidad}");
            }
        }
        


        if (validationErrors.Any())
        {
            throw new Exception("Errores de validación encontrados:\n" + string.Join("\n", validationErrors.Take(10)) + (validationErrors.Count > 10 ? "\n... y más." : ""));
        }

        await _session.SaveChangesAsync();
        return count;
    }
}
