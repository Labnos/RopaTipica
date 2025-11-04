using System.ComponentModel.DataAnnotations;

namespace InventarioRopaTipica.DTOs
{
    public class UpdateUserDto
    {
        [MaxLength(100)]
        public string Nombre { get; set; }

        [EmailAddress(ErrorMessage = "Email inválido")]
        [MaxLength(150)]
        public string Email { get; set; }

        public string Rol { get; set; }
        public bool? Estado { get; set; }
    }
}