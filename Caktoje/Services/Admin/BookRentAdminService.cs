using Caktoje.Data;
using Caktoje.Data.Resources;
using Caktoje.Data.Resources.Common;
using Microsoft.EntityFrameworkCore;

namespace Caktoje.Services.Admin;

public class BookRentAdminService
{
    private readonly CaktojeDbContext _context;

    public BookRentAdminService(CaktojeDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResource<BookRentResource>> GetBookRents(List<long>? bookStockIds, List<string>? renterIds, bool? overdue, int page, int pageSize)
    {
        var bookRentsQuery = _context.BookRents.AsQueryable();

        if (bookStockIds != null && bookStockIds.Any())
        {
            bookRentsQuery = bookRentsQuery.Where(br => bookStockIds.Contains(br.BookStockId));
        }

        if (renterIds != null && renterIds.Any())
        {
            bookRentsQuery = bookRentsQuery.Where(br => renterIds.Contains(br.RenterId));
        }

        if (overdue.HasValue)
        {
            if (overdue.Value)
            {
                bookRentsQuery = bookRentsQuery.Where(br => br.ReturnedAt == null && br.ExpiresAt < DateTime.UtcNow);
            }
            else
            {
                bookRentsQuery = bookRentsQuery.Where(br => br.ReturnedAt != null || br.ExpiresAt >= DateTime.UtcNow);
            }
        }

        var totalItems = await bookRentsQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var bookRents = await bookRentsQuery
            .Include(br => br.Renter)
            .Include(br => br.BookStock)
                .ThenInclude(bs => bs.Book)
                    .ThenInclude(b => b.Image)
            .Include(br => br.BookStock)
                .ThenInclude(bs => bs.PutwallSection)
                    .ThenInclude(ps => ps.Putwall)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(br => new BookRentResource
            {
                Id = br.Id,
                RentedAt = br.RentedAt,
                ExpiresAt = br.ExpiresAt,
                ReturnedAt = br.ReturnedAt,
                Renter = new UserResource
                {
                    Id = br.Renter.Id,
                    UserName = br.Renter.UserName,
                    Email = br.Renter.Email
                },
                BookStock = new BookStockResource
                {
                    Id = br.BookStock.Id,
                    Putwall = br.BookStock.PutwallSection.Putwall.Name,
                    Row = br.BookStock.PutwallSection.Row,
                    Column = br.BookStock.PutwallSection.Column,
                    Book = new BookResource
                    {
                        Id = br.BookStock.Book.Id,
                        Name = br.BookStock.Book.Name,
                        Description = br.BookStock.Book.Description,
                        Image = new FileResource { FileName = br.BookStock.Book.Image.FileName },
                        BookStocks = null,
                        Authors = null,
                        Categories = null
                    }
                }
            })
            .ToListAsync();

        return new PaginatedResource<BookRentResource>
        {
            Items = bookRents,
            TotalPages = totalPages
        };
    }
}