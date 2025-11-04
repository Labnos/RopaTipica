using Microsoft.EntityFrameworkCore;
using InventarioRopaTipica.Models;

namespace InventarioRopaTipica.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets - Tablas de la base de datos
        public DbSet<User> Users { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Sucursal> Sucursales { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Promocion> Promociones { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetalleVentas { get; set; }
        public DbSet<Envio> Envios { get; set; }
        public DbSet<Devolucion> Devoluciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Estado).HasDefaultValue(true);
                entity.Property(e => e.TokenVersion).HasDefaultValue(0);
            });

            // Configuración de Producto
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Codigo).IsUnique();
                entity.HasIndex(e => e.Nombre);
                entity.Property(e => e.VarasDisponibles).HasDefaultValue(8.00m);
                entity.Property(e => e.VarasOriginales).HasDefaultValue(8.00m);
                entity.Property(e => e.EstadoCorte).HasDefaultValue("Completo");

                // Relación con Proveedor
                entity.HasOne(p => p.Proveedor)
                    .WithMany(pv => pv.Productos)
                    .HasForeignKey(p => p.ProveedorId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con Sucursal
                entity.HasOne(p => p.Sucursal)
                    .WithMany(s => s.Productos)
                    .HasForeignKey(p => p.SucursalId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de Venta
            modelBuilder.Entity<Venta>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.NumeroFactura).IsUnique();
                entity.HasIndex(e => e.Fecha);
                entity.Property(e => e.EstadoVenta).HasDefaultValue("Completada");
                entity.Property(e => e.Descuento).HasDefaultValue(0m);
                entity.Property(e => e.Iva).HasDefaultValue(0m);

                // Relación con Cliente
                entity.HasOne(v => v.Cliente)
                    .WithMany(c => c.Ventas)
                    .HasForeignKey(v => v.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con Usuario
                entity.HasOne(v => v.Usuario)
                    .WithMany(u => u.Ventas)
                    .HasForeignKey(v => v.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de DetalleVenta
            modelBuilder.Entity<DetalleVenta>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TipoVenta).HasDefaultValue("Completo");
                entity.Property(e => e.DescuentoAplicado).HasDefaultValue(0m);

                // Relación con Venta
                entity.HasOne(dv => dv.Venta)
                    .WithMany(v => v.DetallesVentas)
                    .HasForeignKey(dv => dv.VentaId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relación con Producto
                entity.HasOne(dv => dv.Producto)
                    .WithMany(p => p.DetallesVentas)
                    .HasForeignKey(dv => dv.ProductoId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con Promocion
                entity.HasOne(dv => dv.Promocion)
                    .WithMany()
                    .HasForeignKey(dv => dv.PromocionId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configuración de Promocion
            modelBuilder.Entity<Promocion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.FechaInicio, e.FechaFin });
                entity.Property(e => e.Activo).HasDefaultValue(true);

                // Relación con Producto
                entity.HasOne(pr => pr.Producto)
                    .WithMany(p => p.Promociones)
                    .HasForeignKey(pr => pr.ProductoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de Envio
            modelBuilder.Entity<Envio>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CostoEnvio).HasDefaultValue(0m);

                // Relación con Venta
                entity.HasOne(e => e.Venta)
                    .WithMany(v => v.Envios)
                    .HasForeignKey(e => e.VentaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de Devolucion
            modelBuilder.Entity<Devolucion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.FechaDevolucion);

                // Relación con Venta
                entity.HasOne(d => d.Venta)
                    .WithMany(v => v.Devoluciones)
                    .HasForeignKey(d => d.VentaId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con Producto
                entity.HasOne(d => d.Producto)
                    .WithMany()
                    .HasForeignKey(d => d.ProductoId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con Usuario Autorizador
                entity.HasOne(d => d.UsuarioAutorizador)
                    .WithMany(u => u.DevolucionesAutorizadas)
                    .HasForeignKey(d => d.AutorizadoPor)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}