using Caktoje.Data;
using Caktoje.Data.Bindings;
using Caktoje.Data.Resources;
using Caktoje.Data.Resources.Common;
using Caktoje.Exceptions;
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
                bookRentsQuery = bookRentsQuery.Where(br => br.DueDate < DateOnly.FromDateTime(DateTime.UtcNow) && br.ReturnedDate == null);
            }
            else
            {
                bookRentsQuery = bookRentsQuery.Where(br => br.ReturnedDate != null || br.DueDate >= DateOnly.FromDateTime(DateTime.UtcNow));
            }
        }

        var totalItems = await bookRentsQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var bookRents = await bookRentsQuery
            .Include(br => br.Renter)
            .Include(br => br.BookStock)
                .ThenInclude(bs => bs!.Book)
                    .ThenInclude(b => b!.Searchable)
                        .ThenInclude(s => s!.File)
            .Include(br => br.BookStock)
                .ThenInclude(bs => bs!.PutwallSection)
                    .ThenInclude(ps => ps!.Putwall)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(br => new BookRentResource
            {
                Id = br.Id,
                RentedAt = br.RentDate.ToDateTime(new TimeOnly(0, 0)),
                ExpiresAt = br.DueDate.ToDateTime(new TimeOnly(0, 0)),
                ReturnedAt = br.ReturnedDate != null ? br.ReturnedDate.Value.ToDateTime(new TimeOnly(0, 0)) : null,
                Renter = new UserResource
                {
                    Id = br.Renter!.Id,
                    UserName = br.Renter!.UserName ?? "N/A",
                    Email = br.Renter!.Email ?? "N/A"
                },
                BookStock = new BookStockResource
                {
                    Id = br.BookStock!.Id,
                    Putwall = br.BookStock.PutwallSection!.Putwall!.Name,
                    Row = br.BookStock.PutwallSection.Row,
                    Column = br.BookStock.PutwallSection.Column,
                    Book = new BookResource
                    {
                        Id = br.BookStock.Book!.Id,
                        Name = br.BookStock.Book.Name,
                        Description = br.BookStock.Book.Description,
                        Image = new FileResource { FileName = br.BookStock.Book.Searchable!.File!.FileName },
                        BookStocks = new List<BookStockResource>(),
                        Authors = new List<AuthorResource>(),
                        Categories = new List<CategoryResource>()
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
    public async Task<BookRentResource?> CreateBookRent(BookRentBinding bookRentBinding){
        var bookRent = new Models.BookRent
        {
            RenterId = bookRentBinding.RenterId,
            BookStockId = bookRentBinding.BookStockId,
            ReturnedDate = null,
            RentDate = DateOnly.FromDateTime(DateTime.UtcNow),
            DueDate = DateOnly.FromDateTime(bookRentBinding.ExpiresAt)
        };
        if(await _context.BookRents.AnyAsync(bs => bs.Id == bookRent.BookStockId && bs.ReturnedDate == null))
        {
            throw new BadRequestException("Book stock is already rented.");
        }
        _context.BookRents.Add(bookRent);
        await _context.SaveChangesAsync();

        return await _context.BookRents
            .Where(br => br.Id == bookRent.Id)
            .Include(br => br.Renter)
            .Include(br => br.BookStock)
                .ThenInclude(bs => bs!.Book)
                    .ThenInclude(b => b!.Searchable)
                        .ThenInclude(s => s!.File)
            .Include(br => br.BookStock)
                .ThenInclude(bs => bs!.PutwallSection)
                    .ThenInclude(ps => ps!.Putwall)
            .Select(br => new BookRentResource
            {
                Id = br.Id,
                RentedAt = br.RentDate.ToDateTime(new TimeOnly(0, 0)),
                ExpiresAt = br.DueDate.ToDateTime(new TimeOnly(0, 0)),
                ReturnedAt = br.ReturnedDate != null ? br.ReturnedDate.Value.ToDateTime(new TimeOnly(0, 0)) : null,
                Renter = new UserResource
                {
                    Id = br.Renter!.Id,
                    UserName = br.Renter!.UserName ?? "N/A",
                    Email = br.Renter!.Email ?? "N/A"
                },
                BookStock = new BookStockResource
                {
                    Id = br.BookStock!.Id,
                    Putwall = br.BookStock.PutwallSection!.Putwall!.Name,
                    Row = br.BookStock.PutwallSection.Row,
                    Column = br.BookStock.PutwallSection.Column,
                    Book = new BookResource
                    {
                        Id = br.BookStock.Book!.Id,
                        Name = br.BookStock.Book.Name,
                        Description = br.BookStock.Book.Description,
                        Image = new FileResource { FileName = br.BookStock.Book.Searchable!.File!.FileName },
                        BookStocks = new List<BookStockResource>(),
                        Authors = new List<AuthorResource>(),
                        Categories = new List<CategoryResource>()
                    }
                }
            })
            .FirstOrDefaultAsync();
    }
    public async Task ReturnBook(long id){
        var bookRent = await _context.BookRents.FindAsync(id) ?? throw new NotFoundException("Book rent not found.");
        bookRent.ReturnedDate = DateOnly.FromDateTime(DateTime.UtcNow);
        await _context.SaveChangesAsync();
    }
}