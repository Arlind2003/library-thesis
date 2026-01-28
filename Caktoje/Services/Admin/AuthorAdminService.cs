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
            authorsQuery = authorsQuery.Where(a => a.FullName.Contains(query));
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
                Image = new FileResource { FileName = a.Searchable!.File!.FileName }
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
                Image = new FileResource { FileName = a.Searchable!.File!.FileName }
            })
            .FirstOrDefaultAsync();
    }

    public async Task<AuthorResource> CreateAuthor(AuthorBinding authorBinding)
    {
        var image = authorBinding.Image != null ? await _storageService.SaveImage(authorBinding.Image) : null;

        Models.File? file = image != null ? new Models.File { FileName = image.FileName, RelativeDirectory = image.Directory } : null;

        if(file != null){
            _context.Files.Add(file);
            await _context.SaveChangesAsync();
        }else {
            file = _context.Files.FirstOrDefault(f => f.FileName == "default-author.png") ?? throw new Exception("Default author image not found.");
        }

        var author = new Author
        {
            FullName = authorBinding.FullName,
            Biography = authorBinding.Biography,
            Searchable = new Searchable(){
                FileId = file.Id,
                Type = Constants.Enums.SearchableType.Author,
                Name = authorBinding.FullName
            }
        };

        _context.Authors.Add(author);
        await _context.SaveChangesAsync();

        return new AuthorResource
        {
            Id = author.Id,
            Name = author.FullName,
            Biography = author.Biography,
            Image = new FileResource { FileName = author.Searchable!.File!.FileName }
        };
    }

    public async Task<AuthorResource?> UpdateAuthor(long id, AuthorBinding authorBinding)
    {
        var author = await _context.Authors.Include(a => a.Searchable).ThenInclude(s => s!.File).FirstOrDefaultAsync(a => a.Id == id);
        if (author == null)
        {
            return null;
        }

        author.FullName = authorBinding.FullName;
        author.Biography = authorBinding.Biography;

        if (authorBinding.Image != null)
        {
            var image = await _storageService.SaveImage(authorBinding.Image);
            if (author.Searchable!.File != null)
            {
                author.Searchable.File.FileName = image.FileName;
            }
            else
            {
                author.Searchable.File = new Models.File { FileName = image.FileName, RelativeDirectory = image.Directory };
            }
        }

        await _context.SaveChangesAsync();

        return new AuthorResource
        {
            Id = author.Id,
            Name = author.FullName,
            Biography = author.Biography,
            Image = new FileResource { FileName = author.Searchable!.File!.FileName }
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