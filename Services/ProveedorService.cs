using Microsoft.EntityFrameworkCore;
using InventarioRopaTipica.Data;
using InventarioRopaTipica.DTOs;
using InventarioRopaTipica.Models;

namespace InventarioRopaTipica.Services
{
    public class ProveedorService : IProveedorService
    {
        private readonly ApplicationDbContext _context;

        public ProveedorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<ProveedorDto>>> GetAllProveedoresAsync()
        {
            try
            {
                var proveedores = await _context.Proveedores
                    .Select(p => new ProveedorDto
                    {
                        Id = p.Id,
                        Nombre = p.Nombre,
                        Telefono = p.Telefono,
                        Direccion = p.Direccion,
                        Email = p.Email,
                        CreadoEn = p.CreadoEn
                    })
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                return ApiResponse<List<ProveedorDto>>.SuccessResponse(proveedores);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ProveedorDto>>.ErrorResponse($"Error al obtener proveedores: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ProveedorDto>> GetProveedorByIdAsync(int id)
        {
            try
            {
                var proveedor = await _context.Proveedores
                    .Where(p => p.Id == id)
                    .Select(p => new ProveedorDto
                    {
                        Id = p.Id,
                        Nombre = p.Nombre,
                        Telefono = p.Telefono,
                        Direccion = p.Direccion,
                        Email = p.Email,
                        CreadoEn = p.CreadoEn
                    })
                    .FirstOrDefaultAsync();

                if (proveedor == null)
                    return ApiResponse<ProveedorDto>.ErrorResponse("Proveedor no encontrado");

                return ApiResponse<ProveedorDto>.SuccessResponse(proveedor);
            }
            catch (Exception ex)
            {
                return ApiResponse<ProveedorDto>.ErrorResponse($"Error al obtener proveedor: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ProveedorDto>> CreateProveedorAsync(CreateProveedorDto createProveedorDto)
        {
            try
            {
                var proveedor = new Proveedor
                {
                    Nombre = createProveedorDto.Nombre,
                    Telefono = createProveedorDto.Telefono,
                    Direccion = createProveedorDto.Direccion,
                    Email = createProveedorDto.Email,
                    CreadoEn = DateTime.Now
                };

                _context.Proveedores.Add(proveedor);
                await _context.SaveChangesAsync();

                var proveedorDto = new ProveedorDto
                {
                    Id = proveedor.Id,
                    Nombre = proveedor.Nombre,
                    Telefono = proveedor.Telefono,
                    Direccion = proveedor.Direccion,
                    Email = proveedor.Email,
                    CreadoEn = proveedor.CreadoEn
                };

                return ApiResponse<ProveedorDto>.SuccessResponse(proveedorDto, "Proveedor creado exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<ProveedorDto>.ErrorResponse($"Error al crear proveedor: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ProveedorDto>> UpdateProveedorAsync(int id, CreateProveedorDto updateProveedorDto)
        {
            try
            {
                var proveedor = await _context.Proveedores.FindAsync(id);
                if (proveedor == null)
                    return ApiResponse<ProveedorDto>.ErrorResponse("Proveedor no encontrado");

                proveedor.Nombre = updateProveedorDto.Nombre;
                proveedor.Telefono = updateProveedorDto.Telefono;
                proveedor.Direccion = updateProveedorDto.Direccion;
                proveedor.Email = updateProveedorDto.Email;

                await _context.SaveChangesAsync();

                var proveedorDto = new ProveedorDto
                {
                    Id = proveedor.Id,
                    Nombre = proveedor.Nombre,
                    Telefono = proveedor.Telefono,
                    Direccion = proveedor.Direccion,
                    Email = proveedor.Email,
                    CreadoEn = proveedor.CreadoEn
                };

                return ApiResponse<ProveedorDto>.SuccessResponse(proveedorDto, "Proveedor actualizado exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<ProveedorDto>.ErrorResponse($"Error al actualizar proveedor: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteProveedorAsync(int id)
        {
            try
            {
                var proveedor = await _context.Proveedores.FindAsync(id);
                if (proveedor == null)
                    return ApiResponse<bool>.ErrorResponse("Proveedor no encontrado");

                _context.Proveedores.Remove(proveedor);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Proveedor eliminado exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error al eliminar proveedor: {ex.Message}");
            }
        }
    }
}