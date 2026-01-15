using Marten;
using Microsoft.AspNetCore.Mvc;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events;
using SincoMaquinaria.DTOs.Requests;
using SincoMaquinaria.Services;

namespace SincoMaquinaria.Endpoints;

public static class AuthEndpoints
{
    public static WebApplication MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth")
            .WithTags("Autenticación");

        group.MapPost("/login", Login);
        group.MapPost("/register", Register).RequireAuthorization("Admin");
        group.MapGet("/me", GetCurrentUser).RequireAuthorization();
        group.MapGet("/users", GetAllUsers).RequireAuthorization("Admin");
        group.MapPost("/setup", SetupAdmin); // Solo para crear el primer admin
        group.MapPut("/users/{id:guid}", ActualizarUsuario).RequireAuthorization("Admin");

        return app;
    }

    private static async Task<IResult> Login(
        IQuerySession session,
        JwtService jwtService,
        [FromBody] LoginRequest req)
    {
        var usuario = await session.Query<Usuario>()
            .FirstOrDefaultAsync(u => u.Email == req.Email && u.Activo);

        if (usuario == null || !JwtService.VerifyPassword(req.Password, usuario.PasswordHash))
        {
            return Results.Unauthorized();
        }

        var (token, expiration) = jwtService.GenerateToken(usuario);

        return Results.Ok(new AuthResponse(
            token, 
            expiration, 
            usuario.Email, 
            usuario.Nombre, 
            usuario.Rol.ToString()
        ));
    }

    private static async Task<IResult> Register(
        IDocumentSession session,
        [FromBody] RegisterRequest req)
    {
        // Verificar si el email ya existe
        var existingByEmail = await session.Query<Usuario>()
            .FirstOrDefaultAsync(u => u.Email == req.Email);

        if (existingByEmail != null)
        {
            return Results.Conflict("El correo electrónico ya está registrado");
        }

        // Verificar si el nombre completo ya existe
        var existingByName = await session.Query<Usuario>()
            .FirstOrDefaultAsync(u => u.Nombre.ToLower() == req.Nombre.ToLower());

        if (existingByName != null)
        {
            return Results.Conflict("Ya existe un usuario con ese nombre completo");
        }

        var usuarioId = Guid.NewGuid();
        var passwordHash = JwtService.HashPassword(req.Password);

        if (!Enum.TryParse<RolUsuario>(req.Rol, true, out var rolUsuario))
        {
            rolUsuario = RolUsuario.User;
        }

        session.Events.StartStream<Usuario>(usuarioId,
            new UsuarioCreado(usuarioId, req.Email, passwordHash, req.Nombre, 
                rolUsuario, DateTime.UtcNow));

        await session.SaveChangesAsync();

        return Results.Created($"/auth/users/{usuarioId}", new { Id = usuarioId });
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
        IDocumentSession session,
        [FromBody] RegisterRequest req)
    {
        // Verificar si ya existen usuarios
        var existingUsers = await session.Query<Usuario>().AnyAsync();
        if (existingUsers)
        {
            return Results.BadRequest("Ya existen usuarios en el sistema. Use /auth/register");
        }

        var usuarioId = Guid.NewGuid();
        var passwordHash = JwtService.HashPassword(req.Password);

        session.Events.StartStream<Usuario>(usuarioId,
            new UsuarioCreado(usuarioId, req.Email, passwordHash, req.Nombre, 
                RolUsuario.Admin, DateTime.UtcNow));

        await session.SaveChangesAsync();

        return Results.Created($"/auth/users/{usuarioId}", new { 
            Id = usuarioId,
            Message = "Administrador creado exitosamente"
        });
    }

    private static async Task<IResult> ActualizarUsuario(
        IDocumentSession session,
        HttpContext httpContext,
        Guid id,
        [FromBody] ActualizarUsuarioRequest req)
    {
        var currentUserId = GetCurrentUserId(httpContext);
        
        var usuario = await session.LoadAsync<Usuario>(id);
        if (usuario == null) return Results.NotFound();

        // Prevent self-deactivation or role downgrade if you are the last admin? 
        // For simplicity, just allow updates.
        
        string? newPasswordHash = null;
        if (!string.IsNullOrEmpty(req.Password))
        {
            newPasswordHash = JwtService.HashPassword(req.Password);
        }

        if (!Enum.TryParse<RolUsuario>(req.Rol, true, out var rolUsuario))
        {
             return Results.BadRequest("Rol inválido");
        }
        
        var currentUser = await session.LoadAsync<Usuario>(currentUserId);
        var currentUserName = currentUser?.Nombre ?? "Unknown";

        session.Events.Append(id, 
            new UsuarioActualizado(id, req.Nombre, rolUsuario, req.Activo, newPasswordHash, currentUserId, currentUserName, DateTimeOffset.Now));

        await session.SaveChangesAsync();
        return Results.Ok();
    }
    
    private static Guid GetCurrentUserId(HttpContext context)
    {
         var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
         if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var id)) return id;
         return Guid.Empty;
    }
}
