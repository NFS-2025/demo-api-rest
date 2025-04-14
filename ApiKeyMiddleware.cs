using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace demo_api_rest
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext context)
        {
            // Vérifier si l'endpoint est marqué comme AllowAnonymous
            var endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                var allowAnonymous = endpoint.Metadata.GetMetadata<IAllowAnonymous>();
                var controllerActionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                
                // Si l'action ou le contrôleur est marqué avec AllowAnonymous, on laisse passer
                if (allowAnonymous != null || 
                    (controllerActionDescriptor != null && 
                     (controllerActionDescriptor.MethodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any() ||
                      controllerActionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any())))
                {
                    await _next(context);
                    return;
                }
            }
            
            // Pour les autres endpoints, on vérifie l'API key
            var apiKey = context.Request.Headers["x-api-key"].FirstOrDefault();

            if (!string.IsNullOrEmpty(apiKey) && _configuration.GetSection("AllowedApiKeys").Get<string[]>()!.Contains(apiKey))
            {
                await _next(context);
            }
            else
            {
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsync("Unauthorized access");
            }
        }
    }
    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ApiKeyMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiKeyMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiKeyMiddleware>();
        }
    }
}
