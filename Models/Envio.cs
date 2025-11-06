using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioRopaTipica.Models
{
    [Table("envios")]
    public class Envio
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("venta_id")]
        public int VentaId { get; set; }

        [MaxLength(255)]
        [Column("direccion_envio")]
        public string DireccionEnvio { get; set; }

        [MaxLength(100)]
        [Column("departamento")]
        public string Departamento { get; set; }

        [MaxLength(100)]
        [Column("municipio")]
        public string Municipio { get; set; }

        [MaxLength(20)]
        [Column("telefono_contacto")]
        public string TelefonoContacto { get; set; }

        [MaxLength(100)]
        [Column("servicio_envio")]
        public string ServicioEnvio { get; set; }

        [MaxLength(50)]
        [Column("numero_guia")]
        public string NumeroGuia { get; set; }

        [Required]
        [Column("estado_envio")]
        public string EstadoEnvio { get; set; } // Pendiente, Enviado, Entregado

        [Column("fecha_envio")]
        public DateTime? FechaEnvio { get; set; }

        [Column("fecha_entrega_estimada")]
        public DateTime? FechaEntregaEstimada { get; set; }

        [Column("fecha_entrega_real")]
        public DateTime? FechaEntregaReal { get; set; }

        [Column("costo_envio", TypeName = "decimal(10,2)")]
        public decimal CostoEnvio { get; set; } = 0;

        [Column("observaciones")]
        public string Observaciones { get; set; }

        // Navegación
        [ForeignKey("VentaId")]
        public virtual Venta Venta { get; set; }
    }
}