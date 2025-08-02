using Booking.Application.Contact;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Google.Apis.Gmail.v1;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using MimeKit;
using System.Text;

namespace Booking.Infrastructure.Emails
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly GoogleOAuthSettings _googleOAuthSettings;
        private readonly IGoogleTokenService _googleTokenService;
        private readonly IConfiguration _configuration;

        public EmailService(
            IConfiguration configuration, 
            IOptions<EmailSettings> emailSettings,
            IOptions<GoogleOAuthSettings> googleOAuthSettings,
            IGoogleTokenService googleTokenService)
        {
            _configuration = configuration;
            _emailSettings = emailSettings.Value;
            _googleOAuthSettings = googleOAuthSettings.Value;
            _googleTokenService = googleTokenService;
        }

        public async Task<bool> SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var accessToken = await GetValidAccessTokenAsync();
                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new UnauthorizedAccessException("Gmail authentication required.");
                }

                var credential = GoogleCredential.FromAccessToken(accessToken);
                var service = new GmailService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Booking App"
                });

                var emailMessage = CreateMimeMessage(email, subject, message);
                var gmail = new Google.Apis.Gmail.v1.Data.Message
                {
                    Raw = Convert.ToBase64String(Encoding.UTF8.GetBytes(emailMessage.ToString()))
                        .Replace('+', '-')
                        .Replace('/', '_')
                        .Replace("=", "")
                };

                await service.Users.Messages.Send(gmail, "me").ExecuteAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Gmail sending failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendContactFormEmailAsync(ContactMessage contactMessage)
        {
            try
            {
                var subject = $"New Contact Form Submission - {contactMessage.Subject}";
                var htmlBody = CreateContactFormEmailBody(contactMessage);
                
                return await SendEmailAsync(_emailSettings.SenderEmail, subject, htmlBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Contact form email failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendContactConfirmationEmailAsync(string clientEmail, string clientName)
        {
            try
            {
                var subject = "Thank you for contacting BookingApp";
                var htmlBody = CreateConfirmationEmailBody(clientName);
                
                return await SendEmailAsync(clientEmail, subject, htmlBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Confirmation email failed: {ex.Message}");
                return false;
            }
        }

        private async Task<string?> GetValidAccessTokenAsync()
        {
            var accessToken = await _googleTokenService.GetValidAccessTokenAsync();
            
            if (string.IsNullOrEmpty(accessToken))
            {
                var refreshedToken = await _googleTokenService.RefreshTokenAsync();
                if (refreshedToken != null)
                {
                    accessToken = refreshedToken.AccessToken;
                }
            }

            return accessToken;
        }

        private MimeMessage CreateMimeMessage(string toEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = body
            };

            message.Body = builder.ToMessageBody();
            return message;
        }

        private string CreateContactFormEmailBody(ContactMessage contact)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>New Contact Form Submission</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f8f9fa; padding: 20px; margin: 20px 0; }}
        .field {{ margin-bottom: 15px; }}
        .label {{ font-weight: bold; color: #495057; }}
        .value {{ margin-top: 5px; padding: 10px; background-color: white; border-left: 4px solid #007bff; }}
        .footer {{ text-align: center; color: #6c757d; font-size: 12px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>New Contact Form Submission</h1>
        </div>
        
        <div class='content'>
            <div class='field'>
                <div class='label'>Name:</div>
                <div class='value'>{contact.Name}</div>
            </div>
            
            <div class='field'>
                <div class='label'>Email:</div>
                <div class='value'>{contact.Email}</div>
            </div>
            
            {(string.IsNullOrEmpty(contact.Phone) ? "" : $@"
            <div class='field'>
                <div class='label'>Phone:</div>
                <div class='value'>{contact.Phone}</div>
            </div>")}
            
            <div class='field'>
                <div class='label'>Subject:</div>
                <div class='value'>{contact.Subject}</div>
            </div>
            
            <div class='field'>
                <div class='label'>Message:</div>
                <div class='value'>{contact.Message.Replace("\n", "<br>")}</div>
            </div>
            
            <div class='field'>
                <div class='label'>Submitted At:</div>
                <div class='value'>{contact.SubmittedAt:yyyy-MM-dd HH:mm:ss} UTC</div>
            </div>
        </div>
        
        <div class='footer'>
            <p>This email was sent from the BookingApp contact form.</p>
            <p>IP Address: {contact.IpAddress}</p>
        </div>
    </div>
</body>
</html>";
        }

        private string CreateConfirmationEmailBody(string clientName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Thank you for contacting us</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f8f9fa; padding: 30px; margin: 20px 0; }}
        .footer {{ text-align: center; color: #6c757d; font-size: 12px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Thank You for Contacting Us!</h1>
        </div>
        
        <div class='content'>
            <p>Dear {clientName},</p>
            
            <p>Thank you for reaching out to BookingApp. We have received your message and will get back to you as soon as possible.</p>
            
            <p>Our team typically responds within 24-48 hours during business days.</p>
            
            <p>In the meantime, feel free to browse our available villas and make reservations on our website.</p>
            
            <p>Best regards,<br>
            The BookingApp Team</p>
        </div>
        
        <div class='footer'>
            <p>This is an automated confirmation email from BookingApp.</p>
            <p>Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";
        }

        // Keep existing methods for backward compatibility
        public string GetGoogleAuthUrl()
        {
            return _googleTokenService.GetAuthorizationUrl();
        }

        public async Task<bool> HandleGoogleCallbackAsync(string code)
        {
            try
            {
                await _googleTokenService.ExchangeCodeForTokenAsync(code);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool IsGoogleAuthReady()
        {
            return _googleTokenService.HasValidToken();
        }
    }
}
