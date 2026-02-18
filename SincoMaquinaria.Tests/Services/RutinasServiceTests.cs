using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Marten;
using SincoMaquinaria.Domain;
using SincoMaquinaria.DTOs.Requests;
using SincoMaquinaria.Services;
using Xunit;

namespace SincoMaquinaria.Tests.Services;

public class RutinasServiceTests : IClassFixture<IntegrationFixture>, IAsyncLifetime
{
    private readonly IntegrationFixture _fixture;
    private RutinasService _rutinasService = null!;
    private IDocumentSession _session = null!;

    public RutinasServiceTests(IntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _session = _fixture.Store.LightweightSession();
        _rutinasService = new RutinasService(_session);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _session.DisposeAsync();
    }

    #region CrearRutina Tests

    [Fact]
    public async Task CrearRutina_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Test User";
        var request = new CreateRutinaRequest(
            Descripcion: "Mantenimiento Preventivo Mensual",
            Grupo: "Excavadoras"
        );

        // Act
        var result = await _rutinasService.CrearRutina(request, userId, userName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.Descripcion.Should().Be(request.Descripcion);
        result.Value.Grupo.Should().Be(request.Grupo);
    }

    [Fact]
    public async Task CrearRutina_WithDuplicateDescripcion_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Test User";
        var descripcion = $"Rutina Duplicada {Guid.NewGuid()}";

        var request1 = new CreateRutinaRequest(descripcion, "Grupo A");
        await _rutinasService.CrearRutina(request1, userId, userName);

        var request2 = new CreateRutinaRequest(descripcion, "Grupo B");

