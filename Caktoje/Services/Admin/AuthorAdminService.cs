using Caktoje.Data;
using Caktoje.Data.Bindings;
using Caktoje.Data.Resources;
using Caktoje.Data.Resources.Common;
using Caktoje.Models;
using Caktoje.Services;
using Microsoft.EntityFrameworkCore;

namespace Caktoje.Services.Admin;

public class AuthorAdminService
{
    private readonly CaktojeDbContext _context;
    private readonly StorageService _storageService;

    public AuthorAdminService(CaktojeDbContext context, StorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<PaginatedResource<AuthorResource>> GetAuthors(string? query, int page, int pageSize)
    {
        var authorsQuery = _context.Authors.AsQueryable();

        if (!string.IsNullOrEmpty(query))
        {
            authorsQuery = authorsQuery.Where(a => a.Name.Contains(query));
        }

        var totalItems = await authorsQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var authors = await authorsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuthorResource
            {
                Id = a.Id,
                Name = a.FullName,
                Biography = a.Biography,
                Image = new FileResource { FileName = a.Image.FileName }
            })
            .ToListAsync();

        return new PaginatedResource<AuthorResource>
        {
            Items = authors,
            TotalPages = totalPages
        };
    }

    public async Task<AuthorResource?> GetAuthorById(long id)
    {
        return await _context.Authors
            .Where(a => a.Id == id)
            .Select(a => new AuthorResource
            {
                Id = a.Id,
                Name = a.FullName,
                Biography = a.Biography,
                Image = new FileResource { FileName = a.Image.FileName }
            })
            .FirstOrDefaultAsync();
    }

    public async Task<AuthorResource> CreateAuthor(AuthorBinding authorBinding)
    {
        var imageName = authorBinding.Image != null ? await _storageService.SaveImage(authorBinding.Image) : null;
        
        var author = new Author
        {
            FullName = authorBinding.FullName,
            Biography = authorBinding.Biography,
            Image = imageName != null ? new Models.File { FileName = imageName, RelativeDirectory } : null
        };

        _context.Authors.Add(author);
        await _context.SaveChangesAsync();

        return new AuthorResource
        {
            Id = author.Id,
            Name = author.Name,
            Biography = author.Biography,
            Image = new FileResource { FileName = author.Image?.FileName }
        };
    }

    public async Task<AuthorResource?> UpdateAuthor(long id, AuthorBinding authorBinding)
    {
        var author = await _context.Authors.Include(a => a.Image).FirstOrDefaultAsync(a => a.Id == id);
        if (author == null)
        {
            return null;
        }

        author.Name = authorBinding.FullName;
        author.Biography = authorBinding.Biography;

        if (authorBinding.Image != null)
        {
            var imageName = await _storageService.SaveImage(authorBinding.Image);
            if (author.Image != null)
            {
                author.Image.FileName = imageName;
            }
            else
            {
                author.Image = new Models.File { FileName = imageName };
            }
        }

        await _context.SaveChangesAsync();

        return new AuthorResource
        {
            Id = author.Id,
            Name = author.Name,
            Biography = author.Biography,
            Image = new FileResource { FileName = author.Image?.FileName }
        };
    }

    public async Task<bool> DeleteAuthor(long id)
    {
        var author = await _context.Authors.FindAsync(id);
        if (author == null)
        {
            return false;
        }

        _context.Authors.Remove(author);
        await _context.SaveChangesAsync();
        return true;
    }
}