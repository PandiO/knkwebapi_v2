using System;
using System.Text.Json.Serialization;

namespace knkwebapi_v2.Dtos;

public class PagedQueryDto
{
    [JsonPropertyName("pageNumber")]
    public int PageNumber { get; set; } = 1;
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; } = 10;
    [JsonPropertyName("searchTerm")]
    public string? SearchTerm { get; set; }
    [JsonPropertyName("sortBy")]
    public string? SortBy { get; set; }
    [JsonPropertyName("sortDescending")]
    public bool SortDescending { get; set; } = false;
    [JsonPropertyName("filters")]
    public Dictionary<string, string>? Filters { get; set; }
}

public class PagedResultDto<T>
{
    [JsonPropertyName("items")]
    public List<T> Items { get; set; } = new();
    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }
    [JsonPropertyName("pageNumber")]
    public int PageNumber { get; set; }
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }
}
