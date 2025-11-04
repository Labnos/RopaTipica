using InventarioRopaTipica.DTOs;

namespace InventarioRopaTipica.Services
{
    public interface IClienteService
    {
        Task<ApiResponse<List<ClienteDto>>> GetAllClientesAsync();
        Task<ApiResponse<ClienteDto>> GetClienteByIdAsync(int id);
        Task<ApiResponse<ClienteDto>> CreateClienteAsync(CreateClienteDto createClienteDto);
        Task<ApiResponse<ClienteDto>> UpdateClienteAsync(int id, CreateClienteDto updateClienteDto);
        Task<ApiResponse<bool>> DeleteClienteAsync(int id);
    }
}