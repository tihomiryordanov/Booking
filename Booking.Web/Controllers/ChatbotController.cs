using Booking.Application.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Booking.Web.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly IChatbotService _chatbotService;
        private readonly ILogger<ChatbotController> _logger;

        public ChatbotController(IChatbotService chatbotService, ILogger<ChatbotController> logger)
        {
            _chatbotService = chatbotService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(new { error = "Message cannot be empty" });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
                string response;

                // The ChatbotService now handles routing internally
                // Check if it's a booking request first
                if (IsBookingRequest(request.Message))
                {
                    response = await _chatbotService.ProcessBookingRequestAsync(request.Message, userId);
                }
                else
                {
                    // For villa searches and general queries, use GetResponseAsync
                    // which internally routes to ProcessVillaSearchAsync if needed
                    response = await _chatbotService.GetResponseAsync(request.Message, request.Context);
                }

                return Json(new { response = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chatbot message: {Message}", request.Message);
                return Json(new { response = "I'm sorry, I'm having trouble processing your request right now. Please try again later." });
            }
        }

        private bool IsBookingRequest(string message)
        {
            var bookingKeywords = new[] { "book villa", "reserve villa", "I want to book", "make a booking", "confirm booking" };
            return bookingKeywords.Any(keyword => message.ToLower().Contains(keyword.ToLower()));
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
        public string? Context { get; set; }
    }
}