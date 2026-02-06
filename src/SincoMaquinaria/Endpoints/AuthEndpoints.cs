using Marten;
using Microsoft.AspNetCore.Mvc;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events;
using SincoMaquinaria.DTOs.Requests;
using SincoMaquinaria.Services;
using SincoMaquinaria.Extensions;

namespace SincoMaquinaria.Endpoints;

public static class AuthEndpoints
{
    public static WebApplication MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth")
            .WithTags("Autenticación");

        group.MapPost("/login", Login);

        group.MapPost("/register", Register)
            .RequireAuthorization("Admin");

        group.MapGet("/me", GetCurrentUser)
            .RequireAuthorization();

        group.MapGet("/users", GetAllUsers)
            .RequireAuthorization("Admin");

        group.MapPost("/setup", SetupAdmin);

        group.MapPut("/users/{id:guid}", ActualizarUsuario)
            .RequireAuthorization("Admin");

        group.MapPost("/refresh", RefreshToken);

        group.MapPost("/logout", Logout)
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> Login(
        AuthService service,
        [FromBody] LoginRequest req)
    {
        var result = await service.Login(req);

        if (result.IsUnauthorized)
        {
             return Results.Unauthorized();
        }

        if (!result.IsSuccess)
        {
            return Results.Problem(result.Error);
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> Register(
        AuthService service,
        [FromBody] RegisterRequest req)
    {
        var result = await service.Register(req);

        if (!result.IsSuccess)
        {
            return Results.Conflict(result.Error);
        }

        return Results.Created($"/auth/users/{result.Value}", new { Id = result.Value });
    }

    private static async Task<IResult> GetCurrentUser(
        IQuerySession session,
        HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Results.Unauthorized();
        }

        var usuario = await session.LoadAsync<Usuario>(userId);
        if (usuario == null || !usuario.Activo)
        {
            return Results.NotFound();
        }

        return Results.Ok(new
        {
            usuario.Id,
            usuario.Email,
            usuario.Nombre,
            Rol = usuario.Rol.ToString()
        });
    }

    private static async Task<IResult> GetAllUsers(IQuerySession session)
    {
        var usuarios = await session.Query<Usuario>()
            .Where(u => u.Activo)
            .ToListAsync();

        var result = usuarios.Select(u => new
        {
            u.Id,
            u.Email,
            u.Nombre,
            Rol = u.Rol.ToString(),
            u.Activo,
            u.FechaCreacion
        }).ToList();

        return Results.Ok(result);
    }

    /// <summary>
    /// Endpoint para crear el primer administrador (solo funciona si no hay usuarios)
    /// </summary>
    private static async Task<IResult> SetupAdmin(
        AuthService service,
        [FromBody] RegisterRequest req)
    {
        var result = await service.SetupAdmin(req);

        if (!result.IsSuccess)
        {
             return Results.BadRequest(result.Error);
        }

        return Results.Created($"/auth/users/{result.Value}", new { 
            Id = result.Value,
            Message = "Administrador creado exitosamente"
        });
    }

    private static async Task<IResult> ActualizarUsuario(
        AuthService service,
        HttpContext httpContext,
        Guid id,
        [FromBody] ActualizarUsuarioRequest req)
    {
        var (currentUserId, currentUserName) = httpContext.GetUserContext();

        var result = await service.ActualizarUsuario(id, req, currentUserId ?? Guid.Empty, currentUserName ?? "Unknown");
        
        if (result.IsNotFound) return Results.NotFound();
        
        if (!result.IsSuccess)
        {
             return Results.Conflict(result.Error);
        }

        return Results.Ok();
    }

    private static async Task<IResult> RefreshToken(
        AuthService authService,
        [FromBody] RefreshTokenRequest req)
    {
        var result = await authService.RefreshToken(req.RefreshToken);

        if (result.IsUnauthorized)
            return Results.Unauthorized();

        if (!result.IsSuccess)
            return Results.BadRequest(new { error = result.Error });

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> Logout(
        AuthService authService,
        HttpContext httpContext)
    {
        var (userId, _) = httpContext.GetUserContext();
        if (!userId.HasValue)
            return Results.Unauthorized();

        var result = await authService.Logout(userId.Value);
        return Results.Ok(new { message = "Sesión cerrada correctamente" });
    }

}
