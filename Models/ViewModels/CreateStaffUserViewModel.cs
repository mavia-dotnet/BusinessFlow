
using System.ComponentModel.DataAnnotations;

namespace BusinessManagementSystem.Models.ViewModels
{
    public class CreateStaffUserViewModel
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(Manager|Staff)$",
            ErrorMessage = "Select Manager or Staff.")]
        public string Role { get; set; } = "Staff";
    }
}
