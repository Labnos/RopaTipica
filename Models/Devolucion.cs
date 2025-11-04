using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioRopaTipica.Models
{
    [Table("Devoluciones")]
    public class Devolucion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("venta_id")]
        public int VentaId { get; set; }

        [Required]
        [Column("detalle_venta_id")]
        public int DetalleVentaId { get; set; }

        [Required]
        [Column("producto_id")]
        public int ProductoId { get; set; }

        [Required]
        [Column("cantidad_devuelta")]
        public int CantidadDevuelta { get; set; }

        [Column("varas_devueltas", TypeName = "decimal(10,2)")]
        public decimal VarasDevueltas { get; set; } = 0;

        [Required]
        [Column("monto_devuelto", TypeName = "decimal(12,2)")]
        public decimal MontoDevuelto { get; set; }

        [Required]
        [Column("motivo_devolucion")]
        public string MotivoDevolucion { get; set; }

        [Column("fecha_devolucion")]
        public DateTime FechaDevolucion { get; set; } = DateTime.Now;

        [Required]
        [Column("autorizado_por")]
        public int AutorizadoPor { get; set; }

        // Navegación
        [ForeignKey("VentaId")]
        public virtual Venta Venta { get; set; }

        [ForeignKey("DetalleVentaId")]
        public virtual DetalleVenta DetalleVenta { get; set; }

        [ForeignKey("ProductoId")]
        public virtual Producto Producto { get; set; }

        [ForeignKey("AutorizadoPor")]
        public virtual User UsuarioAutorizador { get; set; }
    }
}