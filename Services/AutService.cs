using Microsoft.EntityFrameworkCore;
using InventarioRopaTipica.Data;
using InventarioRopaTipica.DTOs;
using InventarioRopaTipica.Helpers;
using InventarioRopaTipica.Models;

namespace InventarioRopaTipica.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtHelper _jwtHelper;

        public AuthService(ApplicationDbContext context, JwtHelper jwtHelper)
        {
            _context = context;
            _jwtHelper = jwtHelper;
        }

        /// <summary>
        /// Realiza el login del usuario
        /// </summary>
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginDto)
        {
            try
            {
                // Buscar usuario por email
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == loginDto.Email.ToLower());

                // Verificar si existe el usuario
                if (user == null)
                {
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Credenciales inválidas"
                    };
                }

                // Verificar si el usuario está activo
                if (!user.Estado)
                {
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Usuario inactivo. Contacte al administrador."
                    };
                }

                // Verificar contraseña
                if (!PasswordHelper.VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Credenciales inválidas"
                    };
                }

                // Generar nuevo token JWT
                var token = _jwtHelper.GenerateToken(user);
                var expiresAt = _jwtHelper.GetTokenExpiration();

                // Actualizar último acceso
                user.UltimoAcceso = DateTime.Now;
                await _context.SaveChangesAsync();

                // Retornar respuesta exitosa
                return new LoginResponseDto
                {
                    Success = true,
                    Message = "Login exitoso",
                    Token = token,
                    TokenVersion = user.TokenVersion,
                    ExpiresAt = expiresAt,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Nombre = user.Nombre,
                        Email = user.Email,
                        Rol = user.Rol,
                        Estado = user.Estado,
                        UltimoAcceso = user.UltimoAcceso,
                        CreadoEn = user.CreadoEn
                    }
                };
            }
            catch (Exception ex)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = $"Error al procesar login: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Valida que el TokenVersion del JWT coincida con el de la base de datos
        /// (Política de no reutilización de tokens)
        /// </summary>
        public async Task<bool> ValidateTokenVersionAsync(int userId, int tokenVersion)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            // Si el TokenVersion del token NO coincide con el de la BD, el token es inválido
            return user.TokenVersion == tokenVersion;
        }

        /// <summary>
        /// Invalida todos los tokens del usuario incrementando TokenVersion
        /// (Útil para logout forzado o cambio de contraseña)
        /// </summary>
        public async Task InvalidateAllTokensAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.TokenVersion++; // Incrementar versión invalida todos los tokens anteriores
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Obtiene un usuario por ID
        /// </summary>
        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        /// <summary>
        /// Obtiene un usuario por Email
        /// </summary>
        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }
    }
}