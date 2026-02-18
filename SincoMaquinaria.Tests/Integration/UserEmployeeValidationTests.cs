using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events.Usuario;
using SincoMaquinaria.Domain.Events.Empleado;
using SincoMaquinaria.DTOs.Requests;
using SincoMaquinaria.Services;
using SincoMaquinaria.Tests.Helpers;
using Marten;
using Xunit;

namespace SincoMaquinaria.Tests.Integration;

[Collection("Integration")]
public class UserEmployeeValidationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public UserEmployeeValidationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<string> GetAdminToken(HttpClient client)
    {
        client.DefaultRequestHeaders.Clear();
        var loginRequest = new LoginRequest("admin@test.com", "Admin123!");
        var response = await client.PostAsJsonAsync("/auth/login", loginRequest);
        var authResponse = await response.Content.ReadFromJsonAsync<TestAuthResponse>();
        return authResponse!.Token;
    }

    [Fact]
    public async Task ActualizarUsuario_NombreDuplicado_DebeRetornarConflict()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var user1Name = "User One";
        var user2Name = "User Two";
        Guid user1Id = Guid.NewGuid();
        Guid user2Id = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                // Create two users using event sourcing
                var passwordHash = JwtService.HashPassword("Test123!");

                session.Events.StartStream<Usuario>(user1Id,
                    new UsuarioCreado(user1Id, "user1@test.com", passwordHash, user1Name, RolUsuario.Admin, DateTime.UtcNow));

                session.Events.StartStream<Usuario>(user2Id,
                    new UsuarioCreado(user2Id, "user2@test.com", passwordHash, user2Name, RolUsuario.Admin, DateTime.UtcNow));

                await session.SaveChangesAsync();
            }
        }

        // Act - Try to rename User 2 to "User One"
        var request = new ActualizarUsuarioRequest(user1Name, "Admin", true, null);
        var response = await client.PutAsJsonAsync($"/auth/users/{user2Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
    
    [Fact]
    public async Task CrearEmpleado_DocumentoDuplicado_DebeRetornarConflict()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var documento = "123456789";
        var empleadoId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                // Create empleado using event sourcing
                session.Events.StartStream<Empleado>(empleadoId,
                    new EmpleadoCreado(empleadoId, "Empleado Existente", documento, "Operario", "", 5000, "Activo", Guid.NewGuid(), "Admin Test", DateTime.UtcNow));

                await session.SaveChangesAsync();
            }
        }

        // Act
        var request = new CrearEmpleadoRequest("Nuevo Empleado", documento, "Operario", "Soldador", 5000, "Activo");
        var response = await client.PostAsJsonAsync("/empleados", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ActualizarEmpleado_DocumentoDuplicado_DebeRetornarConflict()
    {
         // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var documento1 = "11111";
        var documento2 = "22222";
        Guid id1 = Guid.NewGuid();
        Guid id2 = Guid.NewGuid();
        var userId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                // Create empleados using event sourcing
                session.Events.StartStream<Empleado>(id1,
                    new EmpleadoCreado(id1, "Emp 1", documento1, "Operario", "", 5000, "Activo", userId, "Admin Test", DateTime.UtcNow));

                session.Events.StartStream<Empleado>(id2,
                    new EmpleadoCreado(id2, "Emp 2", documento2, "Operario", "", 5000, "Activo", userId, "Admin Test", DateTime.UtcNow));

                await session.SaveChangesAsync();
            }
        }

        // Act - Try to change Emp 2's document to "11111"
        var request = new ActualizarEmpleadoRequest("Emp 2", documento1, "Operario", "Soldador", 5000, "Activo");
        var response = await client.PutAsJsonAsync($"/empleados/{id2}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_NombreDuplicado_DebeRetornarConflict()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var nombreDuplicado = "Admin Existing";
        var userId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                // Create an existing admin user using event sourcing
                var passwordHash = JwtService.HashPassword("Test123!");

                session.Events.StartStream<Usuario>(userId,
                    new UsuarioCreado(userId, "adminexisting@test.com", passwordHash, nombreDuplicado, RolUsuario.Admin, DateTime.UtcNow));

                await session.SaveChangesAsync();
            }
        }

        // Act
        var request = new RegisterRequest("new@test.com", "password123", nombreDuplicado, "User");
        var response = await client.PostAsJsonAsync("/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetAllUsers_ComoAdmin_DebeRetornarLista()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var adminId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                // Create an active admin using event sourcing
                var passwordHash = JwtService.HashPassword("Test123!");

                session.Events.StartStream<Usuario>(adminId,
                    new UsuarioCreado(adminId, "admin_list@test.com", passwordHash, "Admin User List", RolUsuario.Admin, DateTime.UtcNow));

                await session.SaveChangesAsync();
            }
        }

        // Act
        var response = await client.GetAsync("/auth/users");

        // Assert
        response.EnsureSuccessStatusCode();
        var users = await response.Content.ReadFromJsonAsync<List<object>>();
        users.Should().NotBeNull();
        users.Should().NotBeEmpty();
    }
}
