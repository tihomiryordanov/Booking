using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Booking.Web.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("Error")]
        [Route("Error/Index")]
        public IActionResult Index()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            _logger.LogError("General error occurred. Request ID: {RequestId}", requestId);
            
            ViewBag.RequestId = requestId;
            return View();
        }

        [Route("Error/404")]
        public new IActionResult NotFound()
        {
            var requestedUrl = HttpContext.Request.Path;
            _logger.LogWarning("404 Not Found: {RequestedUrl} from {UserAgent}", 
                requestedUrl, HttpContext.Request.Headers.UserAgent);
            
            ViewBag.RequestedUrl = requestedUrl;
            Response.StatusCode = 404;
            
            return View();
        }

        [Route("Error/{statusCode:int}")]
        public IActionResult HandleStatusCode(int statusCode)
        {
            var requestedUrl = HttpContext.Request.Path;
            
            _logger.LogWarning("Status code {StatusCode} for URL: {RequestedUrl}", 
                statusCode, requestedUrl);
            
            ViewBag.StatusCode = statusCode;
            ViewBag.RequestedUrl = requestedUrl;
            Response.StatusCode = statusCode;

            return statusCode switch
            {
                404 => View("NotFound"),
                403 => View("Forbidden"),
                500 => View("InternalServerError"),
                _ => View("Index")
            };
        }

        [Route("Error/403")]
        public IActionResult Forbidden()
        {
            Response.StatusCode = 403;
            return View();
        }

        [Route("Error/500")]
        public IActionResult InternalServerError()
        {
            Response.StatusCode = 500;
            return View();
        }
    }
}