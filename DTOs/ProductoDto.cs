using System.ComponentModel.DataAnnotations;

namespace InventarioRopaTipica.DTOs
{
    public class ProductoDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Tipo { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Stock { get; set; }
        public decimal? VarasDisponibles { get; set; }
        public decimal? VarasOriginales { get; set; }
        public string EstadoCorte { get; set; }
        public DateTime? FechaCompra { get; set; }
        public int? ProveedorId { get; set; }
        public string ProveedorNombre { get; set; }
        public int? SucursalId { get; set; }
        public string SucursalNombre { get; set; }
        public DateTime CreadoEn { get; set; }
    }

    public class CreateProductoDto
    {
        [MaxLength(50)]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(150)]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El tipo es requerido")]
        public string Tipo { get; set; } // Corte, Huipil, Blusa, Faja, Perraje

        [Required(ErrorMessage = "El precio de compra es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal PrecioCompra { get; set; }

        [Required(ErrorMessage = "El precio de venta es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal PrecioVenta { get; set; }

        [Required(ErrorMessage = "El stock es requerido")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; }

        public decimal? VarasDisponibles { get; set; }
        public decimal? VarasOriginales { get; set; }
        public DateTime? FechaCompra { get; set; }
        public int? ProveedorId { get; set; }
        public int? SucursalId { get; set; }
    }
}