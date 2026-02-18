using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SincoMaquinaria.Domain;
using SincoMaquinaria.DTOs.Requests;
using SincoMaquinaria.DTOs.Common;
using Marten;
using Xunit;
using SincoMaquinaria.Tests.Helpers;

namespace SincoMaquinaria.Tests.Integration;

[Collection("Integration")]
public class RutinasEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RutinasEndpointsTests(CustomWebApplicationFactory factory)
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

    #region Listar Rutinas Tests

    [Fact]
    public async Task ListarRutinas_DebeRetornarListaPaginada()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Seed rutinas
        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                for (int i = 1; i <= 5; i++)
                {
                    session.Store(new RutinaMantenimiento
                    {
                        Id = Guid.NewGuid(),
                        Descripcion = $"Rutina Test {i}",
                        Grupo = "Test"
                    });
                }
                await session.SaveChangesAsync();
            }
        }

        // Authenticate
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await client.GetAsync("/rutinas?page=1&pageSize=3");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResult = await response.Content.ReadFromJsonAsync<PagedResponse<object>>();
        pagedResult.Should().NotBeNull();
        pagedResult!.Data.Should().HaveCountGreaterOrEqualTo(3);
        pagedResult.Page.Should().Be(1);
        pagedResult.PageSize.Should().Be(3);
    }

    #endregion

    #region Obtener Rutina Tests

    [Fact]
    public async Task ObtenerRutina_CuandoExiste_DebeRetornarOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        var rutinaId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                session.Store(new RutinaMantenimiento
                {
                    Id = rutinaId,
                    Descripcion = "Rutina Específica",
                    Grupo = "Test"
                });
                await session.SaveChangesAsync();
            }
        }

        // Authenticate
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await client.GetAsync($"/rutinas/{rutinaId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var rutina = await response.Content.ReadFromJsonAsync<RutinaMantenimiento>();
        rutina.Should().NotBeNull();
        rutina!.Id.Should().Be(rutinaId);
        rutina.Descripcion.Should().Be("Rutina Específica");
    }

    [Fact]
    public async Task ObtenerRutina_CuandoNoExiste_DebeRetornarNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var rutinaIdInexistente = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/rutinas/{rutinaIdInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Crear Rutina Tests

    [Fact]
    public async Task CrearRutina_ConDatosValidos_DebeRetornarCreated()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var request = new CreateRutinaRequest("Nueva Rutina Test", "Grupo Test");

        // Act
        var response = await client.PostAsJsonAsync("/rutinas", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var rutina = await response.Content.ReadFromJsonAsync<RutinaMantenimiento>();
        rutina.Should().NotBeNull();
        rutina!.Descripcion.Should().Be("Nueva Rutina Test");
    }

    [Fact]
    public async Task CrearRutina_ConDescripcionVacia_DebeRetornarBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var request = new CreateRutinaRequest("", "Grupo Test");

        // Act
        var response = await client.PostAsJsonAsync("/rutinas", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Actualizar Rutina Tests

    [Fact]
    public async Task ActualizarRutina_ConDatosValidos_DebeRetornarOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        var rutinaId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                session.Store(new RutinaMantenimiento
                {
                    Id = rutinaId,
                    Descripcion = "Rutina Original",
                    Grupo = "Original"
                });
                await session.SaveChangesAsync();
            }
        }

        // Authenticate
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var request = new UpdateRutinaRequest("Rutina Actualizada", "Grupo Actualizado");

        // Act
        var response = await client.PutAsJsonAsync($"/rutinas/{rutinaId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var rutina = await response.Content.ReadFromJsonAsync<RutinaMantenimiento>();
        rutina.Should().NotBeNull();
        rutina!.Descripcion.Should().Be("Rutina Actualizada");
    }

    [Fact]
    public async Task ActualizarRutina_CuandoNoExiste_DebeRetornarNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var rutinaIdInexistente = Guid.NewGuid();
        var request = new UpdateRutinaRequest("Rutina", "Grupo");

        // Act
        var response = await client.PutAsJsonAsync($"/rutinas/{rutinaIdInexistente}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Agregar Parte Tests

    [Fact]
    public async Task AgregarParte_ARutinaExistente_DebeRetornarCreated()
    {
        // Arrange
        var client = _factory.CreateClient();
        var rutinaId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                session.Store(new RutinaMantenimiento
                {
                    Id = rutinaId,
                    Descripcion = "Rutina con Partes",
                    Grupo = "Test"
                });
                await session.SaveChangesAsync();
            }
        }

        // Authenticate
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var request = new AddParteRequest("Nueva Parte Test");

        // Act
        var response = await client.PostAsJsonAsync($"/rutinas/{rutinaId}/partes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task AgregarParte_ARutinaInexistente_DebeRetornarNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var rutinaIdInexistente = Guid.NewGuid();
        var request = new AddParteRequest("Parte Test");

        // Act
        var response = await client.PostAsJsonAsync($"/rutinas/{rutinaIdInexistente}/partes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Actualizar Parte Tests

    [Fact]
    public async Task ActualizarParte_Existente_DebeRetornarOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        var rutinaId = Guid.NewGuid();
        var parteId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                var rutina = new RutinaMantenimiento
                {
                    Id = rutinaId,
                    Descripcion = "Rutina",
                    Grupo = "Test"
                };
                rutina.Partes.Add(new ParteEquipo
                {
                    Id = parteId,
                    Descripcion = "Parte Original"
                });
                session.Store(rutina);
                await session.SaveChangesAsync();
            }
        }

        // Authenticate
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var request = new UpdateParteRequest("Parte Actualizada");

        // Act
        var response = await client.PutAsJsonAsync($"/rutinas/{rutinaId}/partes/{parteId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ActualizarParte_ParteInexistente_DebeRetornarNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var rutinaId = Guid.NewGuid();
        var parteIdInexistente = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                session.Store(new RutinaMantenimiento
                {
                    Id = rutinaId,
                    Descripcion = "Rutina",
                    Grupo = "Test"
                });
                await session.SaveChangesAsync();
            }
        }

        // Authenticate
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var request = new UpdateParteRequest("Parte");

        // Act
        var response = await client.PutAsJsonAsync($"/rutinas/{rutinaId}/partes/{parteIdInexistente}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Eliminar Parte Tests

    [Fact]
    public async Task EliminarParte_Existente_DebeRetornarNoContent()
    {
        // Arrange
        var client = _factory.CreateClient();
        var rutinaId = Guid.NewGuid();
        var parteId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                var rutina = new RutinaMantenimiento
                {
                    Id = rutinaId,
                    Descripcion = "Rutina",
                    Grupo = "Test"
                };
                rutina.Partes.Add(new ParteEquipo
                {
                    Id = parteId,
                    Descripcion = "Parte a Eliminar"
                });
                session.Store(rutina);
                await session.SaveChangesAsync();
            }
        }

        // Authenticate
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await client.DeleteAsync($"/rutinas/{rutinaId}/partes/{parteId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task EliminarParte_ParteInexistente_DebeRetornarNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var rutinaId = Guid.NewGuid();
        var parteIdInexistente = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                session.Store(new RutinaMantenimiento
                {
                    Id = rutinaId,
                    Descripcion = "Rutina",
                    Grupo = "Test"
                });
                await session.SaveChangesAsync();
            }
        }

        // Authenticate
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await client.DeleteAsync($"/rutinas/{rutinaId}/partes/{parteIdInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Agregar Actividad Tests

    [Fact]
    public async Task AgregarActividad_AParteExistente_DebeRetornarCreated()
    {
        // Arrange
        var client = _factory.CreateClient();
        var rutinaId = Guid.NewGuid();
        var parteId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                var rutina = new RutinaMantenimiento
                {
                    Id = rutinaId,
                    Descripcion = "Rutina",
                    Grupo = "Test"
                };
                rutina.Partes.Add(new ParteEquipo
                {
                    Id = parteId,
                    Descripcion = "Parte"
                });
                session.Store(rutina);
                await session.SaveChangesAsync();
            }
        }

        // Authenticate
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var request = new AddActividadRequest(
            "Actividad Test",
            "Inspección",
            1000,
            "km",
            "Kilometraje",
            100,
            0,
            "",
            "",
            0,
            null,
            0
        );

        // Act
        var response = await client.PostAsJsonAsync($"/rutinas/{rutinaId}/partes/{parteId}/actividades", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task AgregarActividad_AParteInexistente_DebeRetornarNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var rutinaId = Guid.NewGuid();
        var parteIdInexistente = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                session.Store(new RutinaMantenimiento
                {
                    Id = rutinaId,
                    Descripcion = "Rutina",
                    Grupo = "Test"
                });
                await session.SaveChangesAsync();
            }
        }

        // Authenticate
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var request = new AddActividadRequest(
            "Actividad",
            "Inspección",
            1000,
            "km",
            "Kilometraje",
            100,
            0,
            "",
            "",
            0,
            null,
            0
        );

        // Act
        var response = await client.PostAsJsonAsync($"/rutinas/{rutinaId}/partes/{parteIdInexistente}/actividades", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Actualizar Actividad Tests

    [Fact]
    public async Task ActualizarActividad_Existente_DebeRetornarOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        var rutinaId = Guid.NewGuid();
        var parteId = Guid.NewGuid();
        var actividadId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                var rutina = new RutinaMantenimiento
                {
                    Id = rutinaId,
                    Descripcion = "Rutina",
                    Grupo = "Test"
                };
                var parte = new ParteEquipo
                {
                    Id = parteId,
                    Descripcion = "Parte"
                };
                parte.Actividades.Add(new ActividadMantenimiento
                {
                    Id = actividadId,
                    Descripcion = "Actividad Original"
                });
                rutina.Partes.Add(parte);
                session.Store(rutina);
                await session.SaveChangesAsync();
            }
        }

        // Authenticate
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var request = new UpdateActividadRequest(
            "Actividad Actualizada",
            "Reparación",
            2000,
            "horas",
            "Horometro",
            200,
            0,
            "",
            "",
            0,
            null,
            0
        );

        // Act
        var response = await client.PutAsJsonAsync($"/rutinas/{rutinaId}/partes/{parteId}/actividades/{actividadId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ActualizarActividad_ActividadInexistente_DebeRetornarNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var rutinaId = Guid.NewGuid();
        var parteId = Guid.NewGuid();
        var actividadIdInexistente = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                var rutina = new RutinaMantenimiento
                {
                    Id = rutinaId,
                    Descripcion = "Rutina",
                    Grupo = "Test"
                };
                rutina.Partes.Add(new ParteEquipo
                {
                    Id = parteId,
                    Descripcion = "Parte"
                });
                session.Store(rutina);
                await session.SaveChangesAsync();
            }
        }

        // Authenticate
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var request = new UpdateActividadRequest(
            "Actividad",
            "Inspección",
            1000,
            "km",
            "Kilometraje",
            100,
            0,
            "",
            "",
            0,
            null,
            0
        );

        // Act
        var response = await client.PutAsJsonAsync($"/rutinas/{rutinaId}/partes/{parteId}/actividades/{actividadIdInexistente}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Eliminar Actividad Tests

    [Fact]
    public async Task EliminarActividad_Existente_DebeRetornarNoContent()
    {
        // Arrange
        var client = _factory.CreateClient();
        var rutinaId = Guid.NewGuid();
        var parteId = Guid.NewGuid();
        var actividadId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                var rutina = new RutinaMantenimiento
                {
                    Id = rutinaId,
                    Descripcion = "Rutina",
                    Grupo = "Test"
                };
                var parte = new ParteEquipo
                {
                    Id = parteId,
                    Descripcion = "Parte"
                };
                parte.Actividades.Add(new ActividadMantenimiento
                {
                    Id = actividadId,
                    Descripcion = "Actividad a Eliminar"
                });
                rutina.Partes.Add(parte);
                session.Store(rutina);
                await session.SaveChangesAsync();
            }
        }

        // Authenticate
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await client.DeleteAsync($"/rutinas/{rutinaId}/partes/{parteId}/actividades/{actividadId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task EliminarActividad_ActividadInexistente_DebeRetornarNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var rutinaId = Guid.NewGuid();
        var parteId = Guid.NewGuid();
        var actividadIdInexistente = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                var rutina = new RutinaMantenimiento
                {
                    Id = rutinaId,
                    Descripcion = "Rutina",
                    Grupo = "Test"
                };
                rutina.Partes.Add(new ParteEquipo
                {
                    Id = parteId,
                    Descripcion = "Parte"
                });
                session.Store(rutina);
                await session.SaveChangesAsync();
            }
        }

        // Authenticate
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await client.DeleteAsync($"/rutinas/{rutinaId}/partes/{parteId}/actividades/{actividadIdInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Descargar Plantilla Tests

    [Fact]
    public async Task DescargarPlantilla_DebeRetornarArchivoExcel()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await client.GetAsync("/rutinas/plantilla");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        var content = await response.Content.ReadAsByteArrayAsync();
        content.Should().NotBeEmpty();
    }

    #endregion
}
