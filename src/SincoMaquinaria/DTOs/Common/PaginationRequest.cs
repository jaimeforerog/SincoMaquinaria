namespace SincoMaquinaria.DTOs.Common;

/// <summary>
/// DTO para solicitudes de paginación
/// </summary>
public class PaginationRequest
{
    private int? _page;
    private int? _pageSize;

    /// <summary>
    /// Número de página (1-indexed)
    /// </summary>
    public int? Page 
    { 
        get => _page; 
        set => _page = value; 
    }

    /// <summary>
    /// Tamaño de página (cantidad de elementos por página)
    /// </summary>
    public int? PageSize 
    { 
        get => _pageSize; 
        set => _pageSize = value; 
    }

    /// <summary>
    /// Campo por el cual ordenar (opcional)
    /// </summary>
    public string? OrderBy { get; set; }

    /// <summary>
    /// Dirección de ordenamiento (asc o desc)
    /// </summary>
    public string? OrderDirection { get; set; }

    /// <summary>
    /// Obtener el número de página efectivo (default: 1)
    /// </summary>
    public int GetPage() => _page ?? 1;

    /// <summary>
    /// Obtener el tamaño de página efectivo (default: 20)
    /// </summary>
    public int GetPageSize() => _pageSize ?? 20;

    /// <summary>
    /// Calcular el offset para la consulta
    /// </summary>
    public int GetOffset() => (GetPage() - 1) * GetPageSize();

    /// <summary>
    /// Validar si la dirección de ordenamiento es válida
    /// </summary>
    public bool IsDescending() => OrderDirection?.Equals("desc", StringComparison.OrdinalIgnoreCase) ?? false;
}