using Caktoje.Data;
using Caktoje.Data.Bindings;
using Caktoje.Data.Resources;
using Caktoje.Data.Resources.Common;
using Caktoje.Models;
using Microsoft.EntityFrameworkCore;

namespace Caktoje.Services.Admin;

public class BookStockAdminService
{
    private readonly CaktojeDbContext _context;

    public BookStockAdminService(CaktojeDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResource<BookStockResource>> GetBookStocks(List<long>? bookIds, List<long>? putwallSectionIds, string? query, int page, int pageSize)
    {
        var bookStocksQuery = _context.BookStocks.AsQueryable();

        if (bookIds != null && bookIds.Any())
        {
            bookStocksQuery = bookStocksQuery.Where(bs => bookIds.Contains(bs.BookId));
        }

        if (putwallSectionIds != null && putwallSectionIds.Any())
        {
            bookStocksQuery = bookStocksQuery.Where(bs => bs.PutwallSectionId.HasValue && putwallSectionIds.Contains(bs.PutwallSectionId.Value));
        }

        if (!string.IsNullOrEmpty(query))
        {
            bookStocksQuery = bookStocksQuery.Where(bs => bs.Book.Name.Contains(query));
        }

        var totalItems = await bookStocksQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var bookStocks = await bookStocksQuery
            .Include(bs => bs.Book)
                .ThenInclude(b => b.Image)
            .Include(bs => bs.PutwallSection)
                .ThenInclude(ps => ps.Putwall)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(bs => new BookStockResource
            {
                Id = bs.Id,
                Putwall = bs.PutwallSection.Putwall.Name,
                Row = bs.PutwallSection.Row,
                Column = bs.PutwallSection.Column,
                Book = new BookResource
                {
                    Id = bs.Book.Id,
                    Name = bs.Book.Name,
                    Description = bs.Book.Description,
                    Image = new FileResource { FileName = bs.Book.Image.FileName },
                    BookStocks = null,
                    Authors = null,
                    Categories = null
                }
            })
            .ToListAsync();

        return new PaginatedResource<BookStockResource>
        {
            Items = bookStocks,
            TotalPages = totalPages
        };
    }

    public async Task<BookStockResource?> GetBookStockById(long id)
    {
        return await _context.BookStocks
            .Where(bs => bs.Id == id)
            .Include(bs => bs.Book)
                .ThenInclude(b => b.Image)
            .Include(bs => bs.PutwallSection)
                .ThenInclude(ps => ps.Putwall)
            .Select(bs => new BookStockResource
            {
                Id = bs.Id,
                Putwall = bs.PutwallSection.Putwall.Name,
                Row = bs.PutwallSection.Row,
                Column = bs.PutwallSection.Column,
                Book = new BookResource
                {
                    Id = bs.Book.Id,
                    Name = bs.Book.Name,
                    Description = bs.Book.Description,
                    Image = new FileResource { FileName = bs.Book.Image.FileName },
                    BookStocks = null,
                    Authors = null,
                    Categories = null
                }
            })
            .FirstOrDefaultAsync();
    }

    public async Task<BookStockResource> CreateBookStock(BookStockBinding bookStockBinding)
    {
        var bookStock = new BookStock
        {
            BookId = bookStockBinding.BookId,
            PutwallSectionId = bookStockBinding.PutwallSectionId
        };

        _context.BookStocks.Add(bookStock);
        await _context.SaveChangesAsync();

        return (await GetBookStockById(bookStock.Id))!;
    }

    public async Task<BookStockResource?> UpdateBookStock(long id, BookStockBinding bookStockBinding)
    {
        var bookStock = await _context.BookStocks.FindAsync(id);
        if (bookStock == null)
        {
            return null;
        }

        bookStock.BookId = bookStockBinding.BookId;
        bookStock.PutwallSectionId = bookStockBinding.PutwallSectionId;

        await _context.SaveChangesAsync();

        return await GetBookStockById(id);
    }

    public async Task<bool> DeleteBookStock(long id)
    {
        var bookStock = await _context.BookStocks.FindAsync(id);
        if (bookStock == null)
        {
            return false;
        }

        _context.BookStocks.Remove(bookStock);
        await _context.SaveChangesAsync();
        return true;
    }
}