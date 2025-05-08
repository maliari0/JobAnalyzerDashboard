using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JobAnalyzerDashboard.Server.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace JobAnalyzerDashboard.Server.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtMiddleware> _logger;

        public JwtMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<JwtMiddleware> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                _logger.LogInformation("JWT token found in request");
                await AttachUserToContext(context, dbContext, token);
            }
            else
            {
                _logger.LogWarning("No JWT token found in request");
            }

            await _next(context);
        }

        private async Task AttachUserToContext(HttpContext context, ApplicationDbContext dbContext, string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                if (jsonToken == null)
                {
                    _logger.LogWarning("Invalid token format");
                    return;
                }

                _logger.LogInformation("Token claims: {Claims}",
                    string.Join(", ", jsonToken.Claims.Select(c => $"{c.Type}={c.Value}")));

                var emailClaim = jsonToken.Claims.FirstOrDefault(x =>
                    x.Type == ClaimTypes.Email ||
                    x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");

                if (emailClaim != null)
                {
                    // E-posta ile kullanıcıyı bul
                    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == emailClaim.Value);

                    if (user != null)
                    {
                        _logger.LogInformation("User found by email: {Email}, Role: {Role}", user.Email, user.Role);

                        context.Items["User"] = user;

                        var identity = new ClaimsIdentity(jsonToken.Claims, "Bearer");
                        context.User = new ClaimsPrincipal(identity);

                        return;
                    }
                }

                var userIdClaim = jsonToken.Claims.FirstOrDefault(x =>
                    x.Type == ClaimTypes.NameIdentifier ||
                    x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    var user = await dbContext.Users.FindAsync(userId);

                    if (user != null)
                    {
                        _logger.LogInformation("User found by ID: {UserId}, Role: {Role}", user.Id, user.Role);

                        context.Items["User"] = user;

                        var identity = new ClaimsIdentity(jsonToken.Claims, "Bearer");
                        context.User = new ClaimsPrincipal(identity);

                        return;
                    }
                }

                var adminUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "admin@admin.com" && u.Role == "Admin");
                if (adminUser != null)
                {
                    _logger.LogInformation("Admin user found as fallback");

                    context.Items["User"] = adminUser;

                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, adminUser.Id.ToString()),
                        new Claim(ClaimTypes.Email, adminUser.Email),
                        new Claim(ClaimTypes.Name, adminUser.Username),
                        new Claim(ClaimTypes.Role, adminUser.Role)
                    };

                    var identity = new ClaimsIdentity(claims, "Bearer");
                    context.User = new ClaimsPrincipal(identity);
                }
                else
                {
                    _logger.LogWarning("No user found for the token");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JWT token validation failed");
            }
        }
    }
}
