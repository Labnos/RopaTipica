using InventarioRopaTipica.DTOs;

namespace InventarioRopaTipica.Services
{
    public interface IVentaService
    {
        Task<ApiResponse<List<VentaDto>>> GetAllVentasAsync();
        Task<ApiResponse<VentaDto>> GetVentaByIdAsync(int id);
        Task<ApiResponse<VentaDto>> CreateVentaAsync(CreateVentaDto createVentaDto, int userId);
        Task<ApiResponse<bool>> CancelarVentaAsync(int id, int userId);
        Task<ApiResponse<bool>> ValidarDevolucionAsync(int detalleVentaId);
    }
}
