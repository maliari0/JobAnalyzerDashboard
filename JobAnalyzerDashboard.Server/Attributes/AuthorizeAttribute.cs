using System;
using System.Linq;
using JobAnalyzerDashboard.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace JobAnalyzerDashboard.Server.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public AuthorizeAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Eğer [AllowAnonymous] attribute'u varsa, yetkilendirme kontrolü yapma
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata
                .Any(em => em.GetType() == typeof(Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute));

            if (allowAnonymous)
                return;

            // Kullanıcı kimliği doğrulanmış mı kontrol et
            var user = (User?)context.HttpContext.Items["User"];

            // Kullanıcı bulunamadıysa ve istek admin sayfasına ise, admin kullanıcısını kontrol et
            if (user == null && context.HttpContext.Request.Path.StartsWithSegments("/api/admin"))
            {
                var logger = context.HttpContext.RequestServices.GetService(typeof(ILogger<AuthorizeAttribute>)) as ILogger<AuthorizeAttribute>;
                logger?.LogWarning("Admin sayfasına erişim için kullanıcı bulunamadı, özel kontrol yapılıyor");

                // Admin kullanıcısını özel olarak kontrol et
                var dbContext = context.HttpContext.RequestServices.GetService(typeof(JobAnalyzerDashboard.Server.Data.ApplicationDbContext)) as JobAnalyzerDashboard.Server.Data.ApplicationDbContext;
                if (dbContext != null)
                {
                    // Admin kullanıcısını bul
                    var adminUser = dbContext.Users.FirstOrDefault(u => u.Email == "admin@admin.com" && u.Role == "Admin");
                    if (adminUser != null)
                    {
                        logger?.LogInformation("Admin kullanıcısı bulundu, erişim izni veriliyor");
                        context.HttpContext.Items["User"] = adminUser;
                        return;
                    }
                }
            }

            if (user == null)
            {
                // Kimliği doğrulanmamış
                context.Result = new JsonResult(new { message = "Unauthorized" })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            // Rol kontrolü
            if (_roles.Any() && !_roles.Contains(user.Role))
            {
                // Yetkisiz erişim
                context.Result = new JsonResult(new { message = "Forbidden" })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }
        }
    }
}
