using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SincoMaquinaria.Extensions;

public static class HttpContextExtensions
{
    public static (Guid? UserId, string? UserName) GetUserContext(this HttpContext context)
    {
        var sub = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                  ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var name = context.User.FindFirst(ClaimTypes.Name)?.Value
                   ?? context.User.FindFirst("name")?.Value;

        Guid? uid = null;
        if (Guid.TryParse(sub, out var g)) uid = g;

        return (uid, name);
    }

    public static IResult? ValidateFileUpload(IFormFile? file, int maxSizeMB)
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest("No file uploaded");

        if (file.Length > (long)maxSizeMB * 1024 * 1024)
            return Results.BadRequest($"El archivo excede el tamaño máximo permitido de {maxSizeMB} MB");

        var allowedExtensions = new[] { ".xlsx", ".xls" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return Results.BadRequest($"Tipo de archivo no permitido. Solo se aceptan archivos: {string.Join(", ", allowedExtensions)}");

        return null;
    }
}
