using Booking.Application.Contact;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.Extensions.Options;

namespace Booking.Infrastructure.Emails
{
    public class GoogleTokenService : IGoogleTokenService
    {
        private readonly GoogleOAuthSettings _oauthSettings;
        private TokenResponse? _currentToken;
        private readonly object _tokenLock = new object();

        public GoogleTokenService(IOptions<GoogleOAuthSettings> oauthSettings)
        {
            _oauthSettings = oauthSettings.Value;
        }

        public async Task<string?> GetValidAccessTokenAsync()
        {
            lock (_tokenLock)
            {
                if (_currentToken != null && IsTokenValid(_currentToken))
                {
                    return _currentToken.AccessToken;
                }
            }
            return null;
        }

        public async Task<TokenResponse> ExchangeCodeForTokenAsync(string authorizationCode)
        {
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _oauthSettings.ClientId,
                    ClientSecret = _oauthSettings.ClientSecret
                },
                Scopes = new[] { "https://www.googleapis.com/auth/gmail.send" }
            });

            var token = await flow.ExchangeCodeForTokenAsync(
                "user", 
                authorizationCode, 
                _oauthSettings.RedirectUri, 
                CancellationToken.None);

            lock (_tokenLock)
            {
                _currentToken = token;
            }

            return token;
        }

        public async Task<TokenResponse?> RefreshTokenAsync()
        {
            lock (_tokenLock)
            {
                if (_currentToken?.RefreshToken == null)
                    return null;
            }

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _oauthSettings.ClientId,
                    ClientSecret = _oauthSettings.ClientSecret
                },
                Scopes = new[] { "https://www.googleapis.com/auth/gmail.send" }
            });

            try
            {
                var refreshToken = _currentToken!.RefreshToken;
                var newToken = await flow.RefreshTokenAsync("user", refreshToken, CancellationToken.None);
                
                lock (_tokenLock)
                {
                    _currentToken = newToken;
                }

                return newToken;
            }
            catch
            {
                lock (_tokenLock)
                {
                    _currentToken = null;
                }
                return null;
            }
        }

        public string GetAuthorizationUrl()
        {
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _oauthSettings.ClientId,
                    ClientSecret = _oauthSettings.ClientSecret
                },
                Scopes = new[] { "https://www.googleapis.com/auth/gmail.send" }
            });

            var authUrl = flow.CreateAuthorizationCodeRequest(_oauthSettings.RedirectUri);
            
            var uriBuilder = new UriBuilder(authUrl.Build());
            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            query["access_type"] = "offline";
            query["prompt"] = "consent";
            uriBuilder.Query = query.ToString();

            return uriBuilder.ToString();
        }

        public bool HasValidToken()
        {
            lock (_tokenLock)
            {
                return _currentToken != null && IsTokenValid(_currentToken);
            }
        }

        private bool IsTokenValid(TokenResponse token)
        {
            if (string.IsNullOrEmpty(token.AccessToken))
                return false;

            var expiresAt = token.IssuedUtc.AddSeconds(token.ExpiresInSeconds ?? 3600);
            return DateTime.UtcNow < expiresAt.AddMinutes(-5);
        }

        public void ClearTokens()
        {
            lock (_tokenLock)
            {
                _currentToken = null;
            }
        }
    }
}