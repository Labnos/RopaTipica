using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventarioRopaTipica.DTOs;
using InventarioRopaTipica.Services;

namespace InventarioRopaTipica.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductosController : ControllerBase
    {
        private readonly IProductoService _productoService;

        public ProductosController(IProductoService productoService)
        {
            _productoService = productoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProductos()
        {
            var result = await _productoService.GetAllProductosAsync();
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductoById(int id)
        {
            var result = await _productoService.GetProductoByIdAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpGet("tipo/{tipo}")]
        public async Task<IActionResult> GetProductosByTipo(string tipo)
        {
            var result = await _productoService.GetProductosByTipoAsync(tipo);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("sucursal/{sucursalId}")]
        public async Task<IActionResult> GetProductosBySucursal(int sucursalId)
        {
            var result = await _productoService.GetProductosBySucursalAsync(sucursalId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("bajo-stock")]
        public async Task<IActionResult> GetProductosBajoStock()
        {
            var result = await _productoService.GetProductosBajoStockAsync();
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Encargado")]
        public async Task<IActionResult> CreateProducto([FromBody] CreateProductoDto createProductoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _productoService.CreateProductoAsync(createProductoDto);
            if (!result.Success) return BadRequest(result);
            return CreatedAtAction(nameof(GetProductoById), new { id = result.Data.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Encargado")]
        public async Task<IActionResult> UpdateProducto(int id, [FromBody] CreateProductoDto updateProductoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _productoService.UpdateProductoAsync(id, updateProductoDto);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var result = await _productoService.DeleteProductoAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }
    }
}