using InventarioRopaTipica.DTOs;

namespace InventarioRopaTipica.Services
{
    public interface IProveedorService
    {
        Task<ApiResponse<List<ProveedorDto>>> GetAllProveedoresAsync();
        Task<ApiResponse<ProveedorDto>> GetProveedorByIdAsync(int id);
        Task<ApiResponse<ProveedorDto>> CreateProveedorAsync(CreateProveedorDto createProveedorDto);
        Task<ApiResponse<ProveedorDto>> UpdateProveedorAsync(int id, CreateProveedorDto updateProveedorDto);
        Task<ApiResponse<bool>> DeleteProveedorAsync(int id);
    }
}