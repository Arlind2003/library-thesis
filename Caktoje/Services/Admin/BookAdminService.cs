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
            booksQuery = booksQuery.Where(b => b.BookCategories.Any(bc => categoryIds.Contains(bc.CategoryId)));
        }

        if (!string.IsNullOrEmpty(query))
        {
            booksQuery = booksQuery.Where(b => b.Name.Contains(query) || b.Description.Contains(query));
        }

        if (authorIds != null && authorIds.Any())
        {
            booksQuery = booksQuery.Where(b => b.BookAuthors.Any(ba => authorIds.Contains(ba.AuthorId)));
        }

        var totalItems = await booksQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var books = await booksQuery
            .Include(b => b.BookStocks)
                .ThenInclude(bs => bs.PutwallSection)
                    .ThenInclude(ps => ps.Putwall)
            .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                    .ThenInclude(a => a.Image)
            .Include(b => b.BookCategories)
                .ThenInclude(bc => bc.Category)
                    .ThenInclude(c => c.Image)
            .Include(b => b.Image)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BookResource
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                Image = new FileResource { FileName = b.Image.FileName },
                BookStocks = b.BookStocks.Select(bs => new BookStockResource
                {
                    Id = bs.Id,
                    Putwall = bs.PutwallSection.Putwall.Name,
                    Row = bs.PutwallSection.Row,
                    Column = bs.PutwallSection.Column,
                    Book = null
                }).ToList(),
                Authors = b.BookAuthors.Select(ba => new AuthorResource
                {
                    Id = ba.Author.Id,
                    Name = ba.Author.Name,
                    Biography = ba.Author.Biography,
                    Image = new FileResource { FileName = ba.Author.Image.FileName }
                }).ToList(),
                Categories = b.BookCategories.Select(bc => new CategoryResource
                {
                    Id = bc.Category.Id,
                    Name = bc.Category.Name,
                    Image = new FileResource { FileName = bc.Category.Image.FileName }
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
                    .ThenInclude(ps => ps.Putwall)
            .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                    .ThenInclude(a => a.Image)
            .Include(b => b.BookCategories)
                .ThenInclude(bc => bc.Category)
                    .ThenInclude(c => c.Image)
            .Include(b => b.Image)
            .Select(b => new BookResource
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                Image = new FileResource { FileName = b.Image.FileName },
                BookStocks = b.BookStocks.Select(bs => new BookStockResource
                {
                    Id = bs.Id,
                    Putwall = bs.PutwallSection.Putwall.Name,
                    Row = bs.PutwallSection.Row,
                    Column = bs.PutwallSection.Column,
                    Book = null
                }).ToList(),
                Authors = b.BookAuthors.Select(ba => new AuthorResource
                {
                    Id = ba.Author.Id,
                    Name = ba.Author.Name,
                    Biography = ba.Author.Biography,
                    Image = new FileResource { FileName = ba.Author.Image.FileName }
                }).ToList(),
                Categories = b.BookCategories.Select(bc => new CategoryResource
                {
                    Id = bc.Category.Id,
                    Name = bc.Category.Name,
                    Image = new FileResource { FileName = bc.Category.Image.FileName }
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<BookResource> CreateBook(BookBinding bookBinding)
    {
        var imageName = bookBinding.Image != null ? await _storageService.SaveImage(bookBinding.Image) : null;

        var book = new Book
        {
            Name = bookBinding.Name,
            Description = bookBinding.Description,
            ISBN = bookBinding.ISBN,
            Image = imageName != null ? new Models.File { FileName = imageName } : null
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
            .Include(b => b.BookAuthors)
            .Include(b => b.BookCategories)
            .Include(b => b.Image)
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
            var imageName = await _storageService.SaveImage(bookBinding.Image);
            if (book.Image != null)
            {
                book.Image.FileName = imageName;
            }
            else
            {
                book.Image = new Models.File { FileName = imageName };
            }
        }

        book.BookAuthors.Clear();
        foreach (var authorId in bookBinding.AuthorIds)
        {
            book.BookAuthors.Add(new BookAuthor { AuthorId = authorId });
        }

        book.BookCategories.Clear();
        foreach (var categoryId in bookBinding.CategoryIds)
        {
            book.BookCategories.Add(new BookCategory { CategoryId = categoryId });
        }

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