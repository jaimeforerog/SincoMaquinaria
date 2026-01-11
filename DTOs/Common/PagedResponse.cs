namespace SincoMaquinaria.DTOs.Common;

/// <summary>
/// Respuesta paginada genérica
/// </summary>
/// <typeparam name="T">Tipo de datos en la colección</typeparam>
public class PagedResponse<T>
{
    /// <summary>
    /// Datos de la página actual
    /// </summary>
    public IReadOnlyList<T> Data { get; set; } = new List<T>();

    /// <summary>
    /// Número de página actual (1-indexed)
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Tamaño de página
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total de elementos en la colección completa
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total de páginas disponibles
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Indica si hay una página anterior
    /// </summary>
    public bool HasPrevious => Page > 1;

    /// <summary>
    /// Indica si hay una página siguiente
    /// </summary>
    public bool HasNext => Page < TotalPages;

    /// <summary>
    /// Constructor vacío
    /// </summary>
    public PagedResponse() { }

    /// <summary>
    /// Constructor con datos
    /// </summary>
    public PagedResponse(IReadOnlyList<T> data, int page, int pageSize, int totalCount)
    {
        Data = data;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    /// <summary>
    /// Crea una respuesta paginada desde una lista
    /// </summary>
    public static PagedResponse<T> Create(IReadOnlyList<T> data, PaginationRequest request, int totalCount)
    {
        return new PagedResponse<T>
        {
            Data = data,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}