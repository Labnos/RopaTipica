using InventarioRopaTipica.DTOs;

namespace InventarioRopaTipica.Services
{
    public interface IProductoService
    {
        Task<ApiResponse<List<ProductoDto>>> GetAllProductosAsync();
        Task<ApiResponse<ProductoDto>> GetProductoByIdAsync(int id);
        Task<ApiResponse<List<ProductoDto>>> GetProductosByTipoAsync(string tipo);
        Task<ApiResponse<List<ProductoDto>>> GetProductosBySucursalAsync(int sucursalId);
        Task<ApiResponse<ProductoDto>> CreateProductoAsync(CreateProductoDto createProductoDto);
        Task<ApiResponse<ProductoDto>> UpdateProductoAsync(int id, CreateProductoDto updateProductoDto);
        Task<ApiResponse<bool>> DeleteProductoAsync(int id);
        Task<ApiResponse<List<ProductoDto>>> GetProductosBajoStockAsync();
    }
}