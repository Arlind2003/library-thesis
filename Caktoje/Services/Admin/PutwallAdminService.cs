using Caktoje.Data;
using Caktoje.Data.Resources.Common;
using Caktoje.Models;
using Microsoft.EntityFrameworkCore;

namespace Caktoje.Services.Admin;

public class PutwallAdminService
{
    private readonly CaktojeDbContext _context;

    public PutwallAdminService(CaktojeDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResource<Putwall>> GetPutwalls(string? query, int page, int pageSize)
    {
        var putwallsQuery = _context.Putwalls.AsQueryable();

        if (!string.IsNullOrEmpty(query))
        {
            putwallsQuery = putwallsQuery.Where(p => p.Name.Contains(query));
        }

        var totalItems = await putwallsQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var putwalls = await putwallsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResource<Putwall>
        {
            Items = putwalls,
            TotalPages = totalPages
        };
    }
}