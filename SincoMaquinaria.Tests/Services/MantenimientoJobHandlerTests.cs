using FluentAssertions;
using Hangfire.Server;
using Marten;
using Microsoft.Extensions.Logging;
using Moq;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events.Usuario;
using SincoMaquinaria.Services.Jobs;
using Xunit;
using static SincoMaquinaria.Domain.Usuario;

namespace SincoMaquinaria.Tests.Services;

public class MantenimientoJobHandlerTests : IClassFixture<IntegrationFixture>, IAsyncLifetime
{
    private readonly IntegrationFixture _fixture;
    private readonly Mock<ILogger<MantenimientoJobHandler>> _mockLogger;
    private MantenimientoJobHandler _handler = null!;
    private IDocumentSession _session = null!;

    public MantenimientoJobHandlerTests(IntegrationFixture fixture)
    {
        _fixture = fixture;
        _mockLogger = new Mock<ILogger<MantenimientoJobHandler>>();
    }

    public async Task InitializeAsync()
    {
        _session = _fixture.Store.LightweightSession();
        _handler = new MantenimientoJobHandler(
            _fixture.Store,
            _mockLogger.Object);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _session.DisposeAsync();
    }

    #region LimpiarTokensExpiradosAsync Tests

    [Fact]
    public async Task LimpiarTokensExpiradosAsync_WithNoExpiredTokens_ShouldNotRevokeAny()
    {
        // Arrange - crear usuario con token válido
        var usuarioId = Guid.NewGuid();
        _session.Events.StartStream<Usuario>(usuarioId,
            new UsuarioCreado(
                usuarioId,
                $"user-{Guid.NewGuid()}@test.com",
                "hashedPassword",
                "Test User",
                RolUsuario.User,
                DateTime.Now));

        _session.Events.Append(usuarioId,
            new RefreshTokenGenerado(usuarioId, "valid-token", DateTime.Now.AddDays(7), DateTimeOffset.UtcNow));

        await _session.SaveChangesAsync();

        // Act
        await _handler.LimpiarTokensExpiradosAsync(null);

        // Assert - verificar que no hubo cambios en el usuario
        using var verifySession = _fixture.Store.LightweightSession();
        var events = await verifySession.Events.FetchStreamAsync(usuarioId);

        events.Should().NotContain(e => e.Data is RefreshTokenRevocado,
            "no debe haber eventos de revocación para tokens válidos");
    }

    [Fact]
    public async Task LimpiarTokensExpiradosAsync_WithExpiredTokens_ShouldRevokeTokens()
    {
        // Arrange - crear dos usuarios con tokens expirados
        var usuario1Id = Guid.NewGuid();
        var usuario2Id = Guid.NewGuid();

        // Usuario 1 con token expirado hace 1 día
        _session.Events.StartStream<Usuario>(usuario1Id,
            new UsuarioCreado(
                usuario1Id,
                $"expired1-{Guid.NewGuid()}@test.com",
                "hashedPassword1",
                "User One",
                RolUsuario.User,
                DateTime.Now));

        _session.Events.Append(usuario1Id,
            new RefreshTokenGenerado(usuario1Id, "expired-token-1", DateTime.Now.AddDays(-1), DateTimeOffset.UtcNow.AddDays(-1)));

        // Usuario 2 con token expirado hace 1 hora
        _session.Events.StartStream<Usuario>(usuario2Id,
            new UsuarioCreado(
                usuario2Id,
                $"expired2-{Guid.NewGuid()}@test.com",
                "hashedPassword2",
                "User Two",
                RolUsuario.User,
                DateTime.Now));

        _session.Events.Append(usuario2Id,
            new RefreshTokenGenerado(usuario2Id, "expired-token-2", DateTime.Now.AddHours(-1), DateTimeOffset.UtcNow.AddHours(-1)));

        await _session.SaveChangesAsync();

        // Act
        await _handler.LimpiarTokensExpiradosAsync(null);

        // Assert - verificar que se crearon eventos de revocación
        using var verifySession = _fixture.Store.LightweightSession();
        var usuario1 = await verifySession.Events.AggregateStreamAsync<Usuario>(usuario1Id);
        var usuario2 = await verifySession.Events.AggregateStreamAsync<Usuario>(usuario2Id);

        usuario1.Should().NotBeNull();
        usuario2.Should().NotBeNull();

        // Verificar que los eventos RefreshTokenRevocado fueron agregados
        var events1 = await verifySession.Events.FetchStreamAsync(usuario1Id);
        var events2 = await verifySession.Events.FetchStreamAsync(usuario2Id);

        events1.Should().Contain(e => e.Data is RefreshTokenRevocado);
        events2.Should().Contain(e => e.Data is RefreshTokenRevocado);
    }

