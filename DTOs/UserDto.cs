namespace InventarioRopaTipica.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Rol { get; set; }
        public bool Estado { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public DateTime CreadoEn { get; set; }
    }
}