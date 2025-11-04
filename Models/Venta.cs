using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioRopaTipica.Models
{
    [Table("Ventas")]
    public class Venta
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [MaxLength(50)]
        [Column("numero_factura")]
        public string NumeroFactura { get; set; }

        [Column("cliente_id")]
        public int? ClienteId { get; set; }

        [Required]
        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        [Column("fecha")]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        [Column("canal")]
        public string Canal { get; set; } // Fisico, Facebook, WhatsApp

        [Required]
        [Column("estado_pago")]
        public string EstadoPago { get; set; } // Pendiente, Pagado

        [Column("subtotal", TypeName = "decimal(12,2)")]
        public decimal Subtotal { get; set; }

        [Column("descuento", TypeName = "decimal(10,2)")]
        public decimal Descuento { get; set; } = 0;

        [Column("iva", TypeName = "decimal(10,2)")]
        public decimal Iva { get; set; } = 0;

        [Column("total", TypeName = "decimal(12,2)")]
        public decimal Total { get; set; }

        [Column("metodo_pago")]
        public string MetodoPago { get; set; } // Efectivo, Tarjeta, Transferencia, Cheque

        [Column("estado_venta")]
        public string EstadoVenta { get; set; } = "Completada"; // Completada, Cancelada, Devuelta

        [Column("observaciones")]
        public string Observaciones { get; set; }

        // Navegación
        [ForeignKey("ClienteId")]
        public virtual Cliente Cliente { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual User Usuario { get; set; }

        public virtual ICollection<DetalleVenta> DetallesVentas { get; set; }
        public virtual ICollection<Envio> Envios { get; set; }
        public virtual ICollection<Devolucion> Devoluciones { get; set; }
    }
}