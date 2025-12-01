using System;

namespace knkwebapi_v2.Models;

/// <summary>
/// Represents a paged query with filtering and sorting options.
/// This class is used to encapsulate the parameters for paginated data retrieval. Initially created on 2024-10-01 for querying entities for FormBuilder and FormWizard to load and search related entities.
/// </summary>
public class PagedQuery
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
    public Dictionary<string, string>? Filters { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
