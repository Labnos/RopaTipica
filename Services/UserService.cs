using Microsoft.EntityFrameworkCore;
using InventarioRopaTipica.Data;
using InventarioRopaTipica.DTOs;
using InventarioRopaTipica.Helpers;
using InventarioRopaTipica.Models;

namespace InventarioRopaTipica.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<UserDto>>> GetAllUsersAsync()
        {
            try
            {
                var users = await _context.Users
                    .Where(u => u.Estado)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        Nombre = u.Nombre,
                        Email = u.Email,
                        Rol = u.Rol,
                        Estado = u.Estado,
                        UltimoAcceso = u.UltimoAcceso,
                        CreadoEn = u.CreadoEn
                    })
                    .OrderBy(u => u.Nombre)
                    .ToListAsync();

                return ApiResponse<List<UserDto>>.SuccessResponse(users);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<UserDto>>.ErrorResponse($"Error al obtener usuarios: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UserDto>> GetUserByIdAsync(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return ApiResponse<UserDto>.ErrorResponse("Usuario no encontrado");

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Nombre = user.Nombre,
                    Email = user.Email,
                    Rol = user.Rol,
                    Estado = user.Estado,
                    UltimoAcceso = user.UltimoAcceso,
                    CreadoEn = user.CreadoEn
                };

                return ApiResponse<UserDto>.SuccessResponse(userDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<UserDto>.ErrorResponse($"Error al obtener usuario: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserDto createUserDto)
        {
            try
            {
                // Verificar si el email ya existe
                if (await _context.Users.AnyAsync(u => u.Email.ToLower() == createUserDto.Email.ToLower()))
                    return ApiResponse<UserDto>.ErrorResponse("El email ya está registrado");

                // Crear nuevo usuario
                var user = new User
                {
                    Nombre = createUserDto.Nombre,
                    Email = createUserDto.Email,
                    PasswordHash = PasswordHelper.HashPassword(createUserDto.Password),
                    Rol = createUserDto.Rol,
                    Estado = true,
                    TokenVersion = 0,
                    CreadoEn = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Nombre = user.Nombre,
                    Email = user.Email,
                    Rol = user.Rol,
                    Estado = user.Estado,
                    CreadoEn = user.CreadoEn
                };

                return ApiResponse<UserDto>.SuccessResponse(userDto, "Usuario creado exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserDto>.ErrorResponse($"Error al crear usuario: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UserDto>> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return ApiResponse<UserDto>.ErrorResponse("Usuario no encontrado");

                // Actualizar campos
                if (!string.IsNullOrEmpty(updateUserDto.Nombre))
                    user.Nombre = updateUserDto.Nombre;

                if (!string.IsNullOrEmpty(updateUserDto.Email))
                {
                    // Verificar que el email no esté en uso
                    if (await _context.Users.AnyAsync(u => u.Email.ToLower() == updateUserDto.Email.ToLower() && u.Id != id))
                        return ApiResponse<UserDto>.ErrorResponse("El email ya está en uso");
                    
                    user.Email = updateUserDto.Email;
                }

                if (!string.IsNullOrEmpty(updateUserDto.Rol))
                    user.Rol = updateUserDto.Rol;

                if (updateUserDto.Estado.HasValue)
                    user.Estado = updateUserDto.Estado.Value;

                await _context.SaveChangesAsync();

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Nombre = user.Nombre,
                    Email = user.Email,
                    Rol = user.Rol,
                    Estado = user.Estado,
                    UltimoAcceso = user.UltimoAcceso,
                    CreadoEn = user.CreadoEn
                };

                return ApiResponse<UserDto>.SuccessResponse(userDto, "Usuario actualizado exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserDto>.ErrorResponse($"Error al actualizar usuario: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteUserAsync(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return ApiResponse<bool>.ErrorResponse("Usuario no encontrado");

                // Soft delete - solo desactivar
                user.Estado = false;
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Usuario eliminado exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error al eliminar usuario: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return ApiResponse<bool>.ErrorResponse("Usuario no encontrado");

                // Verificar contraseña actual
                if (!PasswordHelper.VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
                    return ApiResponse<bool>.ErrorResponse("La contraseña actual es incorrecta");

                // Actualizar contraseña
                user.PasswordHash = PasswordHelper.HashPassword(changePasswordDto.NewPassword);
                
                // Invalidar todos los tokens anteriores
                user.TokenVersion++;
                
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Contraseña actualizada exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error al cambiar contraseña: {ex.Message}");
            }
        }
    }
}