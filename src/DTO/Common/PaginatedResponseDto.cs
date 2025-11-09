using System.Collections.Generic;

namespace DTO.Common;

public class PaginatedResponseDto<T>
{
    public IReadOnlyCollection<T> Items { get; init; } = new List<T>().AsReadOnly();

    public int TotalCount { get; init; }

    public int PageNumber { get; init; }

    public int PageSize { get; init; }

    public int TotalPages { get; init; }

    public bool HasPrevious { get; init; }

    public bool HasNext { get; init; }
}

