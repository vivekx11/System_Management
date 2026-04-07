// password 

using System.ComponentModel.DataAnnotations;

namespace CollegeEventPortal.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
