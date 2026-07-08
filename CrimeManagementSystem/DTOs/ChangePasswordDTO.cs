using System.ComponentModel.DataAnnotations;

namespace CrimeManagementSystem.DTOs
{
    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 8,
            ErrorMessage = "New password must be at least 8 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must contain at least 1 uppercase, 1 lowercase, 1 number and 1 special character")]
        public string NewPassword { get; set; } = string.Empty;
    }
}