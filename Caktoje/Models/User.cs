using Microsoft.AspNetCore.Identity;

namespace Caktoje.Models
{
    public class User : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; } = [];
    }
}