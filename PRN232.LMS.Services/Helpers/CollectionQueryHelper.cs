using PRN232.LMS.Services.BusinessModels;

namespace PRN232.LMS.Services.Helpers;

public static class CollectionQueryHelper
{
    public static bool HasExpand(this CollectionQueryBusinessModel query, string expandName)
    {
        if (string.IsNullOrWhiteSpace(query.Expand))
        {
            return false;
        }

        return query.Expand
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(expand => expand.Equals(expandName, StringComparison.OrdinalIgnoreCase));
    }

    public static PagedResultBusinessModel<T> ToPagedResult<T>(this IEnumerable<T> source, CollectionQueryBusinessModel query)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;
        var items = source.ToList();
        var totalItems = items.Count;
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

        return new PagedResultBusinessModel<T>
        {
            Items = items
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }
}
