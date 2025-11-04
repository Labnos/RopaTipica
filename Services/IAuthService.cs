using InventarioRopaTipica.DTOs;
using InventarioRopaTipica.Models;

namespace InventarioRopaTipica.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto loginDto);
        Task<bool> ValidateTokenVersionAsync(int userId, int tokenVersion);
        Task InvalidateAllTokensAsync(int userId);
        Task<User> GetUserByIdAsync(int userId);
        Task<User> GetUserByEmailAsync(string email);
    }
}
