using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Common.Models;

public sealed class PaginatedResult<T>
{
    public PaginatedResult(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than or equal to 1.");
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than or equal to 1.");
        }

        var materializedItems = items?.ToList() ?? new List<T>();

        Items = materializedItems.AsReadOnly();
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    public IReadOnlyCollection<T> Items { get; }

    public int TotalCount { get; }

    public int PageNumber { get; }

    public int PageSize { get; }

    public int TotalPages { get; }

    public bool HasPrevious => PageNumber > 1;

    public bool HasNext => PageNumber < TotalPages;
}

