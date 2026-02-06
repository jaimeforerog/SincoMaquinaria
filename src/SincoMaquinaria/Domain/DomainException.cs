namespace SincoMaquinaria.Domain;

/// <summary>
/// Exception for domain validation errors
/// </summary>
public class DomainException : Exception
{
    public string[] Errors { get; }

    public DomainException(string message) : base(message)
    {
        Errors = new[] { message };
    }

    public DomainException(string[] errors) : base(string.Join("; ", errors))
    {
        Errors = errors;
    }
}

/// <summary>
/// Domain validation helpers
/// </summary>
public static class DomainGuard
{
    public static void NotNullOrEmpty(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{fieldName} es requerido");
    }

    public static void NotEmpty(Guid value, string fieldName)
    {
        if (value == Guid.Empty)
            throw new DomainException($"{fieldName} es requerido");
    }

    public static void Positive(decimal value, string fieldName)
    {
        if (value < 0)
            throw new DomainException($"{fieldName} debe ser positivo");
    }

    public static void InRange(int value, int min, int max, string fieldName)
    {
        if (value < min || value > max)
            throw new DomainException($"{fieldName} debe estar entre {min} y {max}");
    }

    public static void ValidEnum<T>(string value, string fieldName) where T : struct, Enum
    {
        if (!Enum.TryParse<T>(value, ignoreCase: true, out _))
        {
            var validValues = string.Join(", ", Enum.GetNames<T>());
            throw new DomainException($"{fieldName} debe ser uno de: {validValues}");
        }
    }
}
