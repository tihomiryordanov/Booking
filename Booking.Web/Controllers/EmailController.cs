using Booking.Application.Contact;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Booking.Web.Controllers
{
    public class EmailController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailController> _logger;

        public EmailController(IEmailService emailService, ILogger<EmailController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View(new ContactMessage());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactMessage contactMessage)
        {
            if (!ModelState.IsValid)
            {
                return View(contactMessage);
            }

            // Add additional information
            contactMessage.SubmittedAt = DateTime.UtcNow;
            contactMessage.IpAddress = GetClientIpAddress();
            contactMessage.UserAgent = Request.Headers["User-Agent"].ToString();

            try
            {
                // Send email to admin
                var adminEmailSent = await _emailService.SendContactFormEmailAsync(contactMessage);
                
                if (adminEmailSent)
                {
                    // Send confirmation email to client
                    var confirmationSent = await _emailService.SendContactConfirmationEmailAsync(
                        contactMessage.Email, contactMessage.Name);
                    
                    if (confirmationSent)
                    {
                        TempData["Success"] = "Thank you for your message! We have received your inquiry and will get back to you soon. A confirmation email has been sent to your address.";
                    }
                    else
                    {
                        TempData["Success"] = "Thank you for your message! We have received your inquiry and will get back to you soon.";
                        _logger.LogWarning("Failed to send confirmation email to {Email}", contactMessage.Email);
                    }
                    
                    _logger.LogInformation("Contact form submitted by {Name} ({Email})", contactMessage.Name, contactMessage.Email);
                    
                    // Clear the form after successful submission
                    return RedirectToAction(nameof(Contact));
                }
                else
                {
                    TempData["Error"] = "We're sorry, but there was an issue sending your message. Please try again later or contact us directly.";
                    _logger.LogError("Failed to send contact form email from {Email}", contactMessage.Email);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "We're sorry, but there was an unexpected error. Please try again later.";
                _logger.LogError(ex, "Error processing contact form submission from {Email}", contactMessage.Email);
            }

            return View(contactMessage);
        }

        [HttpGet]
        public IActionResult ThankYou()
        {
            return View();
        }

        private string GetClientIpAddress()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;
            
            // Check for forwarded IP addresses (in case of proxy/load balancer)
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                var forwardedIps = Request.Headers["X-Forwarded-For"].ToString().Split(',');
                if (forwardedIps.Length > 0 && !string.IsNullOrWhiteSpace(forwardedIps[0]))
                {
                    if (IPAddress.TryParse(forwardedIps[0].Trim(), out var parsedIp))
                    {
                        ipAddress = parsedIp;
                    }
                }
            }
            
            return ipAddress?.ToString() ?? "Unknown";
        }
    }
}