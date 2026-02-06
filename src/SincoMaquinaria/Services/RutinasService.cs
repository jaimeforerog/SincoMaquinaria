using Marten;
using SincoMaquinaria.Domain;
using SincoMaquinaria.DTOs.Requests;

namespace SincoMaquinaria.Services;

public class RutinasService
{
    private readonly IDocumentSession _session;

    public RutinasService(IDocumentSession session)
    {
        _session = session;
    }

    public async Task<Result<RutinaMantenimiento>> CrearRutina(CreateRutinaRequest request, Guid? userId, string? userName)
    {
        var existe = await _session.Query<RutinaMantenimiento>()
            .AnyAsync(r => r.Descripcion.Equals(request.Descripcion, StringComparison.CurrentCultureIgnoreCase));

        if (existe)
            return Result<RutinaMantenimiento>.Failure($"Ya existe una rutina con el nombre '{request.Descripcion}'");

        var rutinaId = Guid.NewGuid();
        _session.Events.StartStream<RutinaMantenimiento>(rutinaId,
            new RutinaCreada(rutinaId, request.Descripcion, request.Grupo, userId, userName));
        await _session.SaveChangesAsync();

        return Result<RutinaMantenimiento>.Success(new RutinaMantenimiento
        {
            Id = rutinaId,
            Descripcion = request.Descripcion,
            Grupo = request.Grupo,
            Partes = new List<ParteEquipo>()
        });
    }

    public async Task<Result<RutinaMantenimiento>> ActualizarRutina(Guid id, UpdateRutinaRequest request, Guid? userId, string? userName)
    {
        var rutina = await _session.LoadAsync<RutinaMantenimiento>(id);
        if (rutina == null)
            return Result<RutinaMantenimiento>.NotFound("Rutina no encontrada");

        var existe = await _session.Query<RutinaMantenimiento>()
            .AnyAsync(r => r.Id != id && r.Descripcion.Equals(request.Descripcion, StringComparison.CurrentCultureIgnoreCase));

        if (existe)
            return Result<RutinaMantenimiento>.Failure($"Ya existe otra rutina con el nombre '{request.Descripcion}'");

        _session.Events.Append(id, new RutinaActualizada(id, request.Descripcion, request.Grupo, userId, userName));
        await _session.SaveChangesAsync();

        rutina.Descripcion = request.Descripcion;
        rutina.Grupo = request.Grupo;
        return Result<RutinaMantenimiento>.Success(rutina);
    }
}

