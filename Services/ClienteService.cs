using Microsoft.EntityFrameworkCore;
using InventarioRopaTipica.Data;
using InventarioRopaTipica.DTOs;
using InventarioRopaTipica.Models;

namespace InventarioRopaTipica.Services
{
    public class ClienteService : IClienteService
    {
        private readonly ApplicationDbContext _context;

        public ClienteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<ClienteDto>>> GetAllClientesAsync()
        {
            try
            {
                var clientes = await _context.Clientes
                    .Select(c => new ClienteDto
                    {
                        Id = c.Id,
                        Nombre = c.Nombre,
                        Telefono = c.Telefono,
                        Direccion = c.Direccion,
                        Email = c.Email,
                        CreadoEn = c.CreadoEn
                    })
                    .OrderBy(c => c.Nombre)
                    .ToListAsync();

                return ApiResponse<List<ClienteDto>>.SuccessResponse(clientes);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ClienteDto>>.ErrorResponse($"Error al obtener clientes: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ClienteDto>> GetClienteByIdAsync(int id)
        {
            try
            {
                var cliente = await _context.Clientes
                    .Where(c => c.Id == id)
                    .Select(c => new ClienteDto
                    {
                        Id = c.Id,
                        Nombre = c.Nombre,
                        Telefono = c.Telefono,
                        Direccion = c.Direccion,
                        Email = c.Email,
                        CreadoEn = c.CreadoEn
                    })
                    .FirstOrDefaultAsync();

                if (cliente == null)
                    return ApiResponse<ClienteDto>.ErrorResponse("Cliente no encontrado");

                return ApiResponse<ClienteDto>.SuccessResponse(cliente);
            }
            catch (Exception ex)
            {
                return ApiResponse<ClienteDto>.ErrorResponse($"Error al obtener cliente: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ClienteDto>> CreateClienteAsync(CreateClienteDto createClienteDto)
        {
            try
            {
                var cliente = new Cliente
                {
                    Nombre = createClienteDto.Nombre,
                    Telefono = createClienteDto.Telefono,
                    Direccion = createClienteDto.Direccion,
                    Email = createClienteDto.Email,
                    CreadoEn = DateTime.Now
                };

                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();

                var clienteDto = new ClienteDto
                {
                    Id = cliente.Id,
                    Nombre = cliente.Nombre,
                    Telefono = cliente.Telefono,
                    Direccion = cliente.Direccion,
                    Email = cliente.Email,
                    CreadoEn = cliente.CreadoEn
                };

                return ApiResponse<ClienteDto>.SuccessResponse(clienteDto, "Cliente creado exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<ClienteDto>.ErrorResponse($"Error al crear cliente: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ClienteDto>> UpdateClienteAsync(int id, CreateClienteDto updateClienteDto)
        {
            try
            {
                var cliente = await _context.Clientes.FindAsync(id);
                if (cliente == null)
                    return ApiResponse<ClienteDto>.ErrorResponse("Cliente no encontrado");

                cliente.Nombre = updateClienteDto.Nombre;
                cliente.Telefono = updateClienteDto.Telefono;
                cliente.Direccion = updateClienteDto.Direccion;
                cliente.Email = updateClienteDto.Email;

                await _context.SaveChangesAsync();

                var clienteDto = new ClienteDto
                {
                    Id = cliente.Id,
                    Nombre = cliente.Nombre,
                    Telefono = cliente.Telefono,
                    Direccion = cliente.Direccion,
                    Email = cliente.Email,
                    CreadoEn = cliente.CreadoEn
                };

                return ApiResponse<ClienteDto>.SuccessResponse(clienteDto, "Cliente actualizado exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<ClienteDto>.ErrorResponse($"Error al actualizar cliente: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteClienteAsync(int id)
        {
            try
            {
                var cliente = await _context.Clientes.FindAsync(id);
                if (cliente == null)
                    return ApiResponse<bool>.ErrorResponse("Cliente no encontrado");

                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Cliente eliminado exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error al eliminar cliente: {ex.Message}");
            }
        }
    }
}