using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioRopaTipica.Models
{
    [Table("detalleventas")]
    public class DetalleVenta
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("venta_id")]
        public int VentaId { get; set; }

        [Required]
        [Column("producto_id")]
        public int ProductoId { get; set; }

        [Required]
        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Column("varas_vendidas", TypeName = "decimal(10,2)")]
        public decimal? VarasVendidas { get; set; }

        [Required]
        [Column("precio_unitario", TypeName = "decimal(10,2)")]
        public decimal PrecioUnitario { get; set; }

        [Required]
        [Column("tipo_venta")]
        public string TipoVenta { get; set; } = "Completo"; // Completo, Parcial

        [Column("descuento_aplicado", TypeName = "decimal(10,2)")]
        public decimal DescuentoAplicado { get; set; } = 0;

        [Column("promocion_id")]
        public int? PromocionId { get; set; }

        // Navegación
        [ForeignKey("VentaId")]
        public virtual Venta Venta { get; set; }

        [ForeignKey("ProductoId")]
        public virtual Producto Producto { get; set; }

        [ForeignKey("PromocionId")]
        public virtual Promocion Promocion { get; set; }
    }
}