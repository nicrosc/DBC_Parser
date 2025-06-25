using MediatR;
using ZadatakJuric.Application.DBCParser.Dto;
using ZadatakJuric.Infrastructure.Repositories;

namespace ZadatakJuric.Application.DBCParser.Queries;

public class GetDBCOverviewQuery : IRequest<DBCOverviewResponse>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class GetDBCOverviewQueryHandler : IRequestHandler<GetDBCOverviewQuery, DBCOverviewResponse>
{
    private readonly IDbcRepository _dbcRepository;

    public GetDBCOverviewQueryHandler(IDbcRepository dbcRepository)
    {
        _dbcRepository = dbcRepository;
    }

    public async Task<DBCOverviewResponse> Handle(GetDBCOverviewQuery request, CancellationToken cancellationToken)
    {
        // Get all DBC files with pagination
        var (dbcs, totalCount) = await _dbcRepository.SearchDBCsAsync(
            null, // no search term - get all
            null, // no from date
            null, // no to date
            request.Page,
            request.PageSize
        );

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        // Calculate summary statistics
        var totalMessages = dbcs.Sum(d => d.TotalMessages);
        var totalSignals = dbcs.Sum(d => d.TotalSignals);
        var totalAttributes = dbcs.Sum(d => d.TotalAttributes);

        return new DBCOverviewResponse
        {
            Networks = dbcs.Select(d => new DBCOverviewItemDto
            {
                Id = d.Id,
                NetworkName = d.FileName,
                Version = d.Version,
                Description = d.Description,
                FilePath = Path.GetFileName(d.FilePath),
                CreatedAt = d.CreatedAt,
                TotalMessages = d.TotalMessages,
                TotalSignals = d.TotalSignals,
                TotalAttributes = d.TotalAttributes,
                FileSize = GetFormattedFileSize(d.FileSize)
            }).ToList(),
            TotalNetworks = totalCount,
            TotalMessages = totalMessages,
            TotalSignals = totalSignals,
            TotalAttributes = totalAttributes,
            TotalPages = totalPages,
            CurrentPage = request.Page,
            PageSize = request.PageSize
        };
    }

    private string GetFormattedFileSize(long bytes)
    {
        if (bytes >= 1024 * 1024)
            return $"{bytes / (1024.0 * 1024.0):F1} MB";
        else if (bytes >= 1024)
            return $"{bytes / 1024.0:F1} KB";
        else
            return $"{bytes} B";
    }
} 