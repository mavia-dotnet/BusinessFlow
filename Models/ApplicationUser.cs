using Microsoft.AspNetCore.Identity;

namespace BusinessManagementSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public Customer? Customer { get; set; }
    }
}