using System.Threading.Tasks;
using Caktoje.Constants.Enums;
using Caktoje.Models;
using Microsoft.AspNetCore.Identity;

namespace Caktoje.Data;

public class Seeder
{
    public static async Task Seed(IServiceProvider serviceProvider)
    {
        UserManager<Models.User> userManager = serviceProvider.GetRequiredService<UserManager<Models.User>>();
        var admin = userManager.Users.FirstOrDefault(u => u.UserName == "admin");
        if (admin == null)
        {
            var user = await userManager.CreateAsync(new Models.User
            {
                UserName = "admin@example.com",
                Email = "admin@example.com",
                FirstName = "Admin",
                LastName = "Admin",
                NormalizedUserName = "ADMIN@EXAMPLE.COM",
                NormalizedEmail = "ADMIN@EXAMPLE.COM",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
            }, "Admin123!");
        }
        var adminRole = serviceProvider.GetRequiredService<RoleManager<Role>>();
        if (!await adminRole.RoleExistsAsync(UserRoleEnum.Admin.ToString()))
        {
            await adminRole.CreateAsync(new Role(UserRoleEnum.Admin.ToString()));
        }
        if (!await adminRole.RoleExistsAsync(UserRoleEnum.User.ToString()))
        {
            await adminRole.CreateAsync(new Role(UserRoleEnum.User.ToString()));
        }
        admin = await userManager.FindByNameAsync("admin@example.com");
        if (admin != null)
        {
            await userManager.AddToRoleAsync(admin, UserRoleEnum.Admin.ToString());
        }
    }
}