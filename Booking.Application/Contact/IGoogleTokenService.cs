using Google.Apis.Auth.OAuth2.Responses;

namespace Booking.Application.Contact
{
    public interface IGoogleTokenService
    {
        Task<string?> GetValidAccessTokenAsync();
        Task<TokenResponse> ExchangeCodeForTokenAsync(string authorizationCode);
        Task<TokenResponse?> RefreshTokenAsync();
        string GetAuthorizationUrl();
        bool HasValidToken();
        void ClearTokens();
    }
}