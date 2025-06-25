namespace ZadatakJuric.Application.DBCParser.Dto;

public class SearchDBCRequest
{
    public string? SearchTerm { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class SearchDBCResponse
{
    public List<DBCSearchResultDto> Networks { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
}

public class DBCSearchResultDto
{
    public int Id { get; set; }
    public string NetworkName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int TotalMessages { get; set; }
    public int TotalSignals { get; set; }
    public int TotalAttributes { get; set; }
} 