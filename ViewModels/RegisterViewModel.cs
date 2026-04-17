// register 

using System.ComponentModel.DataAnnotations;

namespace CollegeEventPortal.ViewModels
{
    public class RegisterViewModel
    {
        // required all of this 
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Roll Number")]
        public string? RollNumber { get; set; }

        [Display(Name = "Department")]
        public string? Department { get; set; }

        [Display(Name = "Year")]
        public int? Year { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
    }
}
