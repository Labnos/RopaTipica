using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InventarioRopaTipica.Data;
using InventarioRopaTipica.DTOs;
using InventarioRopaTipica.Models;

namespace InventarioRopaTipica.Services
{
    public class VentaService : IVentaService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VentaService> _logger;

        public VentaService(ApplicationDbContext context, ILogger<VentaService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =============================
        // MÉTODOS PRINCIPALES DE VENTAS
        // =============================

        public async Task<ApiResponse<List<VentaDto>>> GetAllVentasAsync()
        {
            try
            {
                var ventas = await _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.Usuario)
                    .Include(v => v.DetallesVentas)
                        .ThenInclude(dv => dv.Producto)
                    .OrderByDescending(v => v.Fecha)
                    .Select(v => new VentaDto
                    {
                        Id = v.Id,
                        NumeroFactura = v.NumeroFactura,
                        ClienteId = v.ClienteId,
                        ClienteNombre = v.Cliente != null ? v.Cliente.Nombre : "Cliente General",
                        UsuarioId = v.UsuarioId,
                        UsuarioNombre = v.Usuario.Nombre,
                        Fecha = v.Fecha,
                        Canal = v.Canal,
                        EstadoPago = v.EstadoPago,
                        Subtotal = v.Subtotal,
                        Descuento = v.Descuento,
                        Iva = v.Iva,
                        Total = v.Total,
                        MetodoPago = v.MetodoPago,
                        EstadoVenta = v.EstadoVenta,
                        Observaciones = v.Observaciones,
                        DetallesVentas = v.DetallesVentas.Select(dv => new DetalleVentaDto
                        {
                            Id = dv.Id,
                            ProductoId = dv.ProductoId,
                            ProductoNombre = dv.Producto.Nombre,
                            Cantidad = dv.Cantidad,
                            VarasVendidas = dv.VarasVendidas,
                            PrecioUnitario = dv.PrecioUnitario,
                            TipoVenta = dv.TipoVenta,
                            DescuentoAplicado = dv.DescuentoAplicado,
                            PromocionId = dv.PromocionId
                        }).ToList()
                    })
                    .ToListAsync();

                return ApiResponse<List<VentaDto>>.SuccessResponse(ventas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las ventas");
                return ApiResponse<List<VentaDto>>.ErrorResponse($"Error al obtener ventas: {ex.Message}");
            }
        }

        public async Task<ApiResponse<VentaDto>> GetVentaByIdAsync(int id)
        {
            try
            {
                var venta = await _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.Usuario)
                    .Include(v => v.DetallesVentas)
                        .ThenInclude(dv => dv.Producto)
                    .Where(v => v.Id == id)
                    .Select(v => new VentaDto
                    {
                        Id = v.Id,
                        NumeroFactura = v.NumeroFactura,
                        ClienteId = v.ClienteId,
                        ClienteNombre = v.Cliente != null ? v.Cliente.Nombre : "Cliente General",
                        UsuarioId = v.UsuarioId,
                        UsuarioNombre = v.Usuario.Nombre,
                        Fecha = v.Fecha,
                        Canal = v.Canal,
                        EstadoPago = v.EstadoPago,
                        Subtotal = v.Subtotal,
                        Descuento = v.Descuento,
                        Iva = v.Iva,
                        Total = v.Total,
                        MetodoPago = v.MetodoPago,
                        EstadoVenta = v.EstadoVenta,
                        Observaciones = v.Observaciones,
                        DetallesVentas = v.DetallesVentas.Select(dv => new DetalleVentaDto
                        {
                            Id = dv.Id,
                            ProductoId = dv.ProductoId,
                            ProductoNombre = dv.Producto.Nombre,
                            Cantidad = dv.Cantidad,
                            VarasVendidas = dv.VarasVendidas,
                            PrecioUnitario = dv.PrecioUnitario,
                            TipoVenta = dv.TipoVenta,
                            DescuentoAplicado = dv.DescuentoAplicado,
                            PromocionId = dv.PromocionId
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (venta == null)
                    return ApiResponse<VentaDto>.ErrorResponse("Venta no encontrada");

                return ApiResponse<VentaDto>.SuccessResponse(venta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener venta {VentaId}", id);
                return ApiResponse<VentaDto>.ErrorResponse($"Error al obtener venta: {ex.Message}");
            }
        }

        public async Task<ApiResponse<VentaDto>> CreateVentaAsync(CreateVentaDto createVentaDto, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var numeroFactura = $"FAC-{DateTime.Now:yyyyMMddHHmmss}";
                decimal subtotal = 0;
                decimal descuentoTotal = 0;

                var venta = new Venta
                {
                    NumeroFactura = numeroFactura,
                    ClienteId = createVentaDto.ClienteId,
                    UsuarioId = userId,
                    Fecha = DateTime.Now,
                    Canal = createVentaDto.Canal,
                    EstadoPago = createVentaDto.EstadoPago,
                    MetodoPago = createVentaDto.MetodoPago,
                    EstadoVenta = "Completada",
                    Observaciones = createVentaDto.Observaciones
                };

                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();

                foreach (var detalleDto in createVentaDto.DetallesVentas)
                {
                    var producto = await _context.Productos.FindAsync(detalleDto.ProductoId);
                    if (producto == null)
                    {
                        await transaction.RollbackAsync();
                        return ApiResponse<VentaDto>.ErrorResponse($"Producto con ID {detalleDto.ProductoId} no encontrado");
                    }

                    var tipoVenta = "Completo";
                    decimal? varasVendidas = null;

                    if (producto.Tipo == "Corte")
                    {
                        varasVendidas = detalleDto.VarasVendidas ?? detalleDto.Cantidad;

                        if (varasVendidas < 8.00m)
                            tipoVenta = "Parcial";

                        if (producto.VarasDisponibles < varasVendidas)
                        {
                            await transaction.RollbackAsync();
                            return ApiResponse<VentaDto>.ErrorResponse(
                                $"Stock insuficiente para {producto.Nombre}. Disponible: {producto.VarasDisponibles} varas");
                        }

                        producto.VarasDisponibles -= varasVendidas.Value;
                    }
                    else
                    {
                        if (producto.Stock < detalleDto.Cantidad)
                        {
                            await transaction.RollbackAsync();
                            return ApiResponse<VentaDto>.ErrorResponse(
                                $"Stock insuficiente para {producto.Nombre}. Disponible: {producto.Stock} unidades");
                        }

                        producto.Stock -= detalleDto.Cantidad;
                    }

                    var detalleVenta = new DetalleVenta
                    {
                        VentaId = venta.Id,
                        ProductoId = detalleDto.ProductoId,
                        Cantidad = detalleDto.Cantidad,
                        VarasVendidas = varasVendidas,
                        PrecioUnitario = detalleDto.PrecioUnitario,
                        TipoVenta = tipoVenta,
                        DescuentoAplicado = 0,
                        PromocionId = detalleDto.PromocionId
                    };

                    _context.DetalleVentas.Add(detalleVenta);

                    subtotal += detalleDto.PrecioUnitario * detalleDto.Cantidad;
                }

                decimal iva = subtotal * 0.12m;
                decimal total = subtotal + iva - descuentoTotal;

                venta.Subtotal = subtotal;
                venta.Descuento = descuentoTotal;
                venta.Iva = iva;
                venta.Total = total;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var ventaCreada = await GetVentaByIdAsync(venta.Id);
                _logger.LogInformation("Venta creada exitosamente. ID: {VentaId}", venta.Id);

                return ApiResponse<VentaDto>.SuccessResponse(ventaCreada.Data, "Venta creada exitosamente");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al crear venta por usuario {UserId}", userId);
                return ApiResponse<VentaDto>.ErrorResponse($"Error al crear venta: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> CancelarVentaAsync(int id, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var venta = await _context.Ventas
                    .Include(v => v.DetallesVentas)
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (venta == null)
                    return ApiResponse<bool>.ErrorResponse("Venta no encontrada");

                if (venta.EstadoVenta == "Cancelada")
                    return ApiResponse<bool>.ErrorResponse("La venta ya está cancelada");

                foreach (var detalle in venta.DetallesVentas)
                {
                    var producto = await _context.Productos.FindAsync(detalle.ProductoId);
                    if (producto != null)
                    {
                        if (producto.Tipo == "Corte" && detalle.VarasVendidas.HasValue)
                        {
                            producto.VarasDisponibles += detalle.VarasVendidas.Value;
                            producto.EstadoCorte = producto.VarasDisponibles >= producto.VarasOriginales ? "Completo" : "Parcial";
                        }
                        else
                        {
                            producto.Stock += detalle.Cantidad;
                        }
                    }
                }

                venta.EstadoVenta = "Cancelada";
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Venta {VentaId} cancelada exitosamente por usuario {UserId}", id, userId);
                return ApiResponse<bool>.SuccessResponse(true, "Venta cancelada exitosamente");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al cancelar venta {VentaId}", id);
                return ApiResponse<bool>.ErrorResponse($"Error al cancelar venta: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> ValidarDevolucionAsync(int detalleVentaId)
        {
            try
            {
                var detalle = await _context.DetalleVentas
                    .Include(dv => dv.Producto)
                    .FirstOrDefaultAsync(dv => dv.Id == detalleVentaId);

                if (detalle == null)
                    return ApiResponse<bool>.ErrorResponse("Detalle de venta no encontrado");

                if (detalle.TipoVenta == "Parcial")
                    return ApiResponse<bool>.ErrorResponse("No se permiten devoluciones de ventas parciales.");

                return ApiResponse<bool>.SuccessResponse(true, "El producto puede ser devuelto");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar devolución {DetalleId}", detalleVentaId);
                return ApiResponse<bool>.ErrorResponse($"Error al validar devolución: {ex.Message}");
            }
        }

        // ===========================
        // MÉTODOS ADICIONALES DASHBOARD
        // ===========================

       public async Task<ApiResponse<List<TopProductoDto>>> GetTopProductosAsync(int topN)
        {
            try
            {
                var topProductos = await _context.DetalleVentas
                    
                    // 1. FILTRO (Arregla el error 400 de NULL)
                    .Where(dv => dv.Producto != null && dv.Producto.Nombre != null) 
                    
                    // 2. GROUP BY (Agrupamos por ID y Nombre)
                    .GroupBy(dv => new { 
                        dv.Producto.Id, 
                        dv.Producto.Nombre 
                    })
                    
                    // 3. SELECT (Mapeamos a TU DTO)
                    .Select(g => new TopProductoDto 
                    {
                        ProductoId = g.Key.Id,
                        ProductoNombre = g.Key.Nombre,
                        CantidadVendida = g.Sum(dv => dv.Cantidad),
                        TotalGenerado = g.Sum(dv => dv.Cantidad * dv.PrecioUnitario) 
                    })
                    
                    .OrderByDescending(x => x.CantidadVendida)
                    .Take(topN)
                    .ToListAsync();

                // 4. ✅ CORRECCIÓN DE CONSTRUCTOR (Arregla error CS1729)
                // Usamos tu método estático SuccessResponse
                return ApiResponse<List<TopProductoDto>>.SuccessResponse(topProductos, "Top productos obtenidos");
            }
            catch (Exception ex)
            {
                // 5. ✅ CORRECCIÓN DE CONSTRUCTOR (Arregla error CS1729)
                // Usamos tu método estático ErrorResponse
                return ApiResponse<List<TopProductoDto>>.ErrorResponse($"Error al obtener top productos: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<object>>> GetVentasMensualesAsync(int year)
        {
            try // ADDED: try block
            {
                var ventas = await _context.Ventas
                    .Where(v => v.Fecha.Year == year && v.EstadoVenta != "Cancelada")
                    .GroupBy(v => v.Fecha.Month)
                    .Select(g => new
                    {
                        Mes = g.Key,
                        Total = g.Sum(x => x.Total)
                    })
                    .OrderBy(x => x.Mes)
                    .ToListAsync();

                return ApiResponse<List<object>>.SuccessResponse(ventas.Cast<object>().ToList());
            }
            catch (Exception ex) // ADDED: catch block to handle exceptions gracefully
            {
                _logger.LogError(ex, "Error al obtener ventas mensuales para el año {Year}", year);
                return ApiResponse<List<object>>.ErrorResponse($"Error al obtener ventas mensuales: {ex.Message}");
            }
        }

        public async Task<ApiResponse<object>> GetResumenVentasAsync()
        {
            var totalVentas = await _context.Ventas.CountAsync(v => v.EstadoVenta != "Cancelada");
            var totalCanceladas = await _context.Ventas.CountAsync(v => v.EstadoVenta == "Cancelada");
            var totalIngresos = await _context.Ventas
                .Where(v => v.EstadoVenta != "Cancelada")
                .SumAsync(v => v.Total);

            var ventasMesActual = await _context.Ventas
                .Where(v => v.Fecha.Month == DateTime.Now.Month && v.Fecha.Year == DateTime.Now.Year)
                .SumAsync(v => v.Total);

            return ApiResponse<object>.SuccessResponse(new
            {
                totalVentas,
                totalCanceladas,
                totalIngresos,
                ventasMesActual
            });
        }

       public async Task<ApiResponse<DashboardResumenDto>> GetDashboardResumenAsync()
        {
            try
            {
                var hoy = DateTime.Today;
                // FIX: Use a time range check (>= start of day AND < next day)
                var startOfDay = hoy.Date;
                var endOfDay = hoy.Date.AddDays(1);

                var ingresosHoy = await _context.Ventas
                    .Where(v => v.Fecha >= startOfDay && v.Fecha < endOfDay && v.EstadoVenta == "Completada")
                    .SumAsync(v => (decimal?)v.Total) ?? 0;

                var ventasHoy = await _context.Ventas
                    .CountAsync(v => v.Fecha >= startOfDay && v.Fecha < endOfDay && v.EstadoVenta == "Completada");

                var ventasPendientes = await _context.Ventas
                    .CountAsync(v => v.EstadoPago == "Pendiente");

                var productosBajoStock = await _context.Productos
                    .CountAsync(p => p.Stock <= 5 || p.VarasDisponibles <= 5);

                var dto = new DashboardResumenDto
                {
                    IngresosHoy = ingresosHoy,
                    VentasHoy = ventasHoy,
                    VentasPendientes = ventasPendientes,
                    ProductosBajoStock = productosBajoStock
                };

                return ApiResponse<DashboardResumenDto>.SuccessResponse(dto);
            }
            catch (Exception ex)
            {
                return ApiResponse<DashboardResumenDto>.ErrorResponse($"Error al obtener resumen: {ex.Message}");
            }
        }
    }

}
