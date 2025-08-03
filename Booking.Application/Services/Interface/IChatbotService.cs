namespace Booking.Application.Services.Interface
{
    public interface IChatbotService
    {
        Task<string> GetResponseAsync(string userMessage, string? context = null);
        Task<string> ProcessVillaSearchAsync(string userMessage);
        Task<string> ProcessBookingRequestAsync(string userMessage, string userId);
    }
}
