using Booking.Application.Contact;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Booking.Web.Controllers
{
    [Authorize(Roles = "Admin")] // Only admins can authenticate
    public class AdminController : Controller
    {
        private readonly IGoogleTokenService _googleTokenService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IGoogleTokenService googleTokenService, ILogger<AdminController> logger)
        {
            _googleTokenService = googleTokenService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult EmailSetup()
        {
            ViewBag.HasValidToken = _googleTokenService.HasValidToken();
            return View();
        }

        [HttpGet]
        public IActionResult AuthenticateGmail()
        {
            var authUrl = _googleTokenService.GetAuthorizationUrl();
            return Redirect(authUrl);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleCallback(string code, string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                TempData["Error"] = $"Authentication failed: {error}";
                return RedirectToAction("EmailSetup");
            }

            if (string.IsNullOrEmpty(code))
            {
                TempData["Error"] = "No authorization code received";
                return RedirectToAction("EmailSetup");
            }

            try
            {
                await _googleTokenService.ExchangeCodeForTokenAsync(code);
                TempData["Success"] = "Gmail authentication successful! Contact form emails will now work.";
                _logger.LogInformation("Gmail authentication completed successfully");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to authenticate with Google";
                _logger.LogError(ex, "Gmail authentication failed");
            }

            return RedirectToAction("EmailSetup");
        }

        [HttpPost]
        public IActionResult ClearTokens()
        {
            _googleTokenService.ClearTokens();
            TempData["Success"] = "Gmail tokens cleared";
            return RedirectToAction("EmailSetup");
        }
    }
}