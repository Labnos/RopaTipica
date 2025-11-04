using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioRopaTipica.Models
{
    [Table("Promociones")]
    public class Promocion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("producto_id")]
        public int ProductoId { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("nombre")]
        public string Nombre { get; set; }

        [Column("descripcion")]
        public string Descripcion { get; set; }

        [Required]
        [Column("tipo_descuento")]
        public string TipoDescuento { get; set; } // Porcentaje, Monto

        [Required]
        [Column("valor_descuento", TypeName = "decimal(10,2)")]
        public decimal ValorDescuento { get; set; }

        [Required]
        [Column("fecha_inicio")]
        public DateTime FechaInicio { get; set; }

        [Required]
        [Column("fecha_fin")]
        public DateTime FechaFin { get; set; }

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Required]
        [Column("creado_por")]
        public int CreadoPor { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Navegación
        [ForeignKey("ProductoId")]
        public virtual Producto Producto { get; set; }

        [ForeignKey("CreadoPor")]
        public virtual User Usuario { get; set; }
    }
}