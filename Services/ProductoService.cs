using Microsoft.EntityFrameworkCore;
using InventarioRopaTipica.Data;
using InventarioRopaTipica.DTOs;
using InventarioRopaTipica.Models;

namespace InventarioRopaTipica.Services
{
    public class ProductoService : IProductoService
    {
        private readonly ApplicationDbContext _context;

        public ProductoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<ProductoDto>>> GetAllProductosAsync()
        {
            try
            {
                var productos = await _context.Productos
                    .Include(p => p.Proveedor)
                    .Include(p => p.Sucursal)
                    .Select(p => new ProductoDto
                    {
                        Id = p.Id,
                        Codigo = p.Codigo,
                        Nombre = p.Nombre,
                        Descripcion = p.Descripcion,
                        Tipo = p.Tipo,
                        PrecioCompra = p.PrecioCompra,
                        PrecioVenta = p.PrecioVenta,
                        Stock = p.Stock,
                        VarasDisponibles = p.VarasDisponibles,
                        VarasOriginales = p.VarasOriginales,
                        EstadoCorte = p.EstadoCorte,
                        FechaCompra = p.FechaCompra,
                        ProveedorId = p.ProveedorId,
                        ProveedorNombre = p.Proveedor != null ? p.Proveedor.Nombre : null,
                        SucursalId = p.SucursalId,
                        SucursalNombre = p.Sucursal != null ? p.Sucursal.Nombre : null,
                        CreadoEn = p.CreadoEn
                    })
                    .OrderByDescending(p => p.CreadoEn)
                    .ToListAsync();

                return ApiResponse<List<ProductoDto>>.SuccessResponse(productos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ProductoDto>>.ErrorResponse($"Error al obtener productos: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ProductoDto>> GetProductoByIdAsync(int id)
        {
            try
            {
                var producto = await _context.Productos
                    .Include(p => p.Proveedor)
                    .Include(p => p.Sucursal)
                    .Where(p => p.Id == id)
                    .Select(p => new ProductoDto
                    {
                        Id = p.Id,
                        Codigo = p.Codigo,
                        Nombre = p.Nombre,
                        Descripcion = p.Descripcion,
                        Tipo = p.Tipo,
                        PrecioCompra = p.PrecioCompra,
                        PrecioVenta = p.PrecioVenta,
                        Stock = p.Stock,
                        VarasDisponibles = p.VarasDisponibles,
                        VarasOriginales = p.VarasOriginales,
                        EstadoCorte = p.EstadoCorte,
                        FechaCompra = p.FechaCompra,
                        ProveedorId = p.ProveedorId,
                        ProveedorNombre = p.Proveedor != null ? p.Proveedor.Nombre : null,
                        SucursalId = p.SucursalId,
                        SucursalNombre = p.Sucursal != null ? p.Sucursal.Nombre : null,
                        CreadoEn = p.CreadoEn
                    })
                    .FirstOrDefaultAsync();

                if (producto == null)
                    return ApiResponse<ProductoDto>.ErrorResponse("Producto no encontrado");

                return ApiResponse<ProductoDto>.SuccessResponse(producto);
            }
            catch (Exception ex)
            {
                return ApiResponse<ProductoDto>.ErrorResponse($"Error al obtener producto: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ProductoDto>>> GetProductosByTipoAsync(string tipo)
        {
            try
            {
                var productos = await _context.Productos
                    .Include(p => p.Proveedor)
                    .Include(p => p.Sucursal)
                    .Where(p => p.Tipo == tipo)
                    .Select(p => new ProductoDto
                    {
                        Id = p.Id,
                        Codigo = p.Codigo,
                        Nombre = p.Nombre,
                        Descripcion = p.Descripcion,
                        Tipo = p.Tipo,
                        PrecioCompra = p.PrecioCompra,
                        PrecioVenta = p.PrecioVenta,
                        Stock = p.Stock,
                        VarasDisponibles = p.VarasDisponibles,
                        VarasOriginales = p.VarasOriginales,
                        EstadoCorte = p.EstadoCorte,
                        FechaCompra = p.FechaCompra,
                        ProveedorId = p.ProveedorId,
                        ProveedorNombre = p.Proveedor != null ? p.Proveedor.Nombre : null,
                        SucursalId = p.SucursalId,
                        SucursalNombre = p.Sucursal != null ? p.Sucursal.Nombre : null,
                        CreadoEn = p.CreadoEn
                    })
                    .ToListAsync();

                return ApiResponse<List<ProductoDto>>.SuccessResponse(productos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ProductoDto>>.ErrorResponse($"Error al obtener productos: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ProductoDto>>> GetProductosBySucursalAsync(int sucursalId)
        {
            try
            {
                var productos = await _context.Productos
                    .Include(p => p.Proveedor)
                    .Include(p => p.Sucursal)
                    .Where(p => p.SucursalId == sucursalId)
                    .Select(p => new ProductoDto
                    {
                        Id = p.Id,
                        Codigo = p.Codigo,
                        Nombre = p.Nombre,
                        Descripcion = p.Descripcion,
                        Tipo = p.Tipo,
                        PrecioCompra = p.PrecioCompra,
                        PrecioVenta = p.PrecioVenta,
                        Stock = p.Stock,
                        VarasDisponibles = p.VarasDisponibles,
                        VarasOriginales = p.VarasOriginales,
                        EstadoCorte = p.EstadoCorte,
                        FechaCompra = p.FechaCompra,
                        ProveedorId = p.ProveedorId,
                        ProveedorNombre = p.Proveedor != null ? p.Proveedor.Nombre : null,
                        SucursalId = p.SucursalId,
                        SucursalNombre = p.Sucursal != null ? p.Sucursal.Nombre : null,
                        CreadoEn = p.CreadoEn
                    })
                    .ToListAsync();

                return ApiResponse<List<ProductoDto>>.SuccessResponse(productos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ProductoDto>>.ErrorResponse($"Error al obtener productos: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ProductoDto>> CreateProductoAsync(CreateProductoDto createProductoDto)
        {
            try
            {
                // Validar que el código no exista
                if (!string.IsNullOrEmpty(createProductoDto.Codigo))
                {
                    if (await _context.Productos.AnyAsync(p => p.Codigo == createProductoDto.Codigo))
                        return ApiResponse<ProductoDto>.ErrorResponse("El código del producto ya existe");
                }
                else
                {
                    // Generar código automático
                    createProductoDto.Codigo = $"PROD-{DateTime.Now:yyyyMMddHHmmss}";
                }

                var producto = new Producto
                {
                    Codigo = createProductoDto.Codigo,
                    Nombre = createProductoDto.Nombre,
                    Descripcion = createProductoDto.Descripcion,
                    Tipo = createProductoDto.Tipo,
                    PrecioCompra = createProductoDto.PrecioCompra,
                    PrecioVenta = createProductoDto.PrecioVenta,
                    Stock = createProductoDto.Stock,
                    VarasDisponibles = createProductoDto.VarasDisponibles ?? 8.00m,
                    VarasOriginales = createProductoDto.VarasOriginales ?? 8.00m,
                    EstadoCorte = "Completo",
                    FechaCompra = createProductoDto.FechaCompra ?? DateTime.Now,
                    ProveedorId = createProductoDto.ProveedorId,
                    SucursalId = createProductoDto.SucursalId,
                    CreadoEn = DateTime.Now
                };

                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();

                var productoDto = await GetProductoByIdAsync(producto.Id);
                return ApiResponse<ProductoDto>.SuccessResponse(productoDto.Data, "Producto creado exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<ProductoDto>.ErrorResponse($"Error al crear producto: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ProductoDto>> UpdateProductoAsync(int id, CreateProductoDto updateProductoDto)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null)
                    return ApiResponse<ProductoDto>.ErrorResponse("Producto no encontrado");

                // Validar código único
                if (!string.IsNullOrEmpty(updateProductoDto.Codigo) && updateProductoDto.Codigo != producto.Codigo)
                {
                    if (await _context.Productos.AnyAsync(p => p.Codigo == updateProductoDto.Codigo && p.Id != id))
                        return ApiResponse<ProductoDto>.ErrorResponse("El código del producto ya existe");
                    producto.Codigo = updateProductoDto.Codigo;
                }

                producto.Nombre = updateProductoDto.Nombre;
                producto.Descripcion = updateProductoDto.Descripcion;
                producto.Tipo = updateProductoDto.Tipo;
                producto.PrecioCompra = updateProductoDto.PrecioCompra;
                producto.PrecioVenta = updateProductoDto.PrecioVenta;
                producto.Stock = updateProductoDto.Stock;
                producto.ProveedorId = updateProductoDto.ProveedorId;
                producto.SucursalId = updateProductoDto.SucursalId;

                if (updateProductoDto.FechaCompra.HasValue)
                    producto.FechaCompra = updateProductoDto.FechaCompra;

                await _context.SaveChangesAsync();

                var productoDto = await GetProductoByIdAsync(producto.Id);
                return ApiResponse<ProductoDto>.SuccessResponse(productoDto.Data, "Producto actualizado exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<ProductoDto>.ErrorResponse($"Error al actualizar producto: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteProductoAsync(int id)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null)
                    return ApiResponse<bool>.ErrorResponse("Producto no encontrado");

                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Producto eliminado exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error al eliminar producto: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ProductoDto>>> GetProductosBajoStockAsync()
        {
            try
            {
                var productos = await _context.Productos
                    .Include(p => p.Proveedor)
                    .Include(p => p.Sucursal)
                    .Where(p => 
                        (p.Tipo != "Corte" && p.Stock < 5) || 
                        (p.Tipo == "Corte" && p.VarasDisponibles < 3))
                    .Select(p => new ProductoDto
                    {
                        Id = p.Id,
                        Codigo = p.Codigo,
                        Nombre = p.Nombre,
                        Tipo = p.Tipo,
                        Stock = p.Stock,
                        VarasDisponibles = p.VarasDisponibles,
                        EstadoCorte = p.EstadoCorte,
                        ProveedorNombre = p.Proveedor != null ? p.Proveedor.Nombre : null,
                        SucursalNombre = p.Sucursal != null ? p.Sucursal.Nombre : null
                    })
                    .ToListAsync();

                return ApiResponse<List<ProductoDto>>.SuccessResponse(productos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ProductoDto>>.ErrorResponse($"Error al obtener productos bajo stock: {ex.Message}");
            }
        }
    }
}