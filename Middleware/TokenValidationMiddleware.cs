using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using InventarioRopaTipica.Services;

namespace InventarioRopaTipica.Middleware
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenValidationMiddleware> _logger;

        public TokenValidationMiddleware(RequestDelegate next, ILogger<TokenValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IAuthService authService)
        {
            // Rutas que no requieren validación de TokenVersion
            var publicPaths = new[]
            {
                "/api/auth/login",
                "/api/auth/register",
                "/",
                "/home",
                "/swagger"
            };

            var path = context.Request.Path.Value?.ToLower() ?? "";
            
            // Si es una ruta pública, continuar sin validación
            if (publicPaths.Any(p => path.StartsWith(p)))
            {
                await _next(context);
                return;
            }

            // Si el usuario está autenticado, validar TokenVersion
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                try
                {
                    var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                    var tokenVersionClaim = context.User.FindFirst("TokenVersion");

                    if (userIdClaim != null && tokenVersionClaim != null)
                    {
                        int userId = int.Parse(userIdClaim.Value);
                        int tokenVersion = int.Parse(tokenVersionClaim.Value);

                        // Validar TokenVersion contra la base de datos
                        bool isValid = await authService.ValidateTokenVersionAsync(userId, tokenVersion);

                        if (!isValid)
                        {
                            _logger.LogWarning($"Token inválido detectado para usuario {userId}. TokenVersion no coincide.");
                            
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";
                            
                            await context.Response.WriteAsJsonAsync(new
                            {
                                success = false,
                                message = "Token expirado o inválido. Por favor, inicie sesión nuevamente.",
                                code = "TOKEN_VERSION_MISMATCH"
                            });
                            
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error al validar TokenVersion: {ex.Message}");
                    
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        message = "Error al validar el token",
                        code = "TOKEN_VALIDATION_ERROR"
                    });
                    
                    return;
                }
            }

            // Si todo está bien, continuar con el siguiente middleware
            await _next(context);
        }
    }

    // Extension method para registrar el middleware fácilmente
    public static class TokenValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseTokenValidation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenValidationMiddleware>();
        }
    }
}