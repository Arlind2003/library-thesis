using Caktoje.Data;
using Caktoje.Data.Bindings;
using Caktoje.Data.Resources.Common;
using Caktoje.Exceptions;
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
            putwallSectionsQuery = putwallSectionsQuery.Where(ps => ps.Putwall!.Name.Contains(query));
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

    public async Task<PutwallSection> CreatePutwallSection(PutwallSectionBinding binding)
    {
        var putwallSection = new PutwallSection
        {
            Row = binding.Row!.Value,
            Column = binding.Column!.Value,
            PutwallId = binding.PutwallId!.Value
        };

        _context.PutwallSections.Add(putwallSection);
        await _context.SaveChangesAsync();

        return putwallSection;
    }

    public async Task<PutwallSection?> UpdatePutwallSection(long id, PutwallSectionBinding binding)
    {
        var putwallSection = await _context.PutwallSections.FindAsync(id);
        if (putwallSection == null)
        {
            return null;
        }
        if(await _context.PutwallSections.AnyAsync(ps => ps.Id != id && ps.PutwallId == binding.PutwallId && ps.Row == binding.Row && ps.Column == binding.Column))
        {
            throw new BadRequestException("Putwall section with the same row and column already exists in the specified putwall.");
        }

        putwallSection.Row = binding.Row!.Value;
        putwallSection.Column = binding.Column!.Value;
        putwallSection.PutwallId = binding.PutwallId!.Value;

        await _context.SaveChangesAsync();

        return putwallSection;
    }

    public async Task<bool> DeletePutwallSection(long id)
    {
        var putwallSection = await _context.PutwallSections.FindAsync(id);
        if (putwallSection == null)
        {
            return false;
        }

        _context.PutwallSections.Remove(putwallSection);
        await _context.SaveChangesAsync();

        return true;
    }
}