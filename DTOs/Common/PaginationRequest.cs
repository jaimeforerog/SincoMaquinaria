namespace SincoMaquinaria.DTOs.Common;

/// <summary>
/// DTO para solicitudes de paginación
/// </summary>
public class PaginationRequest
{
    /// <summary>
    /// Número de página (1-indexed)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Tamaño de página (cantidad de elementos por página)
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Campo por el cual ordenar (opcional)
    /// </summary>
    public string? OrderBy { get; set; }

    /// <summary>
    /// Dirección de ordenamiento (asc o desc)
    /// </summary>
    public string OrderDirection { get; set; } = "asc";

    /// <summary>
    /// Calcular el offset para la consulta
    /// </summary>
    public int GetOffset() => (Page - 1) * PageSize;

    /// <summary>
    /// Validar si la dirección de ordenamiento es válida
    /// </summary>
    public bool IsDescending() => OrderDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
}