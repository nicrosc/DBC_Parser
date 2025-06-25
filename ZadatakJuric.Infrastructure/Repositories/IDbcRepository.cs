using ZadatakJuric.Domain.Entities;

namespace ZadatakJuric.Infrastructure.Repositories;

public interface IDbcRepository
{
    Task<DBC> SaveAsync(DBC dbc);
    Task<DBC?> GetByIdAsync(int id);
    Task<DBC?> GetByFileHashAsync(string fileHash);
    Task<(IEnumerable<DBC> dbcs, int totalCount)> SearchDBCsAsync(
        string? searchTerm = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 10);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<DBC>> GetAllAsync();
    Task<bool> ExistsAsync(int id);
} 