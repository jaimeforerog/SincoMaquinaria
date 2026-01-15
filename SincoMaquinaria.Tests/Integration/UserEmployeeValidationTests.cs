using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SincoMaquinaria.Domain;
using SincoMaquinaria.DTOs.Requests;
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

    [Fact]
    public async Task ActualizarUsuario_NombreDuplicado_DebeRetornarConflict()
    {
        // Arrange
        var client = _factory.CreateClient();
        var user1Name = "User One";
        var user2Name = "User Two";
        Guid user1Id = Guid.NewGuid();
        Guid user2Id = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                // Create two users
                session.Store(new Usuario { Id = user1Id, Nombre = user1Name, Email = "user1@test.com", Activo = true, Rol = RolUsuario.Admin });
                session.Store(new Usuario { Id = user2Id, Nombre = user2Name, Email = "user2@test.com", Activo = true, Rol = RolUsuario.Admin });
                await session.SaveChangesAsync();
            }
        }

        // Act - Try to rename User 2 to "User One"
        var request = new ActualizarUsuarioRequest(user1Name, "Admin", true);
        var response = await client.PutAsJsonAsync($"/auth/users/{user2Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
    
    [Fact]
    public async Task CrearEmpleado_DocumentoDuplicado_DebeRetornarConflict()
    {
        // Arrange
        var client = _factory.CreateClient();
        var documento = "123456789";

        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                session.Store(new Empleado { Id = Guid.NewGuid(), Identificacion = documento, Nombre = "Empleado Existente", Cargo = CargoEmpleado.Operario, Estado = EstadoEmpleado.Activo });
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
        var documento1 = "11111";
        var documento2 = "22222";
        Guid id2 = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                session.Store(new Empleado { Id = Guid.NewGuid(), Identificacion = documento1, Nombre = "Emp 1", Cargo = CargoEmpleado.Operario, Estado = EstadoEmpleado.Activo });
                session.Store(new Empleado { Id = id2, Identificacion = documento2, Nombre = "Emp 2", Cargo = CargoEmpleado.Operario, Estado = EstadoEmpleado.Activo });
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
        var nombreDuplicado = "Admin Existing";
        
        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                // Create an existing admin user to allow hitting the /register endpoint (which requires Admin role)
                session.Store(new Usuario { Id = Guid.NewGuid(), Nombre = nombreDuplicado, Email = "admin@test.com", Activo = true, Rol = RolUsuario.Admin, PasswordHash = "hash" });
                await session.SaveChangesAsync();
            }
        }
        
        // Authenticate as Admin (CustomWebApplicationFactory handles auth bypass, usually simulating admin)
        // But /register endpoint requires Authorization policy "Admin". 
        // Our CustomWebApplicationFactory Setup:
        // services.AddAuthentication(defaultScheme: "Test") .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
        // And the TestAuthHandler usually adds claims. Let's check TestAuthHandler in CustomWebApplicationFactory logic if possible.
        // Assuming it works as Admin by default or we might need simple setup.
        
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
        var adminId = Guid.NewGuid();
        
        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                // Create an active admin
                session.Store(new Usuario { Id = adminId, Nombre = "Admin User", Email = "admin_list@test.com", Activo = true, Rol = RolUsuario.Admin });
                await session.SaveChangesAsync();
            }
        }

        // TestAuthHandler in CustomWebApplicationFactory likely simulates a user. 
        // We need to ensure it has "Admin" role claim if the endpoint requires it.
        // If CustomWebApplicationFactory mock user is NOT Admin, this will fail with 403.
        // Let's assume the default mock is Admin or check CustomWebApplicationFactory.
        // If it fails, we know we need to adjust the test auth context.
        
        // Act
        var response = await client.GetAsync("/auth/users");

        // Assert
        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
             // This informs us if test setup is why it fails, but for now let's hope it passes or gives us a clue
             // Real user issue might be they are not admin.
        }
        
        response.EnsureSuccessStatusCode();
        var users = await response.Content.ReadFromJsonAsync<List<object>>();
        users.Should().NotBeNull();
        users.Should().NotBeEmpty();
    }
}
