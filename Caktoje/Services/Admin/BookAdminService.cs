using Caktoje.Data;
using Caktoje.Data.Bindings;
using Caktoje.Data.Resources;
using Caktoje.Data.Resources.Common;
using Caktoje.Models;
using Caktoje.Services;
using Microsoft.EntityFrameworkCore;

namespace Caktoje.Services.Admin;

public class BookAdminService
{
    private readonly CaktojeDbContext _context;
    private readonly StorageService _storageService;

    public BookAdminService(CaktojeDbContext context, StorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    

    public async Task<PaginatedResource<BookResource>> GetBooks(List<long>? categoryIds, string? query, List<long>? authorIds, int page, int pageSize)
    {
        var booksQuery = _context.Books.AsQueryable();

        if (categoryIds != null && categoryIds.Any())
        {
            booksQuery = booksQuery.Where(b => b.Categories.Any(bc => categoryIds.Contains(bc.CategoryId)));
        }

        if (!string.IsNullOrEmpty(query))
        {
            booksQuery = booksQuery.Where(b => b.Name.ToLower().Contains(query.ToLower()) || (b.Description != null && b.Description.ToLower().Contains(query.ToLower())));
        }

        if (authorIds != null && authorIds.Any())
        {
            booksQuery = booksQuery.Where(b => b.Authors.Any(ba => authorIds.Contains(ba.AuthorId)));
        }

        var totalItems = await booksQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var books = await booksQuery
            .Include(b => b.BookStocks)
                .ThenInclude(bs => bs.PutwallSection)
                    .ThenInclude(ps => ps!.Putwall)
            .Include(b => b.Authors)
                .ThenInclude(ba => ba.Author)
                    .ThenInclude(a => a!.Searchable)
                        .ThenInclude(s => s!.File)
            .Include(b => b.Categories)
                .ThenInclude(bc => bc.Category)
                    .ThenInclude(c => c!.Searchable)
                        .ThenInclude(s => s!.File)
            .Include(b => b.Searchable)
                .ThenInclude(s => s!.File)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BookResource
            {
                Id = b.Id,
                Name = b.Name,
                ISBN = b.ISBN,
                Description = b.Description,
                Image = new FileResource { FileName = b.Searchable!.File!.FileName },
                BookStocks = b.BookStocks.Select(bs => new BookStockResource
                {
                    Id = bs.Id,
                    State = bs.State,
                    Putwall = bs.PutwallSection!.Putwall!.Name,
                    Row = bs.PutwallSection.Row,
                    Column = bs.PutwallSection.Column,
                    Book = null
                }).ToList(),
                Authors = b.Authors.Select(ba => new AuthorResource
                {
                    Id = ba.Author!.Id,
                    Name = ba.Author!.FullName,
                    Biography = ba.Author!.Biography,
                    Image = new FileResource { FileName = ba.Author!.Searchable!.File!.FileName }
                }).ToList(),
                Categories = b.Categories.Select(bc => new CategoryResource
                {
                    Id = bc.Category!.Id,
                    Name = bc.Category!.Name,
                    Image = new FileResource { FileName = bc.Category!.Searchable!.File!.FileName }
                }).ToList()
            })
            .ToListAsync();

        return new PaginatedResource<BookResource>
        {
            Items = books,
            TotalPages = totalPages
        };
    }

