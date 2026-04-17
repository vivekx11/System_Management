// forget password 

using System.ComponentModel.DataAnnotations;

namespace CollegeEventPortal.ViewModels
{
    public class ForgotPasswordViewModel
    {

        // required 
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
