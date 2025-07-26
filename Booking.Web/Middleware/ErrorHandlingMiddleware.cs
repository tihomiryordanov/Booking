using System.Net;
using System.Text.Json;

namespace Booking.Web.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
                
                // Handle 404 for unmatched routes
                if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
                {
                    await HandleNotFoundAsync(context);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleNotFoundAsync(HttpContext context)
        {
            context.Response.StatusCode = 404;
            
            if (IsApiRequest(context))
            {
                await WriteJsonResponseAsync(context, "Resource not found", 404);
            }
            else
            {
                context.Response.Redirect("/Error/404");
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.StatusCode = 500;
            
            if (IsApiRequest(context))
            {
                await WriteJsonResponseAsync(context, "Internal server error", 500);
            }
            else
            {
                context.Response.Redirect("/Error/500");
            }
        }

        private static bool IsApiRequest(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments("/api") ||
                   context.Request.Headers.Accept.Any(h => h.Contains("application/json"));
        }

        private static async Task WriteJsonResponseAsync(HttpContext context, string message, int statusCode)
        {
            context.Response.ContentType = "application/json";
            
            var response = new
            {
                error = new
                {
                    message = message,
                    statusCode = statusCode,
                    timestamp = DateTime.UtcNow
                }
            };

            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }
    }
}