    public async Task<BookResource?> GetBookById(long id)
    {
        return await _context.Books
            .Where(b => b.Id == id)
            .Include(b => b.BookStocks)
                .ThenInclude(bs => bs.PutwallSection)
                    .ThenInclude(ps => ps!.Putwall)
            .Include(b => b.Authors)
                .ThenInclude(ba => ba.Author)
                    .ThenInclude(a => a!.Searchable)
                        .ThenInclude(s => s!.File)
            .Include(b => b.Categories)
                .ThenInclude(bc => bc.Category)
                    .ThenInclude(c => c!.Searchable)
                        .ThenInclude(s => s!.File)
            .Include(b => b.Searchable)
                .ThenInclude(s => s!.File)
            .Select(b => new BookResource
            {
                Id = b.Id,
                ISBN = b.ISBN,
                Name = b.Name,
                Description = b.Description,
                Image = new FileResource { FileName = b.Searchable!.File!.FileName },
                BookStocks = b.BookStocks.Select(bs => new BookStockResource
                {
                    Id = bs.Id,
                    State = bs.State,
                    Putwall = bs.PutwallSection!.Putwall!.Name,
                    Row = bs.PutwallSection.Row,
                    Column = bs.PutwallSection.Column,
                    Book = null
                }).ToList(),
                Authors = b.Authors.Select(ba => new AuthorResource
                {
                    Id = ba.Author!.Id,
                    Name = ba.Author!.FullName,
                    Biography = ba.Author!.Biography,
                    Image = new FileResource { FileName = ba.Author!.Searchable!.File!.FileName }
                }).ToList(),
                Categories = b.Categories.Select(bc => new CategoryResource
                {
                    Id = bc.Category!.Id,
                    Name = bc.Category!.Name,
                    Image = new FileResource { FileName = bc.Category!.Searchable!.File!.FileName }
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<BookResource> CreateBook(BookBinding bookBinding)
    {
        var image = bookBinding.Image != null ? await _storageService.SaveImage(bookBinding.Image) : null;

        Models.File? file = image != null ? new Models.File { FileName = image.FileName, RelativeDirectory = image.Directory } : null;

        if(file != null){
            _context.Files.Add(file);
            await _context.SaveChangesAsync();
        }else {
            file = _context.Files.FirstOrDefault(f => f.FileName == "default-book.png") ?? throw new Exception("Default book image not found.");
        }

        var book = new Book
        {
            Name = bookBinding.Name,
            Description = bookBinding.Description,
            ISBN = bookBinding.ISBN,
            Categories = [],
            Authors = [],
            BookStocks = [],
            Searchable = new(){
                Name = bookBinding.Name,
                FileId = file.Id,
                Type = Constants.Enums.SearchableType.Book,
            }
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        foreach (var authorId in bookBinding.AuthorIds)
        {
            _context.BookAuthors.Add(new BookAuthor { BookId = book.Id, AuthorId = authorId });
        }

        foreach (var categoryId in bookBinding.CategoryIds)
        {
            _context.BookCategories.Add(new BookCategory { BookId = book.Id, CategoryId = categoryId });
        }

        await _context.SaveChangesAsync();

        return (await GetBookById(book.Id))!;
    }

    public async Task<BookResource?> UpdateBook(long id, BookBinding bookBinding)
    {
        var book = await _context.Books
            .Include(b => b.Authors)
            .Include(b => b.Categories)
            .Include(b => b.Searchable).ThenInclude(s => s!.File)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
        {
            return null;
        }

        book.Name = bookBinding.Name;
        book.Description = bookBinding.Description;
        book.ISBN = bookBinding.ISBN;

        if (bookBinding.Image != null)
        {
            var image = await _storageService.SaveImage(bookBinding.Image);
            if (book.Searchable!.File != null)
            {
                book.Searchable.File.FileName = image.FileName;
            }
            else
            {
                book.Searchable.File = new Models.File { FileName = image.FileName, RelativeDirectory = image.Directory };
            }
        }

        await _context.BookAuthors.Where(ba => ba.BookId == book.Id).ExecuteDeleteAsync();
        var bookAuthors = bookBinding.AuthorIds.Select(a => new BookAuthor { BookId = book.Id, AuthorId = a });
        await _context.BookAuthors.AddRangeAsync(bookAuthors);

        await _context.BookCategories.Where(bc => bc.BookId == book.Id).ExecuteDeleteAsync();
        var bookCategories = bookBinding.CategoryIds.Select(c => new BookCategory { BookId = book.Id, CategoryId = c });
        await _context.BookCategories.AddRangeAsync(bookCategories);

        await _context.SaveChangesAsync();

        return await GetBookById(id);
    }

    public async Task<bool> DeleteBook(long id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null)
        {
            return false;
        }

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
        return true;
    }
}