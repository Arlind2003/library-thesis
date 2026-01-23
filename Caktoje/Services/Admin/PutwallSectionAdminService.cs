using Caktoje.Data;
using Caktoje.Data.Resources.Common;
using Caktoje.Models;
using Microsoft.EntityFrameworkCore;

namespace Caktoje.Services.Admin;

public class PutwallSectionAdminService
{
    private readonly CaktojeDbContext _context;

    public PutwallSectionAdminService(CaktojeDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResource<PutwallSection>> GetPutwallSections(string? query, List<long>? putwallIds, int page, int pageSize)
    {
        var putwallSectionsQuery = _context.PutwallSections.AsQueryable();

        if (!string.IsNullOrEmpty(query))
        {
            putwallSectionsQuery = putwallSectionsQuery.Where(ps => ps.Putwall.Name.Contains(query));
        }
        
        if (putwallIds != null && putwallIds.Any())
        {
            putwallSectionsQuery = putwallSectionsQuery.Where(ps => putwallIds.Contains(ps.PutwallId));
        }

        var totalItems = await putwallSectionsQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var putwallSections = await putwallSectionsQuery
            .Include(ps => ps.Putwall)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResource<PutwallSection>
        {
            Items = putwallSections,
            TotalPages = totalPages
        };
    }
}