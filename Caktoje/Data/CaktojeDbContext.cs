using Caktoje.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Caktoje.Data;

public class CaktojeDbContext(DbContextOptions<CaktojeDbContext> options) : IdentityDbContext<User>(options)
{
    public DbSet<Searchable> Searchables { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<BookStock> BookStocks { get; set; }
    public DbSet<BookAuthor> BookAuthors { get; set; }
    public DbSet<BookCategory> BookCategories { get; set; }
    public DbSet<BookRent> BookRents { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Putwall> Putwalls { get; set; }
    public DbSet<PutwallSection> PutwallSections { get; set; }
    public DbSet<Models.File> Files { get; set; }
    public DbSet<DayClosed> DaysClosed { get; set; }

    // DbSets go here later
}