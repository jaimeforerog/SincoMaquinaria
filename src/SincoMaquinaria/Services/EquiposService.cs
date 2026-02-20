using Marten;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events;
using SincoMaquinaria.DTOs.Requests;

namespace SincoMaquinaria.Services;

public class EquiposService
{
    private readonly IDocumentSession _session;
    private readonly ILogger<EquiposService> _logger;

    public EquiposService(IDocumentSession session, ILogger<EquiposService> logger)
    {
        _session = session;
        _logger = logger;
    }

    public async Task<Result<Guid>> CrearEquipo(CrearEquipoRequest req, Guid? userId, string? userName)
    {
        // Validar campos obligatorios
        if (string.IsNullOrEmpty(req.Grupo) || string.IsNullOrEmpty(req.Rutina))
            return Result<Guid>.Failure("El Grupo de Mantenimiento y la Rutina Asignada son obligatorios.");

        // Verificar si ya existe un equipo con la misma placa
        var existe = await _session.Query<Equipo>().AnyAsync(e => e.Placa == req.Placa);
        if (existe)
            return Result<Guid>.Conflict($"Ya existe un equipo con la placa {req.Placa}");

        var id = Guid.NewGuid();

        _session.Events.StartStream<Equipo>(id,
            new EquipoCreado(id, req.Placa, req.Descripcion, req.Marca, req.Modelo,
                req.Serie, req.Codigo, req.TipoMedidorId, req.TipoMedidorId2,
                req.Grupo, req.Rutina, userId, userName, DateTimeOffset.Now));

        // Registrar lecturas iniciales
        if (!string.IsNullOrEmpty(req.TipoMedidorId) && req.LecturaInicial1.HasValue)
        {
            var fecha = req.FechaInicial1 ?? DateTime.Now;
            _session.Events.Append(id, new MedicionRegistrada(req.TipoMedidorId, req.LecturaInicial1.Value, fecha, req.LecturaInicial1.Value, userId, userName));
        }

        if (!string.IsNullOrEmpty(req.TipoMedidorId2) && req.LecturaInicial2.HasValue)
        {
            var fecha = req.FechaInicial2 ?? DateTime.Now;
            _session.Events.Append(id, new MedicionRegistrada(req.TipoMedidorId2, req.LecturaInicial2.Value, fecha, req.LecturaInicial2.Value, userId, userName));
        }

        try
        {
            await _session.SaveChangesAsync();
            _logger.LogInformation("Equipo {EquipoId} creado con placa {Placa} por usuario {UserId}", id, req.Placa, userId);
            return Result<Guid>.Success(id);
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
        {
            _logger.LogWarning("Intento de crear equipo con placa duplicada: {Placa}", req.Placa);
            return Result<Guid>.Conflict($"Ya existe un equipo con la placa {req.Placa}");
        }
    }

    public async Task<Result<Unit>> ActualizarEquipo(Guid id, ActualizarEquipoRequest req, Guid? userId, string? userName)
    {
        // Validar campos obligatorios
        if (string.IsNullOrEmpty(req.Grupo) || string.IsNullOrEmpty(req.Rutina))
            return Result<Unit>.Failure("El Grupo de Mantenimiento y la Rutina Asignada son obligatorios.");

        _session.Events.Append(id,
            new EquipoActualizado(id, req.Descripcion, req.Marca, req.Modelo,
                req.Serie, req.Codigo, req.TipoMedidorId, req.TipoMedidorId2,
                req.Grupo, req.Rutina, userId, userName, DateTimeOffset.UtcNow));
        await _session.SaveChangesAsync();

        _logger.LogInformation("Equipo {EquipoId} actualizado por usuario {UserId}", id, userId);
        return Result<Unit>.Success(Unit.Value);
    }
}