        // Act
        var result = await _rutinasService.CrearRutina(request2, userId, userName);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Ya existe una rutina");
        result.Error.Should().Contain(descripcion);
    }

    [Fact]
    public async Task CrearRutina_WithCaseInsensitiveDuplicate_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Test User";
        var baseDescripcion = $"Rutina Case Test {Guid.NewGuid()}";

        var request1 = new CreateRutinaRequest(baseDescripcion.ToLower(), "Grupo A");
        await _rutinasService.CrearRutina(request1, userId, userName);

        var request2 = new CreateRutinaRequest(baseDescripcion.ToUpper(), "Grupo B");

        // Act
        var result = await _rutinasService.CrearRutina(request2, userId, userName);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Ya existe una rutina");
    }

    [Fact]
    public async Task CrearRutina_WithNullUserId_ShouldStillCreateSuccessfully()
    {
        // Arrange
        var request = new CreateRutinaRequest(
            Descripcion: $"Rutina Sin Usuario {Guid.NewGuid()}",
            Grupo: "Grupo Test"
        );

        // Act
        var result = await _rutinasService.CrearRutina(request, null, null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CrearRutina_WithEmptyGrupo_ShouldCreateSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Test User";
        var request = new CreateRutinaRequest(
            Descripcion: $"Rutina Sin Grupo {Guid.NewGuid()}",
            Grupo: ""
        );

        // Act
        var result = await _rutinasService.CrearRutina(request, userId, userName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Grupo.Should().Be("");
    }

    [Fact]
    public async Task CrearRutina_ShouldEmitRutinaCreadaEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Test User";
        var request = new CreateRutinaRequest(
            Descripcion: $"Rutina Event Test {Guid.NewGuid()}",
            Grupo: "Grupo Event"
        );

        // Act
        var result = await _rutinasService.CrearRutina(request, userId, userName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var events = await _session.Events.FetchStreamAsync(result.Value.Id);
        events.Should().NotBeEmpty();
        events.Should().Contain(e => e.EventType == typeof(RutinaCreada));
    }

    [Fact]
    public async Task CrearRutina_ShouldPersistInDatabase()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Test User";
        var request = new CreateRutinaRequest(
            Descripcion: $"Rutina Persistence Test {Guid.NewGuid()}",
            Grupo: "Grupo Persistence"
        );

        // Act
        var result = await _rutinasService.CrearRutina(request, userId, userName);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify persistence
        var rutina = await _session.LoadAsync<RutinaMantenimiento>(result.Value.Id);
        rutina.Should().NotBeNull();
        rutina!.Descripcion.Should().Be(request.Descripcion);
    }

    [Fact]
    public async Task CrearRutina_WithSpecialCharacters_ShouldCreateSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Test User";
        var request = new CreateRutinaRequest(
            Descripcion: $"Rutina Especial @#$% {Guid.NewGuid()}",
            Grupo: "Grupo-Test_123"
        );

        // Act
        var result = await _rutinasService.CrearRutina(request, userId, userName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Descripcion.Should().Contain("@#$%");
    }

    [Fact]
    public async Task CrearRutina_WithLongDescripcion_ShouldCreateSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Test User";
        var longDescripcion = $"Rutina con descripción muy larga {new string('x', 200)} {Guid.NewGuid()}";
        var request = new CreateRutinaRequest(longDescripcion, "Grupo Test");

        // Act
        var result = await _rutinasService.CrearRutina(request, userId, userName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Descripcion.Should().Be(longDescripcion);
    }

    [Fact]
    public async Task CrearRutina_ShouldInitializePartesListAsEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Test User";
        var request = new CreateRutinaRequest(
            Descripcion: $"Rutina Partes Test {Guid.NewGuid()}",
            Grupo: "Grupo Test"
        );

        // Act
        var result = await _rutinasService.CrearRutina(request, userId, userName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Partes.Should().NotBeNull();
        result.Value.Partes.Should().BeEmpty();
    }

    #endregion

    #region ActualizarRutina Tests

    [Fact]
    public async Task ActualizarRutina_WithValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Test User";

        // Create initial rutina
        var createRequest = new CreateRutinaRequest($"Rutina Original {Guid.NewGuid()}", "Grupo Original");
        var createResult = await _rutinasService.CrearRutina(createRequest, userId, userName);
        var rutinaId = createResult.Value.Id;

        // Prepare update request
        var updateRequest = new UpdateRutinaRequest("Rutina Actualizada", "Grupo Nuevo");

        // Act
        var result = await _rutinasService.ActualizarRutina(rutinaId, updateRequest, userId, userName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Descripcion.Should().Be("Rutina Actualizada");
        result.Value.Grupo.Should().Be("Grupo Nuevo");
    }

    [Fact]
    public async Task ActualizarRutina_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var userName = "Test User";
        var updateRequest = new UpdateRutinaRequest("Descripcion", "Grupo");

        // Act
        var result = await _rutinasService.ActualizarRutina(nonExistentId, updateRequest, userId, userName);

        // Assert
        result.IsNotFound.Should().BeTrue();
        result.Error.Should().Contain("no encontrada");
    }

    [Fact]
    public async Task ActualizarRutina_WithDuplicateDescripcion_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Test User";
        var uniqueId = Guid.NewGuid();

        // Create two rutinas
        var request1 = new CreateRutinaRequest($"Rutina A {uniqueId}", "Grupo A");
        var rutina1 = await _rutinasService.CrearRutina(request1, userId, userName);

        var request2 = new CreateRutinaRequest($"Rutina B {uniqueId}", "Grupo B");
        var rutina2 = await _rutinasService.CrearRutina(request2, userId, userName);

        // Try to update rutina2 with rutina1's descripcion
        var updateRequest = new UpdateRutinaRequest($"Rutina A {uniqueId}", "Grupo B");

        // Act
        var result = await _rutinasService.ActualizarRutina(rutina2.Value.Id, updateRequest, userId, userName);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Ya existe otra rutina");
    }

    [Fact]
    public async Task ActualizarRutina_WithSameDescripcion_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Test User";
        var descripcion = $"Rutina Misma {Guid.NewGuid()}";

        // Create rutina
        var createRequest = new CreateRutinaRequest(descripcion, "Grupo Original");
        var createResult = await _rutinasService.CrearRutina(createRequest, userId, userName);

        // Update with same descripcion but different grupo
        var updateRequest = new UpdateRutinaRequest(descripcion, "Grupo Nuevo");

        // Act
        var result = await _rutinasService.ActualizarRutina(createResult.Value.Id, updateRequest, userId, userName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Grupo.Should().Be("Grupo Nuevo");
    }

    [Fact]
    public async Task ActualizarRutina_WithCaseInsensitiveDuplicate_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Test User";
        var uniqueId = Guid.NewGuid();

        // Create two rutinas
        var request1 = new CreateRutinaRequest($"rutina lowercase {uniqueId}", "Grupo A");
        var rutina1 = await _rutinasService.CrearRutina(request1, userId, userName);

        var request2 = new CreateRutinaRequest($"rutina other {uniqueId}", "Grupo B");
        var rutina2 = await _rutinasService.CrearRutina(request2, userId, userName);

        // Try to update rutina2 with uppercase version of rutina1's descripcion
        var updateRequest = new UpdateRutinaRequest($"RUTINA LOWERCASE {uniqueId}", "Grupo B");

        // Act
        var result = await _rutinasService.ActualizarRutina(rutina2.Value.Id, updateRequest, userId, userName);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Ya existe otra rutina");
    }

    [Fact]
    public async Task ActualizarRutina_ShouldEmitRutinaActualizadaEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Test User";

        // Create rutina
        var createRequest = new CreateRutinaRequest($"Rutina Event Update {Guid.NewGuid()}", "Grupo Original");
        var createResult = await _rutinasService.CrearRutina(createRequest, userId, userName);

        // Update rutina
        var updateRequest = new UpdateRutinaRequest("Rutina Updated", "Grupo Updated");

        // Act
        var result = await _rutinasService.ActualizarRutina(createResult.Value.Id, updateRequest, userId, userName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var events = await _session.Events.FetchStreamAsync(createResult.Value.Id);
        events.Should().Contain(e => e.EventType == typeof(RutinaActualizada));
    }

    [Fact]
    public async Task ActualizarRutina_WithNullUserId_ShouldStillUpdateSuccessfully()
    {
        // Arrange
        // Create with a user
        var createRequest = new CreateRutinaRequest($"Rutina Null User {Guid.NewGuid()}", "Grupo Original");
        var createResult = await _rutinasService.CrearRutina(createRequest, Guid.NewGuid(), "Creator");

        // Update with null user
        var updateRequest = new UpdateRutinaRequest("Rutina Updated By Null", "Grupo Updated");

        // Act
        var result = await _rutinasService.ActualizarRutina(createResult.Value.Id, updateRequest, null, null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Descripcion.Should().Be("Rutina Updated By Null");
    }

    [Fact]
    public async Task ActualizarRutina_OnlyGrupo_ShouldUpdateGrupoOnly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Test User";
        var originalDescripcion = $"Rutina Grupo Only {Guid.NewGuid()}";

        // Create rutina
        var createRequest = new CreateRutinaRequest(originalDescripcion, "Grupo Original");
        var createResult = await _rutinasService.CrearRutina(createRequest, userId, userName);

        // Update only grupo
        var updateRequest = new UpdateRutinaRequest(originalDescripcion, "Grupo Modificado");

        // Act
        var result = await _rutinasService.ActualizarRutina(createResult.Value.Id, updateRequest, userId, userName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Descripcion.Should().Be(originalDescripcion);
        result.Value.Grupo.Should().Be("Grupo Modificado");
    }

    [Fact]
    public async Task ActualizarRutina_ShouldPersistChanges()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Test User";

        // Create rutina
        var createRequest = new CreateRutinaRequest($"Rutina Persist Update {Guid.NewGuid()}", "Grupo Original");
        var createResult = await _rutinasService.CrearRutina(createRequest, userId, userName);

        // Update rutina
        var updateRequest = new UpdateRutinaRequest("Rutina Persistida", "Grupo Persistido");
        await _rutinasService.ActualizarRutina(createResult.Value.Id, updateRequest, userId, userName);

        // Act - Reload from database
        var rutina = await _session.LoadAsync<RutinaMantenimiento>(createResult.Value.Id);

        // Assert
        rutina.Should().NotBeNull();
        rutina!.Descripcion.Should().Be("Rutina Persistida");
        rutina.Grupo.Should().Be("Grupo Persistido");
    }

    [Fact]
    public async Task ActualizarRutina_MultipleTimes_ShouldKeepUpdating()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Test User";

        // Create rutina
        var createRequest = new CreateRutinaRequest($"Rutina Multiple {Guid.NewGuid()}", "Grupo 1");
        var createResult = await _rutinasService.CrearRutina(createRequest, userId, userName);

        // Act - Update multiple times
        await _rutinasService.ActualizarRutina(createResult.Value.Id, new UpdateRutinaRequest("Update 1", "Grupo 2"), userId, userName);
        await _rutinasService.ActualizarRutina(createResult.Value.Id, new UpdateRutinaRequest("Update 2", "Grupo 3"), userId, userName);
        var finalResult = await _rutinasService.ActualizarRutina(createResult.Value.Id, new UpdateRutinaRequest("Update Final", "Grupo Final"), userId, userName);

        // Assert
        finalResult.IsSuccess.Should().BeTrue();
        finalResult.Value.Descripcion.Should().Be("Update Final");
        finalResult.Value.Grupo.Should().Be("Grupo Final");

        // Verify event stream has all events
        var events = await _session.Events.FetchStreamAsync(createResult.Value.Id);
        events.Count(e => e.EventType == typeof(RutinaActualizada)).Should().Be(3);
    }

    #endregion
}
