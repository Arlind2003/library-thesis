using Caktoje.Data;
using Caktoje.Data.Resources.Common;
using Caktoje.Models;
using Microsoft.EntityFrameworkCore;

namespace Caktoje.Services.Admin;

public class DayClosedAdminService
{
    private readonly CaktojeDbContext _context;

    public DayClosedAdminService(CaktojeDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResource<DayClosed>> GetDaysClosed(int page, int pageSize)
    {
        var daysClosedQuery = _context.DaysClosed.AsQueryable();

        var totalItems = await daysClosedQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var daysClosed = await daysClosedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResource<DayClosed>
        {
            Items = daysClosed,
            TotalPages = totalPages
        };
    }
}