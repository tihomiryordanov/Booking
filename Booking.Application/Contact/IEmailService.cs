using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booking.Application.Contact
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string email, string subject, string message);
        Task<bool> SendContactFormEmailAsync(ContactMessage contactMessage);
        Task<bool> SendContactConfirmationEmailAsync(string clientEmail, string clientName);
    }
}