using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Marten;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events.Empleado;
using SincoMaquinaria.DTOs.Requests;
using Microsoft.Extensions.Logging.Abstractions;
using SincoMaquinaria.Services;
using Xunit;

namespace SincoMaquinaria.Tests.Services;

public class EmpleadosServiceTests : IClassFixture<IntegrationFixture>, IAsyncLifetime
{
    private readonly IntegrationFixture _fixture;
    private EmpleadosService _empleadosService = null!;
    private IDocumentSession _session = null!;

    public EmpleadosServiceTests(IntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _session = _fixture.Store.LightweightSession();
        _empleadosService = new EmpleadosService(_session, NullLogger<EmpleadosService>.Instance);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _session.DisposeAsync();
    }

    #region CrearEmpleado Tests

    [Fact]
    public async Task CrearEmpleado_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Admin Test";
        var request = new CrearEmpleadoRequest(
            Nombre: "Juan Pérez",
            Identificacion: $"ID-{Guid.NewGuid()}",
            Cargo: "Mecanico",
            Especialidad: "Motores Diesel",
            ValorHora: 25000m,
            Estado: "Activo"
        );

        // Act
        var result = await _empleadosService.CrearEmpleado(request, userId, userName);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify empleado was created
        var empleados = await _session.Query<Empleado>()
            .Where(e => e.Identificacion == request.Identificacion)
            .ToListAsync();

