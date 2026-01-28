using Caktoje.Data;
using Caktoje.Data.Bindings;
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
    public async Task<Putwall?> CreatePutwall(PutwallBinding createDto)   
    {
        var putwall = new Putwall
        {
            Name = createDto.Name,
            Rows = createDto.Rows,
            Columns = createDto.Columns
        };

        _context.Putwalls.Add(putwall);
        await _context.SaveChangesAsync();

        List<PutwallSection> sections = [];

        for(int i = 0; i < createDto.Rows; i++)
        {
            for(int j = 0; j < createDto.Columns; j++)
            {
                sections.Add(new PutwallSection
                {
                    Row = i + 1,
                    Column = j + 1,
                    PutwallId = putwall.Id
                });
            }
        }
        await _context.PutwallSections.AddRangeAsync(sections);
        await _context.SaveChangesAsync();

        return putwall;
    }
    public async Task<Putwall?> UpdatePutwall(long id, PutwallBinding updateDto)
    {
        var putwall = await _context.Putwalls.FindAsync(id);
        if (putwall == null)
        {
            return null;
        }
        await _context.PutwallSections
            .Where(ps => ps.PutwallId == id)
            .ExecuteDeleteAsync();

        putwall.Name = updateDto.Name;
        putwall.Rows = updateDto.Rows;
        putwall.Columns = updateDto.Columns;

        await _context.SaveChangesAsync();

        List<PutwallSection> sections = [];
        for (int i = 0; i < updateDto.Rows; i++)
        {
            for (int j = 0; j < updateDto.Columns; j++)
            {
                sections.Add(new PutwallSection
                {
                    Row = i + 1,
                    Column = j + 1,
                    PutwallId = putwall.Id
                });
            }
        }
        await _context.PutwallSections.AddRangeAsync(sections);
        await _context.SaveChangesAsync();

        return putwall;
    }
}