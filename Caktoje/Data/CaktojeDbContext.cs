using Caktoje.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Caktoje.Data;

public class CaktojeDbContext : IdentityDbContext<
    User, 
    Role, 
    string, 
    IdentityUserClaim<string>, 
    UserRole,          
    IdentityUserLogin<string>, 
    IdentityRoleClaim<string>, 
    IdentityUserToken<string>>
{
    public CaktojeDbContext(DbContextOptions<CaktojeDbContext> options) : base(options) { }
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BookAuthor>()
            .HasKey(ba => new { ba.BookId, ba.AuthorId });

        modelBuilder.Entity<BookCategory>()
            .HasKey(bc => new { bc.BookId, bc.CategoryId });

        modelBuilder.Entity<User>(b =>
    {
        b.HasMany(e => e.UserRoles)
            .WithOne(e => e.User)
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();
    });

    // Map the Role side of the join
    modelBuilder.Entity<Role>(b =>
    {
        b.HasMany(e => e.UserRoles)
            .WithOne(e => e.Role)
            .HasForeignKey(ur => ur.RoleId)
            .IsRequired();
    });
    }
}