        empleados.Should().ContainSingle();
        var empleado = empleados.First();
        empleado.Nombre.Should().Be("Juan Pérez");
        empleado.Cargo.Should().Be(CargoEmpleado.Mecanico);
        empleado.Especialidad.Should().Be("Motores Diesel");
        empleado.ValorHora.Should().Be(25000m);
        empleado.Estado.Should().Be(EstadoEmpleado.Activo);
    }

    [Fact]
    public async Task CrearEmpleado_WithDuplicateIdentificacion_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Admin Test";
        var identificacion = $"DUPLICATE-{Guid.NewGuid()}";

        // Create first empleado
        var empleadoId = Guid.NewGuid();
        _session.Events.StartStream<Empleado>(empleadoId,
            new EmpleadoCreado(empleadoId, "Empleado Existente", identificacion,
                "Operario", "", 20000m, "Activo", userId, userName, DateTimeOffset.Now));
        await _session.SaveChangesAsync();

        var request = new CrearEmpleadoRequest(
            Nombre: "Nuevo Empleado",
            Identificacion: identificacion, // Same identificacion
            Cargo: "Conductor",
            Especialidad: null,
            ValorHora: 30000m,
            Estado: "Activo"
        );

        // Act
        var result = await _empleadosService.CrearEmpleado(request, userId, userName);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Ya existe un empleado con el documento");
        result.Error.Should().Contain(identificacion);
    }

    [Fact]
    public async Task CrearEmpleado_WithNullEspecialidad_ShouldUseEmptyString()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Admin Test";
        var request = new CrearEmpleadoRequest(
            Nombre: "Ana López",
            Identificacion: $"ID-{Guid.NewGuid()}",
            Cargo: "Operario",
            Especialidad: null, // Null especialidad
            ValorHora: 18000m,
            Estado: "Activo"
        );

        // Act
        var result = await _empleadosService.CrearEmpleado(request, userId, userName);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var empleado = await _session.Query<Empleado>()
            .FirstOrDefaultAsync(e => e.Identificacion == request.Identificacion);

        empleado.Should().NotBeNull();
        empleado!.Especialidad.Should().BeEmpty(); // Should be empty string, not null
    }

    [Fact]
    public async Task CrearEmpleado_WithZeroValorHora_ShouldAccept()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Admin Test";
        var request = new CrearEmpleadoRequest(
            Nombre: "Practicante Sin Pago",
            Identificacion: $"ID-{Guid.NewGuid()}",
            Cargo: "Operario",
            Especialidad: "Aprendiz",
            ValorHora: 0m, // Zero valor hora
            Estado: "Activo"
        );

        // Act
        var result = await _empleadosService.CrearEmpleado(request, userId, userName);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var empleado = await _session.Query<Empleado>()
            .FirstOrDefaultAsync(e => e.Identificacion == request.Identificacion);

        empleado.Should().NotBeNull();
        empleado!.ValorHora.Should().Be(0m);
    }

    [Fact]
    public async Task CrearEmpleado_WithEstadoInactivo_ShouldCreateSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Admin Test";
        var request = new CrearEmpleadoRequest(
            Nombre: "Pedro Gómez",
            Identificacion: $"ID-{Guid.NewGuid()}",
            Cargo: "Conductor",
            Especialidad: "Camiones",
            ValorHora: 22000m,
            Estado: "Inactivo"
        );

        // Act
        var result = await _empleadosService.CrearEmpleado(request, userId, userName);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var empleado = await _session.Query<Empleado>()
            .FirstOrDefaultAsync(e => e.Identificacion == request.Identificacion);

        empleado.Should().NotBeNull();
        empleado!.Estado.Should().Be(EstadoEmpleado.Inactivo);
    }

    [Fact]
    public async Task CrearEmpleado_ShouldEmitEmpleadoCreadoEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Admin Test";
        var request = new CrearEmpleadoRequest(
            Nombre: "Carlos Ruiz",
            Identificacion: $"ID-{Guid.NewGuid()}",
            Cargo: "Mecanico",
            Especialidad: "Electricidad",
            ValorHora: 28000m,
            Estado: "Activo"
        );

        // Act
        var result = await _empleadosService.CrearEmpleado(request, userId, userName);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var empleado = await _session.Query<Empleado>()
            .FirstOrDefaultAsync(e => e.Identificacion == request.Identificacion);

        // Verify event was emitted
        var events = await _session.Events.FetchStreamAsync(empleado!.Id);
        events.Should().ContainSingle(e => e.EventType == typeof(EmpleadoCreado));

        var createdEvent = events.First().Data as EmpleadoCreado;
        createdEvent.Should().NotBeNull();
        createdEvent!.Nombre.Should().Be("Carlos Ruiz");
        createdEvent.Cargo.Should().Be("Mecanico");
        createdEvent.Especialidad.Should().Be("Electricidad");
        createdEvent.ValorHora.Should().Be(28000m);
        createdEvent.UsuarioId.Should().Be(userId);
        createdEvent.UsuarioNombre.Should().Be(userName);
    }

    [Fact]
    public async Task CrearEmpleado_WithCargoOperario_ShouldCreateSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Admin Test";
        var request = new CrearEmpleadoRequest(
            Nombre: "Luis Martínez",
            Identificacion: $"ID-{Guid.NewGuid()}",
            Cargo: "Operario",
            Especialidad: "Soldadura",
            ValorHora: 20000m,
            Estado: "Activo"
        );

        // Act
        var result = await _empleadosService.CrearEmpleado(request, userId, userName);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var empleado = await _session.Query<Empleado>()
            .FirstOrDefaultAsync(e => e.Identificacion == request.Identificacion);

        empleado.Should().NotBeNull();
        empleado!.Cargo.Should().Be(CargoEmpleado.Operario);
    }

    [Fact]
    public async Task CrearEmpleado_WithCargoConductor_ShouldCreateSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Admin Test";
        var request = new CrearEmpleadoRequest(
            Nombre: "Miguel Ángel",
            Identificacion: $"ID-{Guid.NewGuid()}",
            Cargo: "Conductor",
            Especialidad: "Transporte pesado",
            ValorHora: 24000m,
            Estado: "Activo"
        );

        // Act
        var result = await _empleadosService.CrearEmpleado(request, userId, userName);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var empleado = await _session.Query<Empleado>()
            .FirstOrDefaultAsync(e => e.Identificacion == request.Identificacion);

        empleado.Should().NotBeNull();
        empleado!.Cargo.Should().Be(CargoEmpleado.Conductor);
    }

    [Fact]
    public async Task CrearEmpleado_ShouldBeLoadableAfterCreation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Admin Test";
        var identificacion = $"ID-{Guid.NewGuid()}";
        var request = new CrearEmpleadoRequest(
            Nombre: "Roberto Silva",
            Identificacion: identificacion,
            Cargo: "Mecanico",
            Especialidad: "Hidráulica",
            ValorHora: 26000m,
            Estado: "Activo"
        );

        // Act
        await _empleadosService.CrearEmpleado(request, userId, userName);

        // Create new session to verify persistence
        await using var verificationSession = _fixture.Store.LightweightSession();
        var empleado = await verificationSession.Query<Empleado>()
            .FirstOrDefaultAsync(e => e.Identificacion == identificacion);

        // Assert
        empleado.Should().NotBeNull();
        empleado!.Nombre.Should().Be("Roberto Silva");
        empleado.Identificacion.Should().Be(identificacion);
    }

    #endregion

    #region ActualizarEmpleado Tests

    [Fact]
    public async Task ActualizarEmpleado_WithValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Admin Test";
        var empleadoId = Guid.NewGuid();
        var identificacion = $"ID-{Guid.NewGuid()}";

        // Create empleado first
        _session.Events.StartStream<Empleado>(empleadoId,
            new EmpleadoCreado(empleadoId, "Nombre Original", identificacion,
                "Operario", "Especialidad Original", 15000m, "Activo", userId, userName, DateTimeOffset.Now));
        await _session.SaveChangesAsync();

        var updateRequest = new ActualizarEmpleadoRequest(
            Nombre: "Nombre Actualizado",
            Identificacion: identificacion,
            Cargo: "Mecanico",
            Especialidad: "Nueva Especialidad",
            ValorHora: 30000m,
            Estado: "Activo"
        );

        // Act
        var result = await _empleadosService.ActualizarEmpleado(empleadoId, updateRequest, userId, userName);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var empleado = await _session.LoadAsync<Empleado>(empleadoId);
        empleado.Should().NotBeNull();
        empleado!.Nombre.Should().Be("Nombre Actualizado");
        empleado.Cargo.Should().Be(CargoEmpleado.Mecanico);
        empleado.Especialidad.Should().Be("Nueva Especialidad");
        empleado.ValorHora.Should().Be(30000m);
    }

    [Fact]
    public async Task ActualizarEmpleado_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Admin Test";
        var nonExistentId = Guid.NewGuid();

        var updateRequest = new ActualizarEmpleadoRequest(
            Nombre: "Test",
            Identificacion: "12345",
            Cargo: "Operario",
            Especialidad: "Test",
            ValorHora: 20000m,
            Estado: "Activo"
        );

        // Act
        var result = await _empleadosService.ActualizarEmpleado(nonExistentId, updateRequest, userId, userName);

        // Assert
        result.IsNotFound.Should().BeTrue();
        result.Error.Should().Contain("no encontrado");
    }

    [Fact]
    public async Task ActualizarEmpleado_WithDuplicateIdentificacion_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Admin Test";

        // Create first empleado
        var empleado1Id = Guid.NewGuid();
        var identificacion1 = $"ID-{Guid.NewGuid()}";
        _session.Events.StartStream<Empleado>(empleado1Id,
            new EmpleadoCreado(empleado1Id, "Empleado 1", identificacion1,
                "Operario", "", 20000m, "Activo", userId, userName, DateTimeOffset.Now));

        // Create second empleado
        var empleado2Id = Guid.NewGuid();
        var identificacion2 = $"ID-{Guid.NewGuid()}";
        _session.Events.StartStream<Empleado>(empleado2Id,
            new EmpleadoCreado(empleado2Id, "Empleado 2", identificacion2,
                "Conductor", "", 22000m, "Activo", userId, userName, DateTimeOffset.Now));

        await _session.SaveChangesAsync();

        // Try to update empleado2 with empleado1's identificacion
        var updateRequest = new ActualizarEmpleadoRequest(
            Nombre: "Empleado 2 Updated",
            Identificacion: identificacion1, // Duplicate!
            Cargo: "Conductor",
            Especialidad: "",
            ValorHora: 25000m,
            Estado: "Activo"
        );

        // Act
        var result = await _empleadosService.ActualizarEmpleado(empleado2Id, updateRequest, userId, userName);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Ya existe otro empleado con el documento");
        result.Error.Should().Contain(identificacion1);
    }

    [Fact]
    public async Task ActualizarEmpleado_WithSameIdentificacion_ShouldAllowUpdate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Admin Test";
        var empleadoId = Guid.NewGuid();
        var identificacion = $"ID-{Guid.NewGuid()}";

        // Create empleado
        _session.Events.StartStream<Empleado>(empleadoId,
            new EmpleadoCreado(empleadoId, "Nombre Original", identificacion,
                "Operario", "", 15000m, "Activo", userId, userName, DateTimeOffset.Now));
        await _session.SaveChangesAsync();

        // Update with same identificacion but different data
        var updateRequest = new ActualizarEmpleadoRequest(
            Nombre: "Nombre Actualizado",
            Identificacion: identificacion, // Same identificacion
            Cargo: "Mecanico",
            Especialidad: "Nueva",
            ValorHora: 30000m,
            Estado: "Activo"
        );

        // Act
        var result = await _empleadosService.ActualizarEmpleado(empleadoId, updateRequest, userId, userName);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ActualizarEmpleado_ChangingEstadoToInactivo_ShouldUpdateSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Admin Test";
        var empleadoId = Guid.NewGuid();
        var identificacion = $"ID-{Guid.NewGuid()}";

        // Create active empleado
        _session.Events.StartStream<Empleado>(empleadoId,
            new EmpleadoCreado(empleadoId, "Test Employee", identificacion,
                "Operario", "", 20000m, "Activo", userId, userName, DateTimeOffset.Now));
        await _session.SaveChangesAsync();

        var updateRequest = new ActualizarEmpleadoRequest(
            Nombre: "Test Employee",
            Identificacion: identificacion,
            Cargo: "Operario",
            Especialidad: "",
            ValorHora: 20000m,
            Estado: "Inactivo" // Change to inactive
        );

        // Act
        var result = await _empleadosService.ActualizarEmpleado(empleadoId, updateRequest, userId, userName);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var empleado = await _session.LoadAsync<Empleado>(empleadoId);
        empleado!.Estado.Should().Be(EstadoEmpleado.Inactivo);
    }

    [Fact]
    public async Task ActualizarEmpleado_ChangingNombre_ShouldUpdateSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Admin Test";
        var empleadoId = Guid.NewGuid();
        var identificacion = $"ID-{Guid.NewGuid()}";

        _session.Events.StartStream<Empleado>(empleadoId,
            new EmpleadoCreado(empleadoId, "Original Name", identificacion,
                "Operario", "", 20000m, "Activo", userId, userName, DateTimeOffset.Now));
        await _session.SaveChangesAsync();

        var updateRequest = new ActualizarEmpleadoRequest(
            Nombre: "Updated Name", // Changed name
            Identificacion: identificacion,
            Cargo: "Operario",
            Especialidad: "",
            ValorHora: 20000m,
            Estado: "Activo"
        );

        // Act
        var result = await _empleadosService.ActualizarEmpleado(empleadoId, updateRequest, userId, userName);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var empleado = await _session.LoadAsync<Empleado>(empleadoId);
        empleado!.Nombre.Should().Be("Updated Name");
    }

    [Fact]
    public async Task ActualizarEmpleado_ChangingCargo_ShouldUpdateSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Admin Test";
        var empleadoId = Guid.NewGuid();
        var identificacion = $"ID-{Guid.NewGuid()}";

        _session.Events.StartStream<Empleado>(empleadoId,
            new EmpleadoCreado(empleadoId, "Test", identificacion,
                "Operario", "", 20000m, "Activo", userId, userName, DateTimeOffset.Now));
        await _session.SaveChangesAsync();

        var updateRequest = new ActualizarEmpleadoRequest(
            Nombre: "Test",
            Identificacion: identificacion,
            Cargo: "Mecanico", // Changed cargo
            Especialidad: "",
            ValorHora: 20000m,
            Estado: "Activo"
        );

        // Act
        var result = await _empleadosService.ActualizarEmpleado(empleadoId, updateRequest, userId, userName);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var empleado = await _session.LoadAsync<Empleado>(empleadoId);
        empleado!.Cargo.Should().Be(CargoEmpleado.Mecanico);
    }

    [Fact]
    public async Task ActualizarEmpleado_ChangingValorHora_ShouldUpdateSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Admin Test";
        var empleadoId = Guid.NewGuid();
        var identificacion = $"ID-{Guid.NewGuid()}";

        _session.Events.StartStream<Empleado>(empleadoId,
            new EmpleadoCreado(empleadoId, "Test", identificacion,
                "Operario", "", 20000m, "Activo", userId, userName, DateTimeOffset.Now));
        await _session.SaveChangesAsync();

        var updateRequest = new ActualizarEmpleadoRequest(
            Nombre: "Test",
            Identificacion: identificacion,
            Cargo: "Operario",
            Especialidad: "",
            ValorHora: 35000m, // Changed valor hora
            Estado: "Activo"
        );

        // Act
        var result = await _empleadosService.ActualizarEmpleado(empleadoId, updateRequest, userId, userName);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var empleado = await _session.LoadAsync<Empleado>(empleadoId);
        empleado!.ValorHora.Should().Be(35000m);
    }

    [Fact]
    public async Task ActualizarEmpleado_ChangingEspecialidadToNull_ShouldUseEmptyString()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Admin Test";
        var empleadoId = Guid.NewGuid();
        var identificacion = $"ID-{Guid.NewGuid()}";

        _session.Events.StartStream<Empleado>(empleadoId,
            new EmpleadoCreado(empleadoId, "Test", identificacion,
                "Operario", "Original Especialidad", 20000m, "Activo", userId, userName, DateTimeOffset.Now));
        await _session.SaveChangesAsync();

        var updateRequest = new ActualizarEmpleadoRequest(
            Nombre: "Test",
            Identificacion: identificacion,
            Cargo: "Operario",
            Especialidad: null, // Changed to null
            ValorHora: 20000m,
            Estado: "Activo"
        );

        // Act
        var result = await _empleadosService.ActualizarEmpleado(empleadoId, updateRequest, userId, userName);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var empleado = await _session.LoadAsync<Empleado>(empleadoId);
        empleado!.Especialidad.Should().BeEmpty(); // Should be empty string
    }

    [Fact]
    public async Task ActualizarEmpleado_ShouldEmitEmpleadoActualizadoEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "Admin Test";
        var empleadoId = Guid.NewGuid();
        var identificacion = $"ID-{Guid.NewGuid()}";

        _session.Events.StartStream<Empleado>(empleadoId,
            new EmpleadoCreado(empleadoId, "Original", identificacion,
                "Operario", "Test", 20000m, "Activo", userId, userName, DateTimeOffset.Now));
        await _session.SaveChangesAsync();

        var updateRequest = new ActualizarEmpleadoRequest(
            Nombre: "Updated",
            Identificacion: identificacion,
            Cargo: "Mecanico",
            Especialidad: "New Spec",
            ValorHora: 30000m,
            Estado: "Activo"
        );

        // Act
        var result = await _empleadosService.ActualizarEmpleado(empleadoId, updateRequest, userId, userName);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify event was emitted
        var events = await _session.Events.FetchStreamAsync(empleadoId);
        events.Should().Contain(e => e.EventType == typeof(EmpleadoActualizado));

        var updatedEvent = events.First(e => e.EventType == typeof(EmpleadoActualizado)).Data as EmpleadoActualizado;
        updatedEvent.Should().NotBeNull();
        updatedEvent!.Nombre.Should().Be("Updated");
        updatedEvent.Cargo.Should().Be("Mecanico");
        updatedEvent.Especialidad.Should().Be("New Spec");
        updatedEvent.ValorHora.Should().Be(30000m);
        updatedEvent.UsuarioId.Should().Be(userId);
        updatedEvent.UsuarioNombre.Should().Be(userName);
    }

    #endregion
}
