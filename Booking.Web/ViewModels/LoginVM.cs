using System.ComponentModel.DataAnnotations;

namespace Booking.Web.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; } = false;
        public string? ReturnUrl { get; set; }
        //public string? ExternalLoginProvider { get; set; }
        //public string? ExternalLoginUrl { get; set; }
        //public bool IsExternalLogin { get; set; } = false;
    }
}
