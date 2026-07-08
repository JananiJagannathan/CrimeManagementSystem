using System.ComponentModel.DataAnnotations;

namespace CrimeManagementSystem.DTOs
{
    public class AddOfficerDTO
    {
        [Required(ErrorMessage = "Officer name is required")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format. Example: officer@cms.com")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8,
            ErrorMessage = "Password must be at least 8 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must contain at least 1 uppercase, 1 lowercase, 1 number and 1 special character")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Badge number is required")]
        [StringLength(20, MinimumLength = 3,
            ErrorMessage = "Badge number must be between 3 and 20 characters")]
        [RegularExpression(@"^[A-Z]{2}-\d{3}$",
            ErrorMessage = "Badge number format must be like TN-001")]
        public string BadgeNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^\d{10}$",
            ErrorMessage = "Phone number must be exactly 10 digits")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(250, MinimumLength = 5,
            ErrorMessage = "Address must be between 5 and 250 characters")]
        public string Address { get; set; } = string.Empty;
    }
}