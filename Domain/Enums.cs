using System.ComponentModel;

namespace SincoMaquinaria.Domain;

/// <summary>
/// Estados del ciclo de vida de una Orden de Trabajo
/// </summary>
public enum EstadoOrdenDeTrabajo
{
    [Description("Inexistente")]
    Inexistente,

    [Description("Borrador")]
    Borrador,

    [Description("Programada")]
    Programada,

    [Description("En Ejecución")]
    EnEjecucion,

    [Description("Ejecución Completa")]
    EjecucionCompleta
}

/// <summary>
/// Estados de un detalle/actividad dentro de una orden de trabajo
/// </summary>
public enum EstadoDetalleOrden
{
    [Description("Pendiente")]
    Pendiente,

    [Description("En Proceso")]
    EnProceso,

    [Description("Finalizado")]
    Finalizado
}

/// <summary>
/// Cargos válidos para empleados
/// </summary>
public enum CargoEmpleado
{
    [Description("Conductor")]
    Conductor,

    [Description("Operario")]
    Operario,

    [Description("Mecánico")]
    Mecanico
}

/// <summary>
/// Estados de un empleado
/// </summary>
public enum EstadoEmpleado
{
    [Description("Activo")]
    Activo,

    [Description("Inactivo")]
    Inactivo
}

/// <summary>
/// Tipos de mantenimiento
/// </summary>
public enum TipoMantenimiento
{
    [Description("Preventivo")]
    Preventivo,

    [Description("Correctivo")]
    Correctivo
}

/// <summary>
/// Métodos de extensión para trabajar con enums y strings
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Convierte un enum a su representación en string (nombre del enum)
    /// </summary>
    public static string ToStringValue(this Enum value)
    {
        return value.ToString();
    }

    /// <summary>
    /// Convierte un string al enum correspondiente. Retorna el valor por defecto si falla.
    /// </summary>
    public static TEnum ToEnum<TEnum>(this string value) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return default;

        if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var result))
            return result;

        return default;
    }

    /// <summary>
    /// Valida si un string es un valor válido del enum
    /// </summary>
    public static bool IsValidEnum<TEnum>(this string value) where TEnum : struct, Enum
    {
        return Enum.TryParse<TEnum>(value, ignoreCase: true, out _);
    }

    /// <summary>
    /// Obtiene todos los valores posibles de un enum como lista de strings
    /// </summary>
    public static List<string> GetEnumValues<TEnum>() where TEnum : struct, Enum
    {
        return Enum.GetNames<TEnum>().ToList();
    }
}