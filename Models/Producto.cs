using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioRopaTipica.Models
{
    [Table("productos")]
    public class Producto
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [MaxLength(50)]
        [Column("codigo")]
        public string Codigo { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("nombre")]
        public string Nombre { get; set; }

        [Column("descripcion")]
        public string Descripcion { get; set; }

        [Required]
        [Column("tipo")]
        public string Tipo { get; set; } // Corte, Huipil, Blusa, Faja, Perraje

        [Required]
        [Column("precio_compra", TypeName = "decimal(10,2)")]
        public decimal PrecioCompra { get; set; }

        [Required]
        [Column("precio_venta", TypeName = "decimal(10,2)")]
        public decimal PrecioVenta { get; set; }

        [Required]
        [Column("stock")]
        public int Stock { get; set; }

        [Column("varas_disponibles", TypeName = "decimal(10,2)")]
        public decimal? VarasDisponibles { get; set; } = 8.00m;

        [Column("varas_originales", TypeName = "decimal(10,2)")]
        public decimal? VarasOriginales { get; set; } = 8.00m;

        [Column("estado_corte")]
        public string EstadoCorte { get; set; } = "Completo"; // Completo, Parcial, Agotado

        [Column("fecha_compra")]
        public DateTime? FechaCompra { get; set; }

        [Column("proveedor_id")]
        public int? ProveedorId { get; set; }

        [Column("sucursal_id")]
        public int? SucursalId { get; set; }

        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.Now;

        // Navegación
        [ForeignKey("ProveedorId")]
        public virtual Proveedor Proveedor { get; set; }

        [ForeignKey("SucursalId")]
        public virtual Sucursal Sucursal { get; set; }

        public virtual ICollection<DetalleVenta> DetallesVentas { get; set; }
        public virtual ICollection<Promocion> Promociones { get; set; }
    }
}