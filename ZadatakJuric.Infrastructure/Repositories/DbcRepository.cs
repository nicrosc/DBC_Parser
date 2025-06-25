using Microsoft.EntityFrameworkCore;
using ZadatakJuric.Domain.Entities;
using ZadatakJuric.Infrastructure.Data;

namespace ZadatakJuric.Infrastructure.Repositories;

public class DbcRepository : IDbcRepository
{
    private readonly DBCDbContext _context;

    public DbcRepository(DBCDbContext context)
    {
        _context = context;
    }

    public async Task<DBC> SaveAsync(DBC dbc)
    {
        _context.DBCFiles.Add(dbc);
        await _context.SaveChangesAsync();
        return dbc;
    }

    public async Task<DBC?> GetByIdAsync(int id)
    {
        return await _context.DBCFiles
            .Include(d => d.Networks)
                .ThenInclude(n => n.Messages)
                    .ThenInclude(m => m.Signals)
                        .ThenInclude(s => s.Attributes)
            .Include(d => d.Networks)
                .ThenInclude(n => n.Messages)
                    .ThenInclude(m => m.Attributes)
            .Include(d => d.Networks)
                .ThenInclude(n => n.Attributes)
            .Include(d => d.Attributes)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<DBC?> GetByFileHashAsync(string fileHash)
    {
        return await _context.DBCFiles
            .FirstOrDefaultAsync(d => d.FileHash == fileHash);
    }

    public async Task<(IEnumerable<DBC> dbcs, int totalCount)> SearchDBCsAsync(
        string? searchTerm = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 10)
    {
        var query = _context.DBCFiles
            .Include(d => d.Networks)
                .ThenInclude(n => n.Messages)
                    .ThenInclude(m => m.Signals)
                        .ThenInclude(s => s.Attributes)
            .Include(d => d.Networks)
                .ThenInclude(n => n.Messages)
                    .ThenInclude(m => m.Attributes)
            .Include(d => d.Networks)
                .ThenInclude(n => n.Attributes)
            .Include(d => d.Attributes)
            .AsQueryable();

        // Apply search term filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(d => 
                d.FileName.Contains(searchTerm) ||
                d.Description.Contains(searchTerm) ||
                d.Networks.Any(n => n.Name.Contains(searchTerm)) ||
                d.Networks.Any(n => n.Messages.Any(m => m.Name.Contains(searchTerm))));
        }

        // Apply date range filters
        if (fromDate.HasValue)
        {
            query = query.Where(d => d.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(d => d.CreatedAt <= toDate.Value);
        }

        var totalCount = await query.CountAsync();

        var dbcs = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (dbcs, totalCount);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var dbc = await _context.DBCFiles.FindAsync(id);
        if (dbc == null)
            return false;

        _context.DBCFiles.Remove(dbc);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<DBC>> GetAllAsync()
    {
        return await _context.DBCFiles
            .Include(d => d.Networks)
                .ThenInclude(n => n.Messages)
                    .ThenInclude(m => m.Signals)
                        .ThenInclude(s => s.Attributes)
            .Include(d => d.Networks)
                .ThenInclude(n => n.Messages)
                    .ThenInclude(m => m.Attributes)
            .Include(d => d.Networks)
                .ThenInclude(n => n.Attributes)
            .Include(d => d.Attributes)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.DBCFiles.AnyAsync(d => d.Id == id);
    }
} 