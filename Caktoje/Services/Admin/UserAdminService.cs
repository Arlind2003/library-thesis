using Caktoje.Constants.Enums;
using Caktoje.Data;
using Caktoje.Data.Bindings;
using Caktoje.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Caktoje.Services.Admin
{
    public class UserAdminService
    {
        private readonly CaktojeDbContext _context;
        private readonly UserManager<User> _userManager;

        public UserAdminService(CaktojeDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<object> GetUsers(string? query, int page, int pageSize)
        {
            var usersQuery = _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                usersQuery = usersQuery.Where(u => (u.FirstName != null && u.FirstName.Contains(query)) || (u.LastName != null && u.LastName.Contains(query)) || (u.Email != null && u.Email.Contains(query)));
            }
            usersQuery = usersQuery.Where(u => !u.UserRoles.Any(ur => ur.Role.Name == UserRoleEnum.Admin.ToString()));

            var totalItems = await usersQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var users = await usersQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            users.ForEach(u => Console.WriteLine($"ROLES: {string.Join(", ", u.UserRoles.Select(ur => ur.Role.Name))}"));

            return new { items = users.Select(u => new {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.PhoneNumber
            }), totalPages };
        }

        public async Task<IdentityResult> CreateUser(UserBinding model)
        {
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = true
            };

            var password = GenerateRandomPassword();
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, UserRoleEnum.User.ToString());
            }

            return result;
        }

        private string GenerateRandomPassword(int length = 12)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
            var random = new Random();
            var chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = validChars[random.Next(0, validChars.Length)];
            }
            return new string(chars);
        }
    }
}
