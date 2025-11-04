using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioRopaTipica.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("email")]
        public string Email { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("password_hash")]
        public string PasswordHash { get; set; }

        [Required]
        [Column("rol")]
        public string Rol { get; set; } // Administrador, Vendedor, Encargado

        [Column("estado")]
        public bool Estado { get; set; } = true;

        [Column("token_version")]
        public int TokenVersion { get; set; } = 0;

        [Column("ultimo_acceso")]
        public DateTime? UltimoAcceso { get; set; }

        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.Now;

        // Navegación
        public virtual ICollection<Venta> Ventas { get; set; }
        public virtual ICollection<Devolucion> DevolucionesAutorizadas { get; set; }
    }
}
