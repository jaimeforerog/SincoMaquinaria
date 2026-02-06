using Marten;
using Marten.Pagination;
using SincoMaquinaria.DTOs.Common;

namespace SincoMaquinaria.Extensions;

public static class PaginationExtensions
{
    /// <summary>
    /// Aplica paginación a una query de Marten
    /// </summary>
    public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(
        this IQueryable<T> query,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
        where T : notnull
    {
        // Contar total de elementos
        var totalCount = await query.CountAsync(cancellationToken);

        // Obtener datos paginados
        var data = await query
            .Skip(pagination.GetOffset())
            .Take(pagination.GetPageSize())
            .ToListAsync(cancellationToken);

        return PagedResponse<T>.Create(data, pagination, totalCount);
    }

    /// <summary>
    /// Aplica ordenamiento dinámico a una query
    /// </summary>
    public static IQueryable<T> ApplyOrdering<T>(
        this IQueryable<T> query,
        PaginationRequest pagination)
    {
        if (string.IsNullOrWhiteSpace(pagination.OrderBy))
            return query;

        // Obtener la propiedad para ordenar
        var propertyInfo = typeof(T).GetProperty(pagination.OrderBy);
        if (propertyInfo == null)
            return query; // Si la propiedad no existe, ignorar ordenamiento

        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T), "x");
        var property = System.Linq.Expressions.Expression.Property(parameter, propertyInfo);
        var lambda = System.Linq.Expressions.Expression.Lambda(property, parameter);

        var methodName = pagination.IsDescending() ? "OrderByDescending" : "OrderBy";
        var resultExpression = System.Linq.Expressions.Expression.Call(
            typeof(Queryable),
            methodName,
            new Type[] { typeof(T), propertyInfo.PropertyType },
            query.Expression,
            System.Linq.Expressions.Expression.Quote(lambda));

        return query.Provider.CreateQuery<T>(resultExpression);
    }
}