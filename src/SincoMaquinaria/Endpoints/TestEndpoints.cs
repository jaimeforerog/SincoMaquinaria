using Marten;
using Microsoft.AspNetCore.Mvc;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Services;
using SincoMaquinaria.DTOs.Requests;

namespace SincoMaquinaria.Endpoints;

/// <summary>
/// Endpoints para facilitar E2E testing - SOLO DISPONIBLES EN DEVELOPMENT
/// </summary>
public static class TestEndpoints
{
    public static WebApplication MapTestEndpoints(this WebApplication app)
    {
        // Solo habilitar en Development
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        var group = app.MapGroup("/test")
            .WithTags("Testing (Development Only)")
            .WithDescription("Endpoints de utilidad para E2E testing - solo disponibles en Development");

        group.MapPost("/reset-data", ResetTestData)
            .WithDescription("Elimina todos los usuarios y crea el usuario de prueba E2E");

        group.MapPost("/seed-test-user", SeedTestUser)
            .WithDescription("Crea el usuario de prueba E2E si no existe");

        group.MapDelete("/clear-all-data", ClearAllData)
            .WithDescription("PELIGRO: Elimina TODOS los datos de la base de datos");

        return app;
    }

    /// <summary>
    /// Elimina todos los usuarios y crea el usuario de prueba E2E
    /// </summary>
    private static async Task<IResult> ResetTestData(
        IDocumentStore store,
        AuthService authService)
    {
        try
        {
            await using var session = store.LightweightSession();

            // Eliminar todos los usuarios
            var usuarios = await session.Query<Usuario>().ToListAsync();
            foreach (var usuario in usuarios)
            {
                session.Delete(usuario);
            }
            await session.SaveChangesAsync();

            // Crear usuario de prueba E2E
            var testUserRequest = new RegisterRequest(
                Email: "e2e-test@sinco.com",
                Nombre: "E2E Test Admin",
                Password: "TestPassword123"
            );

            var result = await authService.SetupAdmin(testUserRequest);

            if (!result.IsSuccess)
            {
                return Results.Problem($"Error creando usuario de prueba: {result.Error}");
            }

            return Results.Ok(new
            {
                Message = "Base de datos reiniciada para testing",
                UsuariosEliminados = usuarios.Count,
                TestUserCreated = new
                {
                    Email = "e2e-test@sinco.com",
                    Password = "TestPassword123",
                    Nombre = "E2E Test Admin"
                }
            });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error reiniciando datos de prueba: {ex.Message}");
        }
    }

    /// <summary>
    /// Crea el usuario de prueba E2E si no existe
    /// </summary>
    private static async Task<IResult> SeedTestUser(
        IDocumentStore store,
        AuthService authService)
    {
        try
        {
            await using var session = store.LightweightSession();

            // Verificar si ya existe
            var existingUser = await session.Query<Usuario>()
                .FirstOrDefaultAsync(u => u.Email == "e2e-test@sinco.com");

            if (existingUser != null)
            {
                return Results.Ok(new
                {
                    Message = "Usuario de prueba ya existe",
                    Email = "e2e-test@sinco.com"
                });
            }

            // Verificar si hay otros usuarios (para usar Setup o Register)
            var userCount = await session.Query<Usuario>().CountAsync();

            var testUserRequest = new RegisterRequest(
                Email: "e2e-test@sinco.com",
                Nombre: "E2E Test Admin",
                Password: "TestPassword123"
            );

            Result<Guid> result;

            if (userCount == 0)
            {
                // No hay usuarios, usar SetupAdmin
                result = await authService.SetupAdmin(testUserRequest);
            }
            else
            {
                // Ya hay usuarios, crear como admin directamente usando event sourcing
                await using var adminSession = store.LightweightSession();
                var usuarioId = Guid.NewGuid();
                var passwordHash = JwtService.HashPassword("TestPassword123");

                adminSession.Events.StartStream<Usuario>(usuarioId,
                    new Domain.Events.UsuarioCreado(
                        usuarioId,
                        "e2e-test@sinco.com",
                        passwordHash,
                        "E2E Test Admin",
                        RolUsuario.Admin,
                        DateTime.UtcNow));

                await adminSession.SaveChangesAsync();
                result = Result<Guid>.Success(usuarioId);
            }

            if (!result.IsSuccess)
            {
                return Results.Problem($"Error creando usuario de prueba: {result.Error}");
            }

            return Results.Ok(new
            {
                Message = "Usuario de prueba creado exitosamente",
                Email = "e2e-test@sinco.com",
                Password = "TestPassword123",
                Nombre = "E2E Test Admin",
                UserId = result.Value
            });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error creando usuario de prueba: {ex.Message}");
        }
    }

    /// <summary>
    /// PELIGRO: Elimina TODOS los datos de la base de datos
    /// </summary>
    private static async Task<IResult> ClearAllData(IDocumentStore store)
    {
        try
        {
            await using var session = store.LightweightSession();

            // Eliminar todos los usuarios
            var usuarios = await session.Query<Usuario>().ToListAsync();
            foreach (var usuario in usuarios)
            {
                session.Delete(usuario);
            }

            // Aquí podrías agregar más entidades si es necesario
            // var equipos = await session.Query<Equipo>().ToListAsync();
            // foreach (var equipo in equipos) session.Delete(equipo);

            await session.SaveChangesAsync();

            return Results.Ok(new
            {
                Message = "TODOS los datos han sido eliminados",
                UsuariosEliminados = usuarios.Count
            });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error eliminando datos: {ex.Message}");
        }
    }
}
