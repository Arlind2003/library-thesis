using Caktoje.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Caktoje.Data;

public class CaktojeDbContext : IdentityDbContext<User>
{
    public CaktojeDbContext(DbContextOptions<CaktojeDbContext> options)
        : base(options)
    {
    }

    // DbSets go here later
}