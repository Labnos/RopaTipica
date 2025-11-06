namespace InventarioRopaTipica.Models
{
    public class Promocion
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Producto { get; set; }
        public string Tipo { get; set; } // Porcentaje, Monto
        public decimal ValorDescuento { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public DateTime CreadoEn { get; set; }
    }
}