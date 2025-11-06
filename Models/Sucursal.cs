using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioRopaTipica.Models
{
    [Table("sucursales")]
    public class Sucursal
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; }

        [MaxLength(255)]
        [Column("direccion")]
        public string Direccion { get; set; }

        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.Now;

        // Navegación
        public virtual ICollection<Producto> Productos { get; set; }
    }
}