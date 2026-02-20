using Marten;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events;
using SincoMaquinaria.DTOs.Requests;

namespace SincoMaquinaria.Services;

public class AuthService
{
    private readonly IDocumentSession _session;
    private readonly JwtService _jwtService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IDocumentSession session, JwtService jwtService, ILogger<AuthService> logger)
    {
        _session = session;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Login(LoginRequest req)
    {
        var usuario = await _session.Query<Usuario>()
            .FirstOrDefaultAsync(u => u.Email == req.Email && u.Activo);

        if (usuario == null || !JwtService.VerifyPassword(req.Password, usuario.PasswordHash))
        {
            _logger.LogWarning("Login fallido para email {Email}", req.Email);
            return Result<AuthResponse>.Unauthorized();
        }

        // Generar access token y refresh token
        var (token, expiration, refreshToken, refreshExpiry) = _jwtService.GenerateTokens(usuario);

        // Use exclusive write lock to safely append to the user stream
        var stream = await _session.Events.FetchForExclusiveWriting<Usuario>(usuario.Id);

        // Guardar refresh token en BD
        stream.AppendOne(new RefreshTokenGenerado(
            usuario.Id, refreshToken, refreshExpiry, DateTimeOffset.UtcNow));

        await _session.SaveChangesAsync();

        _logger.LogInformation("Login exitoso para usuario {UserId} ({Email})", usuario.Id, usuario.Email);
        return Result<AuthResponse>.Success(new AuthResponse(
            token,
            expiration,
            refreshToken,
            refreshExpiry,
            usuario.Id,
            usuario.Email,
            usuario.Nombre,
            usuario.Rol.ToString()
        ));
    }

    public async Task<Result<Guid>> Register(RegisterRequest req)
    {
        // Verificar si el email ya existe
        var existingByEmail = await _session.Query<Usuario>()
            .FirstOrDefaultAsync(u => u.Email == req.Email);

        if (existingByEmail != null)
        {
            return Result<Guid>.Failure("El correo electrónico ya está registrado");
        }

        // Verificar si el nombre completo ya existe
        var existingByName = await _session.Query<Usuario>()
            .FirstOrDefaultAsync(u => u.Nombre.ToLower() == req.Nombre.ToLower());

        if (existingByName != null)
        {
            return Result<Guid>.Failure("Ya existe un usuario con ese nombre completo");
        }

        var usuarioId = Guid.NewGuid();
        var passwordHash = JwtService.HashPassword(req.Password);

        if (!Enum.TryParse<RolUsuario>(req.Rol, true, out var rolUsuario))
        {
            rolUsuario = RolUsuario.User;
        }

        _session.Events.StartStream<Usuario>(usuarioId,
            new UsuarioCreado(usuarioId, req.Email, passwordHash, req.Nombre,
                rolUsuario, DateTime.UtcNow));

        await _session.SaveChangesAsync();

        _logger.LogInformation("Usuario {UserId} registrado ({Email}, rol: {Rol})", usuarioId, req.Email, rolUsuario);
        return Result<Guid>.Success(usuarioId);
    }

    public async Task<Result<Guid>> SetupAdmin(RegisterRequest req)
    {
        // Verificar si ya existen usuarios
        var existingUsers = await _session.Query<Usuario>().AnyAsync();
        if (existingUsers)
        {
            return Result<Guid>.Failure("Ya existen usuarios en el sistema. Use /auth/register");
        }

        var usuarioId = Guid.NewGuid();
        var passwordHash = JwtService.HashPassword(req.Password);

        _session.Events.StartStream<Usuario>(usuarioId,
            new UsuarioCreado(usuarioId, req.Email, passwordHash, req.Nombre, 
                RolUsuario.Admin, DateTime.UtcNow));

        await _session.SaveChangesAsync();

        return Result<Guid>.Success(usuarioId);
    }

    public async Task<Result<Unit>> ActualizarUsuario(Guid id, ActualizarUsuarioRequest req, Guid currentUserId, string currentUserName)
    {
        var usuario = await _session.LoadAsync<Usuario>(id);
        if (usuario == null) 
        {
            return Result<Unit>.NotFound("Usuario no encontrado");
        }

        string? newPasswordHash = null;
        if (!string.IsNullOrEmpty(req.Password))
        {
            newPasswordHash = JwtService.HashPassword(req.Password);
        }

        if (!Enum.TryParse<RolUsuario>(req.Rol, true, out var rolUsuario))
        {
             return Result<Unit>.Failure("Rol inválido");
        }
        
        // Validar unicidad de nombre (excluyendo el usuario actual)
        var existingUserByName = await _session.Query<Usuario>()
            .AnyAsync(u => u.Id != id && u.Nombre.Equals(req.Nombre, StringComparison.CurrentCultureIgnoreCase) && u.Activo);

        if (existingUserByName)
        {
            return Result<Unit>.Failure($"Ya existe otro usuario con el nombre '{req.Nombre}'");
        }
        
        _session.Events.Append(id, 
            new UsuarioActualizado(id, req.Nombre, rolUsuario, req.Activo, newPasswordHash, currentUserId, currentUserName, DateTimeOffset.Now));

        await _session.SaveChangesAsync();

        return Result<Unit>.Success(Unit.Value);
    }

    public async Task<Result<AuthResponse>> RefreshToken(string refreshToken)
    {
        var usuario = await _session.Query<Usuario>()
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && u.Activo);

        if (usuario == null || usuario.RefreshTokenExpiry == null ||
            usuario.RefreshTokenExpiry < DateTime.UtcNow)
        {
            return Result<AuthResponse>.Unauthorized();
        }

        // Use exclusive write lock to prevent concurrent version conflicts
        // (multiple refresh requests for the same user at the same time)
        var stream = await _session.Events.FetchForExclusiveWriting<Usuario>(usuario.Id);

        // Generar nuevos tokens
        var (newToken, expiration, newRefreshToken, refreshExpiry) =
            _jwtService.GenerateTokens(usuario);

        // Append via the locked stream to avoid duplicate version
        stream.AppendOne(new RefreshTokenGenerado(
            usuario.Id, newRefreshToken, refreshExpiry, DateTimeOffset.UtcNow));

        await _session.SaveChangesAsync();

        return Result<AuthResponse>.Success(new AuthResponse(
            newToken,
            expiration,
            newRefreshToken,
            refreshExpiry,
            usuario.Id,
            usuario.Email,
            usuario.Nombre,
            usuario.Rol.ToString()
        ));
    }

    public async Task<Result<bool>> Logout(Guid usuarioId)
    {
        var stream = await _session.Events.FetchForExclusiveWriting<Usuario>(usuarioId);

        stream.AppendOne(new RefreshTokenRevocado(
            usuarioId, DateTimeOffset.UtcNow));

        await _session.SaveChangesAsync();
        return Result<bool>.Success(true);
    }
}
