using System.ComponentModel.DataAnnotations;

namespace InventarioRopaTipica.DTOs
{
    public class VentaDto
    {
        public int Id { get; set; }
        public string NumeroFactura { get; set; }
        public int? ClienteId { get; set; }
        public string ClienteNombre { get; set; }
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; }
        public DateTime Fecha { get; set; }
        public string Canal { get; set; }
        public string EstadoPago { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
        public string MetodoPago { get; set; }
        public string EstadoVenta { get; set; }
        public string Observaciones { get; set; }
        public List<DetalleVentaDto> DetallesVentas { get; set; }
    }

    public class CreateVentaDto
    {
        public int? ClienteId { get; set; }

        [Required(ErrorMessage = "El canal es requerido")]
        public string Canal { get; set; } // Fisico, Facebook, WhatsApp

        [Required(ErrorMessage = "El método de pago es requerido")]
        public string MetodoPago { get; set; } // Efectivo, Tarjeta, Transferencia, Cheque

        [Required(ErrorMessage = "El estado de pago es requerido")]
        public string EstadoPago { get; set; } // Pendiente, Pagado

        public string Observaciones { get; set; }

        [Required(ErrorMessage = "Debe incluir al menos un producto")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un producto")]
        public List<CreateDetalleVentaDto> DetallesVentas { get; set; }
    }

    public class DetalleVentaDto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; }
        public int Cantidad { get; set; }
        public decimal? VarasVendidas { get; set; }
        public decimal PrecioUnitario { get; set; }
        public string TipoVenta { get; set; }
        public decimal DescuentoAplicado { get; set; }
        public int? PromocionId { get; set; }
    }

    public class CreateDetalleVentaDto
    {
        [Required(ErrorMessage = "El producto es requerido")]
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }

        public decimal? VarasVendidas { get; set; }

        [Required(ErrorMessage = "El precio unitario es requerido")]
        public decimal PrecioUnitario { get; set; }

        public int? PromocionId { get; set; }
    }
}