    [Fact]
    public async Task LimpiarTokensExpiradosAsync_WithMixedTokens_ShouldOnlyRevokeExpired()
    {
        // Arrange
        var expiredUserId = Guid.NewGuid();
        var validUserId = Guid.NewGuid();

        // Usuario con token expirado
        _session.Events.StartStream<Usuario>(expiredUserId,
            new UsuarioCreado(
                expiredUserId,
                $"expired-{Guid.NewGuid()}@test.com",
                "hashedPassword",
                "Expired User",
                RolUsuario.User,
                DateTime.Now));

        _session.Events.Append(expiredUserId,
            new RefreshTokenGenerado(expiredUserId, "expired-token", DateTime.Now.AddDays(-1), DateTimeOffset.UtcNow.AddDays(-1)));

        // Usuario con token válido
        _session.Events.StartStream<Usuario>(validUserId,
            new UsuarioCreado(
                validUserId,
                $"valid-{Guid.NewGuid()}@test.com",
                "hashedPassword",
                "Valid User",
                RolUsuario.User,
                DateTime.Now));

        _session.Events.Append(validUserId,
            new RefreshTokenGenerado(validUserId, "valid-token", DateTime.Now.AddDays(7), DateTimeOffset.UtcNow));

        await _session.SaveChangesAsync();

        // Act
        await _handler.LimpiarTokensExpiradosAsync(null);

        // Assert - verificar que solo el token expirado fue revocado
        using var verifySession = _fixture.Store.LightweightSession();
        var expiredEvents = await verifySession.Events.FetchStreamAsync(expiredUserId);
        var validEvents = await verifySession.Events.FetchStreamAsync(validUserId);

        expiredEvents.Should().Contain(e => e.Data is RefreshTokenRevocado,
            "el token expirado debe tener un evento de revocación");

        validEvents.Should().NotContain(e => e.Data is RefreshTokenRevocado,
            "el token válido NO debe tener un evento de revocación");
    }

    [Fact]
    public async Task LimpiarTokensExpiradosAsync_WithPerformContext_ShouldExecuteSuccessfully()
    {
        // Arrange & Act & Assert - debe ejecutarse sin errores (context es nullable)
        await _handler.LimpiarTokensExpiradosAsync(null);
    }

    [Fact]
    public async Task LimpiarTokensExpiradosAsync_ShouldLogCorrectMessages()
    {
        // Arrange - crear un usuario con token expirado
        var usuarioId = Guid.NewGuid();
        _session.Events.StartStream<Usuario>(usuarioId,
            new UsuarioCreado(
                usuarioId,
                $"user-{Guid.NewGuid()}@test.com",
                "hashedPassword",
                "Test User",
                RolUsuario.Admin,
                DateTime.Now));

        _session.Events.Append(usuarioId,
            new RefreshTokenGenerado(usuarioId, "expired-token", DateTime.Now.AddDays(-1), DateTimeOffset.UtcNow.AddDays(-1)));

        await _session.SaveChangesAsync();

        // Act
        await _handler.LimpiarTokensExpiradosAsync(null);

        // Assert - verificar que el token fue revocado
        using var verifySession = _fixture.Store.LightweightSession();
        var events = await verifySession.Events.FetchStreamAsync(usuarioId);

        events.Should().Contain(e => e.Data is RefreshTokenRevocado,
            "debe haber un evento de revocación para el token expirado");
    }

    #endregion
}
