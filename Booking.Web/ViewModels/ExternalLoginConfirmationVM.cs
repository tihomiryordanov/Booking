using System.ComponentModel.DataAnnotations;

namespace Booking.Web.ViewModels
{
    public class ExternalLoginConfirmationVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}