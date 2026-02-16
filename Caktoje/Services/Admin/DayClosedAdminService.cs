using Caktoje.Data;
using Caktoje.Data.Bindings;
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
    public async Task<DayClosed?> CreateDayClosed(DayClosedBinding dayClosed)   
    {
        var result = await _context.DaysClosed.AddAsync(new DayClosed
        {
            Date = dayClosed.Date,
            RecurringType = dayClosed.RecurringType
        });
        await _context.SaveChangesAsync();

        return result.Entity;
    }
    public async Task DeleteDayClosed(long id)
    {
        var dayClosed = await _context.DaysClosed.FindAsync(id);
        if (dayClosed != null)
        {
            _context.DaysClosed.Remove(dayClosed);
            await _context.SaveChangesAsync();
        }
    }
    public async Task<DayClosed?> UpdateDayClosed(long id, DayClosedBinding updatedDayClosed)
    {
        var dayClosed = await _context.DaysClosed.FindAsync(id);
        if (dayClosed == null)
        {
            return null;
        }

        dayClosed.Date = updatedDayClosed.Date;
        dayClosed.RecurringType = updatedDayClosed.RecurringType;

        await _context.SaveChangesAsync();

        return dayClosed;
    }
}