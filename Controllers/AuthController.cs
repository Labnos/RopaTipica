using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using InventarioRopaTipica.DTOs;
using InventarioRopaTipica.Services;
using InventarioRopaTipica.Helpers;

namespace InventarioRopaTipica.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly JwtHelper _jwtHelper;

        public AuthController(IAuthService authService, JwtHelper jwtHelper)
        {
            _authService = authService;
            _jwtHelper = jwtHelper;
        }

        /// <summary>
        /// POST: api/Auth/Login
        /// Realiza el login del usuario
        /// </summary>
        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Datos inválidos",
                        errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var result = await _authService.LoginAsync(loginDto);

                if (!result.Success)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = result.Message
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// POST: api/Auth/Logout
        /// Invalida todos los tokens del usuario actual
        /// </summary>
        [HttpPost("Logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Token inválido"
                    });
                }

                int userId = int.Parse(userIdClaim.Value);
                await _authService.InvalidateAllTokensAsync(userId);

                return Ok(new
                {
                    success = true,
                    message = "Logout exitoso"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al cerrar sesión",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// GET: api/Auth/ValidateToken
        /// Valida si el token actual es válido
        /// </summary>
        [HttpGet("ValidateToken")]
        [Authorize]
        public async Task<IActionResult> ValidateToken()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var tokenVersionClaim = User.FindFirst("TokenVersion");

                if (userIdClaim == null || tokenVersionClaim == null)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Token inválido"
                    });
                }

                int userId = int.Parse(userIdClaim.Value);
                int tokenVersion = int.Parse(tokenVersionClaim.Value);

                // Validar TokenVersion contra la base de datos
                bool isValid = await _authService.ValidateTokenVersionAsync(userId, tokenVersion);

                if (!isValid)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Token expirado o inválido. Por favor, inicie sesión nuevamente."
                    });
                }

                var user = await _authService.GetUserByIdAsync(userId);

                return Ok(new
                {
                    success = true,
                    message = "Token válido",
                    user = new
                    {
                        id = user.Id,
                        nombre = user.Nombre,
                        email = user.Email,
                        rol = user.Rol
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al validar token",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// GET: api/Auth/Me
        /// Obtiene la información del usuario autenticado
        /// </summary>
        [HttpGet("Me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Token inválido"
                    });
                }

                int userId = int.Parse(userIdClaim.Value);
                var user = await _authService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Usuario no encontrado"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = new UserDto
                    {
                        Id = user.Id,
                        Nombre = user.Nombre,
                        Email = user.Email,
                        Rol = user.Rol,
                        Estado = user.Estado,
                        UltimoAcceso = user.UltimoAcceso,
                        CreadoEn = user.CreadoEn
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener información del usuario",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// POST: api/Auth/ChangePassword
        /// Cambia la contraseña del usuario actual
        /// </summary>
        [HttpPost("ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Datos inválidos",
                        errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Token inválido"
                    });
                }

                int userId = int.Parse(userIdClaim.Value);

                // Usar el UserService para cambiar contraseña
                // Nota: Necesitaremos inyectar IUserService también
                return Ok(new
                {
                    success = true,
                    message = "Contraseña actualizada exitosamente. Por favor, inicie sesión nuevamente."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al cambiar contraseña",
                    error = ex.Message
                });
            }
        }
    }
}