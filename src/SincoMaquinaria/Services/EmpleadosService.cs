using Marten;
using Microsoft.Extensions.Logging;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events;
using SincoMaquinaria.DTOs.Requests;

namespace SincoMaquinaria.Services;

public class EmpleadosService
{
    private readonly IDocumentSession _session;
    private readonly ILogger<EmpleadosService> _logger;

    public EmpleadosService(IDocumentSession session, ILogger<EmpleadosService> logger)
    {
        _session = session;
        _logger = logger;
    }

    public async Task<Result<Guid>> CrearEmpleado(CrearEmpleadoRequest req, Guid userId, string? userName)
    {
        // Validar unicidad de documento
        var existeDocumento = await _session.Query<Empleado>()
            .AnyAsync(e => e.Identificacion == req.Identificacion);

        if (existeDocumento)
        {
            return Result<Guid>.Failure($"Ya existe un empleado con el documento '{req.Identificacion}'");
        }

        var empleadoId = Guid.NewGuid();
        _session.Events.StartStream<Empleado>(empleadoId,
            new EmpleadoCreado(empleadoId, req.Nombre, req.Identificacion,
                req.Cargo, req.Especialidad ?? "", req.ValorHora, req.Estado, userId, userName, DateTimeOffset.Now));

        await _session.SaveChangesAsync();
        _logger.LogInformation("Empleado creado: {EmpleadoId} - {Nombre}", empleadoId, req.Nombre);

        return Result<Guid>.Success(empleadoId);
    }

    public async Task<Result<Unit>> ActualizarEmpleado(Guid id, ActualizarEmpleadoRequest req, Guid userId, string? userName)
    {
        // Validar unicidad de documento (excluyendo el actual)
        var existeDocumento = await _session.Query<Empleado>()
            .AnyAsync(e => e.Id != id && e.Identificacion == req.Identificacion);

        if (existeDocumento)
        {
            return Result<Unit>.Failure($"Ya existe otro empleado con el documento '{req.Identificacion}'");
        }

        var empleado = await _session.LoadAsync<Empleado>(id);
        if (empleado == null)
            return Result<Unit>.NotFound($"Empleado con ID '{id}' no encontrado");

        _session.Events.Append(id,
            new EmpleadoActualizado(id, req.Nombre, req.Identificacion,
                req.Cargo, req.Especialidad ?? "", req.ValorHora, req.Estado, userId, userName, DateTimeOffset.UtcNow));

        await _session.SaveChangesAsync();
        _logger.LogInformation("Empleado actualizado: {EmpleadoId} - {Nombre}", id, req.Nombre);

        return Result<Unit>.Success(Unit.Value);
    }
}

