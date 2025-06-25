using MediatR;
using ZadatakJuric.Application.DBCParser.Dto;
using ZadatakJuric.Infrastructure.Repositories;

namespace ZadatakJuric.Application.DBCParser.Commands;

public class SearchDBCFilesCommand : IRequest<SearchDBCResponse>
{
    public string? SearchTerm { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class SearchDBCFilesCommandHandler : IRequestHandler<SearchDBCFilesCommand, SearchDBCResponse>
{
    private readonly IDbcRepository _dbcRepository;

    public SearchDBCFilesCommandHandler(IDbcRepository dbcRepository)
    {
        _dbcRepository = dbcRepository;
    }

    public async Task<SearchDBCResponse> Handle(SearchDBCFilesCommand request, CancellationToken cancellationToken)
    {
        var (dbcs, totalCount) = await _dbcRepository.SearchDBCsAsync(
            request.SearchTerm,
            request.FromDate,
            request.ToDate,
            request.Page,
            request.PageSize
        );

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new SearchDBCResponse
        {
            Networks = dbcs.Select(d => new DBCSearchResultDto
            {
                Id = d.Id,
                NetworkName = d.FileName,
                Version = d.Version,
                Description = d.Description,
                FilePath = d.FilePath,
                CreatedAt = d.CreatedAt,
                TotalMessages = d.TotalMessages,
                TotalSignals = d.TotalSignals,
                TotalAttributes = d.TotalAttributes
            }).ToList(),
            TotalCount = totalCount,
            TotalPages = totalPages,
            CurrentPage = request.Page,
            PageSize = request.PageSize
        };
    }
} 