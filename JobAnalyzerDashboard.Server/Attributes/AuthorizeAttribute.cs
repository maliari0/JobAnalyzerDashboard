using System;
using System.Linq;
using JobAnalyzerDashboard.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

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
