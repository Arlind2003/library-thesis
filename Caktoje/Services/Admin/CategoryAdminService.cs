using Caktoje.Constants.Enums;
using Caktoje.Data;
using Caktoje.Data.Bindings;
using Caktoje.Data.Resources;
using Caktoje.Data.Resources.Common;
using Caktoje.Exceptions;
using Caktoje.Models;
using Caktoje.Services;
using Microsoft.EntityFrameworkCore;

namespace Caktoje.Services.Admin;

public class CategoryAdminService
{
    private readonly CaktojeDbContext _context;
    private readonly StorageService _storageService;

    public CategoryAdminService(CaktojeDbContext context, StorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<PaginatedResource<CategoryResource>> GetCategories(string? query, int page, int pageSize)
    {
        var categoriesQuery = _context.Categories.Include(c => c.Searchable).ThenInclude(s => s!.File).AsQueryable();

        if (!string.IsNullOrEmpty(query))
        {
            categoriesQuery = categoriesQuery.Where(c => c.Name.Contains(query));
        }

        var totalItems = await categoriesQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var categories = await categoriesQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CategoryResource
            {
                Id = c.Id,
                Name = c.Name,
                Image = new FileResource { FileName = c.Searchable!.File!.FileName }
            })
            .ToListAsync();

        return new PaginatedResource<CategoryResource>
        {
            Items = categories,
            TotalPages = totalPages
        };
    }

    public async Task<CategoryResource?> GetCategoryById(long id)
    {
        return await _context.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryResource
            {
                Id = c.Id,
                Name = c.Name,
                Image = new FileResource { FileName = c.Searchable!.File!.FileName }
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CategoryResource> CreateCategory(CategoryBinding categoryBinding)
    {
        var savedFile = categoryBinding.Image != null ? await _storageService.SaveImage(categoryBinding.Image) : null;

        Models.File? file = savedFile != null ? new Models.File { FileName = savedFile.FileName, RelativeDirectory = savedFile.Directory } : null;

        if(file != null){
            _context.Files.Add(file);
            await _context.SaveChangesAsync();
        }else {
            file = _context.Files.FirstOrDefault(f => f.FileName == "default-category.png") ?? throw new Exception("Default category image not found.");
        }

        var category = new Category
        {
            Name = categoryBinding.Name,
            Searchable = new Searchable
            {
                Type = Constants.Enums.SearchableType.Category,
                Name = categoryBinding.Name,
                FileId = file.Id
            },
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        category = await _context.Categories.AsNoTracking()
            .Include(c => c.Searchable)
                .ThenInclude(s => s!.File)
            .FirstAsync(c => c.Id == category.Id);

        return new CategoryResource
        {
            Id = category.Id,
            Name = category.Name,
            Image = new FileResource { FileName = category.Searchable?.File?.FileName ?? throw new CriticalDevelopmentException("Category image not found.", DevelopmentExceptionDomainEnum.CategoryAdminService.ToString(), nameof(CategoryAdminService))}
        };
    }

    public async Task<CategoryResource?> UpdateCategory(long id, CategoryBinding categoryBinding)
    {
        var category = await _context.Categories.Include(c => c.Searchable).ThenInclude(s => s!.File).FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
        {
            return null;
        }

        category.Name = categoryBinding.Name;

        if (categoryBinding.Image != null)
        {
            var imageName = await _storageService.SaveImage(categoryBinding.Image);
            Models.File fileToDelete = await _context.Files.Where(f => f.Id == category.Searchable!.FileId).FirstOrDefaultAsync() ?? throw new Exception("File to delete not found.");
            _context.Files.Remove(fileToDelete);
            FileSaveResult savedFile = await _storageService.SaveImage(categoryBinding.Image);
            Models.File newFile = new Models.File { FileName = savedFile.FileName, RelativeDirectory = savedFile.Directory };
            _context.Files.Add(newFile);
            await _context.SaveChangesAsync();
            category.Searchable!.FileId = newFile.Id;
        }

        await _context.SaveChangesAsync();

        return new CategoryResource
        {
            Id = category.Id,
            Name = category.Name,
            Image = new FileResource { FileName = category.Searchable?.File?.FileName ?? throw new CriticalDevelopmentException("Category image not found.", DevelopmentExceptionDomainEnum.CategoryAdminService.ToString(), nameof(CategoryAdminService))}
        };
    }

    public async Task<bool> DeleteCategory(long id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return false;
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }
}