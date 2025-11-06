using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioRopaTipica.Models
{
    [Table("clientes")]
    public class Cliente
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("nombre")]
        public string Nombre { get; set; }

        [MaxLength(20)]
        [Column("telefono")]
        public string Telefono { get; set; }

        [MaxLength(255)]
        [Column("direccion")]
        public string Direccion { get; set; }

        [MaxLength(150)]
        [Column("email")]
        public string Email { get; set; }

        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.Now;

        // Navegación
        public virtual ICollection<Venta> Ventas { get; set; }
    }
}
