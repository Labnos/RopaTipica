using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using InventarioRopaTipica.DTOs;
using InventarioRopaTipica.Services;

namespace InventarioRopaTipica.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requiere autenticación para todas las rutas
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// GET: api/Users
        /// Obtiene todos los usuarios activos
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var result = await _userService.GetAllUsersAsync();
                
                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener usuarios",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// GET: api/Users/{id}
        /// Obtiene un usuario por ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var result = await _userService.GetUserByIdAsync(id);
                
                if (!result.Success)
                    return NotFound(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener usuario",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// POST: api/Users
        /// Crea un nuevo usuario
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
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

                // Validar rol
                var rolesValidos = new[] { "Administrador", "Vendedor", "Encargado" };
                if (!rolesValidos.Contains(createUserDto.Rol))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Rol inválido. Roles permitidos: Administrador, Vendedor, Encargado"
                    });
                }

                var result = await _userService.CreateUserAsync(createUserDto);
                
                if (!result.Success)
                    return BadRequest(result);

                return CreatedAtAction(nameof(GetUserById), new { id = result.Data.Id }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al crear usuario",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// PUT: api/Users/{id}
        /// Actualiza un usuario existente
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
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

                // Validar rol si se está actualizando
                if (!string.IsNullOrEmpty(updateUserDto.Rol))
                {
                    var rolesValidos = new[] { "Administrador", "Vendedor", "Encargado" };
                    if (!rolesValidos.Contains(updateUserDto.Rol))
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Rol inválido. Roles permitidos: Administrador, Vendedor, Encargado"
                        });
                    }
                }

                var result = await _userService.UpdateUserAsync(id, updateUserDto);
                
                if (!result.Success)
                    return NotFound(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al actualizar usuario",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// DELETE: api/Users/{id}
        /// Elimina (desactiva) un usuario
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                // Verificar que no se esté eliminando a sí mismo
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (currentUserIdClaim != null)
                {
                    int currentUserId = int.Parse(currentUserIdClaim.Value);
                    if (currentUserId == id)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "No puedes eliminar tu propio usuario"
                        });
                    }
                }

                var result = await _userService.DeleteUserAsync(id);
                
                if (!result.Success)
                    return NotFound(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al eliminar usuario",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// POST: api/Users/{id}/ChangePassword
        /// Cambia la contraseña de un usuario (solo Administrador)
        /// </summary>
        [HttpPost("{id}/ChangePassword")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ChangeUserPassword(int id, [FromBody] ChangePasswordDto changePasswordDto)
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

                var result = await _userService.ChangePasswordAsync(id, changePasswordDto);
                
                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
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