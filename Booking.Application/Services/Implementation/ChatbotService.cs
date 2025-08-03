using Booking.Application.Services.Interface;
using Booking.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

public class ChatbotService : IChatbotService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IBookingService _bookingService;
    private readonly IVillaService _villaService;

    public ChatbotService(HttpClient httpClient, IConfiguration configuration, IBookingService bookingService, IVillaService villaService)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
        _villaService = villaService ?? throw new ArgumentNullException(nameof(villaService));
        
        var apiKey = _configuration["DeepSeek:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("DeepSeek API key is not configured");
        }
        
        _httpClient.BaseAddress = new Uri("https://api.deepseek.com/");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        _httpClient.Timeout = TimeSpan.FromSeconds(30); // Add timeout
    }

    public async Task<string> GetResponseAsync(string userMessage, string? context = null)
    {
        var systemPrompt = @"
You are a helpful villa booking assistant. You can help users with:
- Finding available villas based on their preferences (dates, budget, occupancy)
- Providing villa details and amenities
- Assisting with booking process
- Checking booking status

When users ask about villa availability or booking, ask for:
1. Check-in and check-out dates
2. Number of guests
3. Budget range (optional)
4. Any specific amenities they need

Keep responses friendly, concise, and helpful. Guide users through the booking process step by step.";

        // Check if the user is asking about villa search or booking
        if (IsVillaSearchQuery(userMessage))
        {
            return await ProcessVillaSearchAsync(userMessage);
        }

        var requestBody = new
        {
            model = "deepseek-chat",
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userMessage }
            },
            max_tokens = 500,
            temperature = 0.7
        };

        if (!string.IsNullOrEmpty(context))
        {
            requestBody = new
            {
                model = "deepseek-chat",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "assistant", content = $"Context: {context}" },
                    new { role = "user", content = userMessage }
                },
                max_tokens = 500,
                temperature = 0.7
            };
        }

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("chat/completions", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var result = JsonSerializer.Deserialize<DeepSeekResponse>(responseContent);
            return result?.choices?.FirstOrDefault()?.message?.content ?? "I'm sorry, I couldn't process your request.";
        }

        return "I'm experiencing technical difficulties. Please try again later.";
    }

    public async Task<string> ProcessVillaSearchAsync(string userMessage)
    {
        var searchParams = await ExtractSearchParametersAsync(userMessage);
        
        IEnumerable<Villa> villas;
        
        if (searchParams.CheckInDate.HasValue && searchParams.Nights.HasValue)
        {
            // Search with availability
            villas = _villaService.SearchVillasWithAvailability(
                searchParams.SearchTerm,
                searchParams.MinPrice,
                searchParams.MaxPrice,
                searchParams.MinOccupancy,
                searchParams.MaxOccupancy,
                searchParams.Nights.Value,
                searchParams.CheckInDate.Value
            );
        }
        else
        {
            // General search without date constraints
            villas = _villaService.SearchVillas(
                searchParams.SearchTerm,
                searchParams.MinPrice,
                searchParams.MaxPrice,
                searchParams.MinOccupancy,
                searchParams.MaxOccupancy
            );
        }

        return await FormatVillaSearchResults(villas.Take(5), searchParams);
    }

    public async Task<string> ProcessBookingRequestAsync(string userMessage, string userId)
    {
        var bookingParams = await ExtractBookingParametersAsync(userMessage);
        
        if (bookingParams.VillaId.HasValue && bookingParams.CheckInDate.HasValue && 
            bookingParams.CheckOutDate.HasValue && !string.IsNullOrEmpty(bookingParams.Name) && 
            !string.IsNullOrEmpty(bookingParams.Email))
        {
            var villa = _villaService.GetVillaById(bookingParams.VillaId.Value);
            if (villa != null)
            {
                var nights = (bookingParams.CheckOutDate.Value.ToDateTime(TimeOnly.MinValue) - 
                             bookingParams.CheckInDate.Value.ToDateTime(TimeOnly.MinValue)).Days;
                
                var isAvailable = _villaService.IsVillaAvailableByDate(
                    bookingParams.VillaId.Value, 
                    nights, 
                    bookingParams.CheckInDate.Value
                );

                if (isAvailable)
                {
                    var totalCost = villa.Price * nights;
                    return $"✅ Great! Villa '{villa.Name}' is available from {bookingParams.CheckInDate.Value:MMM dd} to {bookingParams.CheckOutDate.Value:MMM dd}.\n\n" +
                           $"📋 **Booking Summary:**\n" +
                           $"• Villa: {villa.Name}\n" +
                           $"• Dates: {bookingParams.CheckInDate.Value:MMM dd} - {bookingParams.CheckOutDate.Value:MMM dd} ({nights} nights)\n" +
                           $"• Total Cost: ${totalCost:F2}\n\n" +
                           $"🔗 [View Villa Details & Book Now](/Home/VillaDetails/{bookingParams.VillaId.Value}?checkInDate={bookingParams.CheckInDate.Value:yyyy-MM-dd}&nights={nights})\n\n" +
                           $"To complete your booking, please click the link above or provide payment details.";
                }
                else
                {
                    return $"❌ Sorry, Villa '{villa.Name}' is not available for the selected dates. " +
                           $"Let me help you find alternative options with similar features.";
                }
            }
        }

        return "To help you with booking, I need:\n" +
               "• Villa ID or name\n" +
               "• Check-in and check-out dates\n" +
               "• Your name and email\n\n" +
               "Please provide these details so I can assist you with the booking.";
    }

    private bool IsVillaSearchQuery(string message)
    {
        var searchKeywords = new[] { "villa", "search", "available", "find", "book", "stay", "accommodation", "room" };
        return searchKeywords.Any(keyword => message.ToLower().Contains(keyword));
    }

    private async Task<SearchParameters> ExtractSearchParametersAsync(string userMessage)
    {
        var extractionPrompt = $@"
Extract search parameters from this message: '{userMessage}'

Return JSON with these fields (use null for missing values):
{{
    ""searchTerm"": ""string or null"",
    ""minPrice"": number or null,
    ""maxPrice"": number or null,
    ""minOccupancy"": number or null,
    ""maxOccupancy"": number or null,
    ""checkInDate"": ""YYYY-MM-DD or null"",
    ""nights"": number or null
}}

Examples:
- ""Looking for a villa for 4 people"" → {{""minOccupancy"": 4}}
- ""Something under $200"" → {{""maxPrice"": 200}}
- ""Check in March 15 for 3 nights"" → {{""checkInDate"": ""2025-03-15"", ""nights"": 3}}";

        var requestBody = new
        {
            model = "deepseek-chat",
            messages = new[]
            {
                new { role = "system", content = "You are a parameter extraction assistant. Return only valid JSON." },
                new { role = "user", content = extractionPrompt }
            },
            max_tokens = 200,
            temperature = 0.1
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("chat/completions", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var result = JsonSerializer.Deserialize<DeepSeekResponse>(responseContent);
            var jsonResponse = result?.choices?.FirstOrDefault()?.message?.content ?? "{}";

            try
            {
                return JsonSerializer.Deserialize<SearchParameters>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new SearchParameters();
            }
            catch
            {
                return new SearchParameters();
            }
        }

        return new SearchParameters();
    }

    private async Task<BookingParameters> ExtractBookingParametersAsync(string userMessage)
    {
        var extractionPrompt = $@"
Extract booking parameters from this message: '{userMessage}'

Return JSON with these fields (use null for missing values):
{{
    ""villaId"": number or null,
    ""villaName"": ""string or null"",
    ""checkInDate"": ""YYYY-MM-DD or null"",
    ""checkOutDate"": ""YYYY-MM-DD or null"",
    ""name"": ""string or null"",
    ""email"": ""string or null"",
    ""phone"": ""string or null""
}}";

        var requestBody = new
        {
            model = "deepseek-chat",
            messages = new[]
            {
                new { role = "system", content = "You are a parameter extraction assistant. Return only valid JSON." },
                new { role = "user", content = extractionPrompt }
            },
            max_tokens = 200,
            temperature = 0.1
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("chat/completions", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var result = JsonSerializer.Deserialize<DeepSeekResponse>(responseContent);
            var jsonResponse = result?.choices?.FirstOrDefault()?.message?.content ?? "{}";

            try
            {
                return JsonSerializer.Deserialize<BookingParameters>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new BookingParameters();
            }
            catch
            {
                return new BookingParameters();
            }
        }

        return new BookingParameters();
    }

    private async Task<string> FormatVillaSearchResults(IEnumerable<Villa> villas, SearchParameters searchParams)
    {
        if (!villas.Any())
        {
            return "🏠 I couldn't find any villas matching your criteria. Try adjusting your search parameters:\n" +
                   "• Different dates\n" +
                   "• Broader price range\n" +
                   "• Flexible occupancy requirements";
        }

        var resultBuilder = new StringBuilder();
        resultBuilder.AppendLine("🏠 **Here are the available villas for you:**\n");

        foreach (var villa in villas)
        {
            resultBuilder.AppendLine($"**{villa.Name}** (ID: {villa.Id})");
            resultBuilder.AppendLine($"💰 ${villa.Price:F2}/night");
            resultBuilder.AppendLine($"👥 Up to {villa.Occupancy} guests");
            resultBuilder.AppendLine($"📐 {villa.SquareFeet} sq ft");
            
            if (!string.IsNullOrEmpty(villa.Description))
            {
                var shortDescription = villa.Description.Length > 100 
                    ? villa.Description[..100] + "..." 
                    : villa.Description;
                resultBuilder.AppendLine($"📝 {shortDescription}");
            }
            
            // Generate the correct link to your VillaDetails action in HomeController
            var detailsLink = $"/Home/VillaDetails/{villa.Id}";
            
            // If we have search dates, include them in the link
            if (searchParams.CheckInDate.HasValue && searchParams.Nights.HasValue)
            {
                detailsLink += $"?checkInDate={searchParams.CheckInDate.Value:yyyy-MM-dd}&nights={searchParams.Nights.Value}";
            }
            
            resultBuilder.AppendLine($"🔗 [View Details & Book]({detailsLink})");
            resultBuilder.AppendLine();
        }

        if (searchParams.CheckInDate.HasValue && searchParams.Nights.HasValue)
        {
            resultBuilder.AppendLine($"📅 Showing availability for {searchParams.CheckInDate.Value:MMM dd, yyyy} ({searchParams.Nights.Value} nights)\n");
        }

        resultBuilder.AppendLine("💬 **To book a villa, just say:** \"I want to book villa [ID] from [check-in date] to [check-out date]\"");
        
        return resultBuilder.ToString();
    }

    private class DeepSeekResponse
    {
        public Choice[]? choices { get; set; }
    }

    private class Choice
    {
        public Message? message { get; set; }
    }

    private class Message
    {
        public string? content { get; set; }
    }

    private class SearchParameters
    {
        public string? SearchTerm { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public int? MinOccupancy { get; set; }
        public int? MaxOccupancy { get; set; }
        public DateOnly? CheckInDate { get; set; }
        public int? Nights { get; set; }
    }

    private class BookingParameters
    {
        public int? VillaId { get; set; }
        public string? VillaName { get; set; }
        public DateOnly? CheckInDate { get; set; }
        public DateOnly? CheckOutDate { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}