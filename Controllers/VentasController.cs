using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using InventarioRopaTipica.DTOs;
using InventarioRopaTipica.Services;

namespace InventarioRopaTipica.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VentasController : ControllerBase
    {
        private readonly IVentaService _ventaService;

        public VentasController(IVentaService ventaService)
        {
            _ventaService = ventaService;
        }

        /// <summary>
        /// GET: api/Ventas
        /// Obtiene todas las ventas
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllVentas()
        {
            var result = await _ventaService.GetAllVentasAsync();
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// GET: api/Ventas/{id}
        /// Obtiene una venta por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVentaById(int id)
        {
            var result = await _ventaService.GetVentaByIdAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        /// <summary>
        /// POST: api/Ventas
        /// Crea una nueva venta
        /// CON VALIDACIÓN AUTOMÁTICA DE VENTAS PARCIALES/COMPLETAS
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateVenta([FromBody] CreateVentaDto createVentaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Datos inválidos",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            // Obtener el ID del usuario autenticado
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Usuario no autenticado"
                });
            }

            int userId = int.Parse(userIdClaim.Value);

            var result = await _ventaService.CreateVentaAsync(createVentaDto, userId);
            if (!result.Success) return BadRequest(result);
            
            return CreatedAtAction(nameof(GetVentaById), new { id = result.Data.Id }, result);
        }

        /// <summary>
        /// POST: api/Ventas/{id}/Cancelar
        /// Cancela una venta y devuelve el stock
        /// </summary>
        [HttpPost("{id}/Cancelar")]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> CancelarVenta(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            var result = await _ventaService.CancelarVentaAsync(id, userId);
            if (!result.Success) return BadRequest(result);
            
            return Ok(result);
        }

        /// <summary>
        /// GET: api/Ventas/ValidarDevolucion/{detalleVentaId}
        /// VALIDACIÓN CRÍTICA: Verifica si un producto puede ser devuelto
        /// Regla: NO se permiten devoluciones de ventas PARCIALES
        /// </summary>
        [HttpGet("ValidarDevolucion/{detalleVentaId}")]
        public async Task<IActionResult> ValidarDevolucion(int detalleVentaId)
        {
            var result = await _ventaService.ValidarDevolucionAsync(detalleVentaId);
            
            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message,
                    puedeDevolver = false
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message,
                puedeDevolver = true
            });
        }
    }
}