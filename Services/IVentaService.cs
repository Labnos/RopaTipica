using InventarioRopaTipica.DTOs;

namespace InventarioRopaTipica.Services
{
    public interface IVentaService
    {
        // CRUD principal de ventas
        Task<ApiResponse<List<VentaDto>>> GetAllVentasAsync();
        Task<ApiResponse<VentaDto>> GetVentaByIdAsync(int id);
        Task<ApiResponse<VentaDto>> CreateVentaAsync(CreateVentaDto createVentaDto, int userId);
        Task<ApiResponse<bool>> CancelarVentaAsync(int id, int userId);
        Task<ApiResponse<bool>> ValidarDevolucionAsync(int detalleVentaId);

        // Métodos adicionales para dashboard y estadísticas
        Task<ApiResponse<List<TopProductoDto>>> GetTopProductosAsync(int topN = 5);
        Task<ApiResponse<List<object>>> GetVentasMensualesAsync(int year);
        Task<ApiResponse<object>> GetResumenVentasAsync();
        Task<ApiResponse<DashboardResumenDto>> GetDashboardResumenAsync();
    }

    // DTO adicional para reportes y dashboard
    public class TopProductoDto
    {
        public string Producto { get; set; }
        public int CantidadVendida { get; set; }
    }
}
