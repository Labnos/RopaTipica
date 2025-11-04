using System.ComponentModel.DataAnnotations;

namespace InventarioRopaTipica.DTOs
{
    public class ProveedorDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public string Email { get; set; }
        public DateTime CreadoEn { get; set; }
    }

    public class CreateProveedorDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(150)]
        public string Nombre { get; set; }

        [MaxLength(20)]
        public string Telefono { get; set; }

        [MaxLength(255)]
        public string Direccion { get; set; }

        [EmailAddress(ErrorMessage = "Email inválido")]
        [MaxLength(150)]
        public string Email { get; set; }
    }
}