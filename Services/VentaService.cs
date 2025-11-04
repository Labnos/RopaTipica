using Microsoft.EntityFrameworkCore;
using InventarioRopaTipica.Data;
using InventarioRopaTipica.DTOs;
using InventarioRopaTipica.Models;

namespace InventarioRopaTipica.Services
{
    public class VentaService : IVentaService
    {
        private readonly ApplicationDbContext _context;

        public VentaService(ApplicationDbContext context)
        {
            _context = context;
        }

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
                return ApiResponse<VentaDto>.ErrorResponse($"Error al obtener venta: {ex.Message}");
            }
        }

        public async Task<ApiResponse<VentaDto>> CreateVentaAsync(CreateVentaDto createVentaDto, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Generar número de factura único
                var numeroFactura = $"FAC-{DateTime.Now:yyyyMMddHHmmss}";

                // Calcular totales
                decimal subtotal = 0;
                decimal descuentoTotal = 0;

                // Crear la venta
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

                // Procesar detalles de venta
                foreach (var detalleDto in createVentaDto.DetallesVentas)
                {
                    var producto = await _context.Productos.FindAsync(detalleDto.ProductoId);
                    if (producto == null)
                    {
                        await transaction.RollbackAsync();
                        return ApiResponse<VentaDto>.ErrorResponse($"Producto con ID {detalleDto.ProductoId} no encontrado");
                    }

                    // VALIDACIÓN CRÍTICA: Determinar tipo de venta para CORTES
                    string tipoVenta = "Completo";
                    decimal? varasVendidas = null;

                    if (producto.Tipo == "Corte")
                    {
                        // Para cortes, usar varas vendidas
                        varasVendidas = detalleDto.VarasVendidas ?? detalleDto.Cantidad;

                        // REGLA DE NEGOCIO: Si vende menos de 8 varas = Parcial
                        if (varasVendidas < 8.00m)
                        {
                            tipoVenta = "Parcial";
                        }

                        // Validar stock disponible
                        if (producto.VarasDisponibles < varasVendidas)
                        {
                            await transaction.RollbackAsync();
                            return ApiResponse<VentaDto>.ErrorResponse(
                                $"Stock insuficiente para {producto.Nombre}. Disponible: {producto.VarasDisponibles} varas");
                        }
                    }
                    else
                    {
                        // Para otros productos, validar stock normal
                        if (producto.Stock < detalleDto.Cantidad)
                        {
                            await transaction.RollbackAsync();
                            return ApiResponse<VentaDto>.ErrorResponse(
                                $"Stock insuficiente para {producto.Nombre}. Disponible: {producto.Stock} unidades");
                        }
                    }

                    // Crear detalle de venta
                    var detalleVenta = new DetalleVenta
                    {
                        VentaId = venta.Id,
                        ProductoId = detalleDto.ProductoId,
                        Cantidad = detalleDto.Cantidad,
                        VarasVendidas = varasVendidas,
                        PrecioUnitario = detalleDto.PrecioUnitario,
                        TipoVenta = tipoVenta, // CRÍTICO: Marcar si es Completo o Parcial
                        DescuentoAplicado = 0,
                        PromocionId = detalleDto.PromocionId
                    };

                    _context.DetalleVentas.Add(detalleVenta);

                    // Calcular subtotal del detalle
                    decimal subtotalDetalle = detalleDto.PrecioUnitario * detalleDto.Cantidad;
                    subtotal += subtotalDetalle;

                    // Nota: El trigger de MySQL se encargará de actualizar el stock y estado del corte
                }

                await _context.SaveChangesAsync();

                // Actualizar totales de la venta
                decimal iva = subtotal * 0.12m; // 12% IVA en Guatemala
                decimal total = subtotal + iva - descuentoTotal;

                venta.Subtotal = subtotal;
                venta.Descuento = descuentoTotal;
                venta.Iva = iva;
                venta.Total = total;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Retornar la venta creada
                var ventaCreada = await GetVentaByIdAsync(venta.Id);
                return ApiResponse<VentaDto>.SuccessResponse(ventaCreada.Data, "Venta creada exitosamente");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
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

                // Devolver stock
                foreach (var detalle in venta.DetallesVentas)
                {
                    var producto = await _context.Productos.FindAsync(detalle.ProductoId);
                    if (producto != null)
                    {
                        if (producto.Tipo == "Corte" && detalle.VarasVendidas.HasValue)
                        {
                            producto.VarasDisponibles += detalle.VarasVendidas.Value;
                            
                            // Actualizar estado del corte
                            if (producto.VarasDisponibles >= producto.VarasOriginales)
                                producto.EstadoCorte = "Completo";
                            else if (producto.VarasDisponibles > 0)
                                producto.EstadoCorte = "Parcial";
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

                return ApiResponse<bool>.SuccessResponse(true, "Venta cancelada exitosamente");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ApiResponse<bool>.ErrorResponse($"Error al cancelar venta: {ex.Message}");
            }
        }

        /// <summary>
        /// VALIDACIÓN CRÍTICA: Valida si un detalle de venta puede ser devuelto
        /// Regla: NO se permiten devoluciones de ventas PARCIALES
        /// </summary>
        public async Task<ApiResponse<bool>> ValidarDevolucionAsync(int detalleVentaId)
        {
            try
            {
                var detalle = await _context.DetalleVentas
                    .Include(dv => dv.Producto)
                    .FirstOrDefaultAsync(dv => dv.Id == detalleVentaId);

                if (detalle == null)
                    return ApiResponse<bool>.ErrorResponse("Detalle de venta no encontrado");

                // REGLA DE NEGOCIO: Si es venta PARCIAL, NO se permite devolución
                if (detalle.TipoVenta == "Parcial")
                {
                    return ApiResponse<bool>.ErrorResponse(
                        "No se permiten devoluciones de ventas parciales. " +
                        "Solo productos vendidos completos pueden ser devueltos.");
                }

                return ApiResponse<bool>.SuccessResponse(true, "El producto puede ser devuelto");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error al validar devolución: {ex.Message}");
            }
        }
    }
}