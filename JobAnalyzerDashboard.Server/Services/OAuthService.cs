using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using JobAnalyzerDashboard.Server.Data;
using JobAnalyzerDashboard.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JobAnalyzerDashboard.Server.Services
{
    public class OAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OAuthService> _logger;

        public OAuthService(IConfiguration configuration, ApplicationDbContext context, ILogger<OAuthService> logger)
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;
        }

        public string GetAuthorizationUrl(string provider, int profileId)
        {
            if (provider.ToLower() == "google")
            {
                var clientId = _configuration["Authentication:Google:ClientId"];
                var redirectUri = _configuration["Authentication:Google:RedirectUri"];

                // Düzeltme: Scopes'u doğrudan string olarak al
                string scopeString;
                var scopesConfig = _configuration["Authentication:Google:Scopes"];

                if (!string.IsNullOrEmpty(scopesConfig))
                {
                    // Eğer zaten boşluklarla ayrılmış bir string ise, doğrudan kullan
                    scopeString = scopesConfig;
                }
                else
                {
                    // Eğer null ise, varsayılan scope'ları kullan
                    scopeString = "https://www.googleapis.com/auth/gmail.send https://www.googleapis.com/auth/userinfo.email https://www.googleapis.com/auth/userinfo.profile";
                }

                _logger.LogInformation("Google OAuth scopes: {Scopes}", scopeString);

                var state = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{provider}:{profileId}"));

                return $"https://accounts.google.com/o/oauth2/v2/auth?client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope={Uri.EscapeDataString(scopeString)}&access_type=offline&prompt=consent&state={state}";
            }

            throw new NotSupportedException($"Provider {provider} is not supported");
        }

        public async Task<OAuthToken> ExchangeCodeForTokenAsync(string provider, string code, int profileId)
        {
            if (provider.ToLower() == "google")
            {
                var clientId = _configuration["Authentication:Google:ClientId"];
                var clientSecret = _configuration["Authentication:Google:ClientSecret"];
                var redirectUri = _configuration["Authentication:Google:RedirectUri"];

                var tokenEndpoint = "https://oauth2.googleapis.com/token";

                using (var client = new HttpClient())
                {
                    var content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "code", code },
                        { "client_id", clientId },
                        { "client_secret", clientSecret },
                        { "redirect_uri", redirectUri },
                        { "grant_type", "authorization_code" }
                    });

                    var response = await client.PostAsync(tokenEndpoint, content);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError("Failed to exchange code for token: {Response}", responseString);
                        throw new Exception($"Failed to exchange code for token: {responseString}");
                    }

                    var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseString);

                    var accessToken = tokenResponse.GetProperty("access_token").GetString();
                    var refreshToken = tokenResponse.TryGetProperty("refresh_token", out var refreshTokenElement)
                        ? refreshTokenElement.GetString()
                        : null;
                    var expiresIn = tokenResponse.GetProperty("expires_in").GetInt32();

                    // Get user email
                    var userInfoResponse = await client.GetAsync($"https://www.googleapis.com/oauth2/v2/userinfo?access_token={accessToken}");
                    var userInfoString = await userInfoResponse.Content.ReadAsStringAsync();
                    var userInfo = JsonSerializer.Deserialize<JsonElement>(userInfoString);
                    var email = userInfo.GetProperty("email").GetString();

                    // Check if token already exists
                    var existingToken = await _context.OAuthTokens
                        .FirstOrDefaultAsync(t => t.ProfileId == profileId && t.Provider == provider);

                    if (existingToken != null)
                    {
                        existingToken.AccessToken = accessToken;
                        if (!string.IsNullOrEmpty(refreshToken))
                        {
                            existingToken.RefreshToken = refreshToken;
                        }
                        existingToken.ExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn);
                        existingToken.Email = email;

                        _context.OAuthTokens.Update(existingToken);
                        await _context.SaveChangesAsync();

                        return existingToken;
                    }
                    else
                    {
                        // Düzeltme: Scope'u doğrudan string olarak al
                        string scopeString;
                        var scopesConfig = _configuration["Authentication:Google:Scopes"];

                        if (!string.IsNullOrEmpty(scopesConfig))
                        {
                            scopeString = scopesConfig;
                        }
                        else
                        {
                            scopeString = "https://www.googleapis.com/auth/gmail.send https://www.googleapis.com/auth/userinfo.email https://www.googleapis.com/auth/userinfo.profile";
                        }

                        var newToken = new OAuthToken
                        {
                            ProfileId = profileId,
                            Provider = provider,
                            AccessToken = accessToken,
                            RefreshToken = refreshToken ?? string.Empty,
                            ExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn),
                            Email = email,
                            Scope = scopeString
                        };

                        _context.OAuthTokens.Add(newToken);
                        await _context.SaveChangesAsync();

                        return newToken;
                    }
                }
            }

            throw new NotSupportedException($"Provider {provider} is not supported");
        }
    

        public async Task<string> RefreshTokenAsync(OAuthToken token)
        {
            if (token.Provider.ToLower() == "google")
            {
                var clientId = _configuration["Authentication:Google:ClientId"];
                var clientSecret = _configuration["Authentication:Google:ClientSecret"];

                var tokenEndpoint = "https://oauth2.googleapis.com/token";

                using (var client = new HttpClient())
                {
                    var content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "client_id", clientId },
                        { "client_secret", clientSecret },
                        { "refresh_token", token.RefreshToken },
                        { "grant_type", "refresh_token" }
                    });

                    var response = await client.PostAsync(tokenEndpoint, content);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError("Failed to refresh token: {Response}", responseString);
                        throw new Exception($"Failed to refresh token: {responseString}");
                    }

                    var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseString);

                    var accessToken = tokenResponse.GetProperty("access_token").GetString();
                    var expiresIn = tokenResponse.GetProperty("expires_in").GetInt32();

                    token.AccessToken = accessToken;
                    token.ExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn);

                    _context.OAuthTokens.Update(token);
                    await _context.SaveChangesAsync();

                    return accessToken;
                }
            }

            throw new NotSupportedException($"Provider {token.Provider} is not supported");
        }

        public async Task<string> GetValidAccessTokenAsync(int profileId, string provider)
        {
            var token = await _context.OAuthTokens
                .FirstOrDefaultAsync(t => t.ProfileId == profileId && t.Provider == provider);

            if (token == null)
            {
                throw new Exception($"No OAuth token found for profile {profileId} and provider {provider}");
            }

            if (token.ExpiresAt <= DateTime.UtcNow.AddMinutes(5))
            {
                return await RefreshTokenAsync(token);
            }

            return token.AccessToken;
        }

        public async Task<List<OAuthToken>> GetOAuthTokensAsync(int profileId)
        {
            return await _context.OAuthTokens
                .Where(t => t.ProfileId == profileId)
                .ToListAsync();
        }

        public async Task<OAuthToken?> GetOAuthTokenByProfileIdAndProvider(int profileId, string provider)
        {
            return await _context.OAuthTokens
                .FirstOrDefaultAsync(t => t.ProfileId == profileId && t.Provider == provider);
        }

        public async Task<bool> RevokeTokenAsync(int profileId, string provider)
        {
            var token = await _context.OAuthTokens
                .FirstOrDefaultAsync(t => t.ProfileId == profileId && t.Provider == provider);

            if (token == null)
            {
                return false;
            }

            _context.OAuthTokens.Remove(token);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
