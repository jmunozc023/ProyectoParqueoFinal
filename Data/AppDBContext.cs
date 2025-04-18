using Microsoft.EntityFrameworkCore;
using ProyectoParqueoFinal.Models;
using System.Security.Cryptography;

namespace ProyectoParqueoFinal.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public DbSet<Parqueo> Parqueos { get; set; }
        public DbSet<Bitacora> Bitacoras { get; set; }
        public DbSet<Vehiculo> Vehiculos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Parqueo>(tb =>
            {
                tb.HasKey(col => col.IdParqueo);
                tb.Property(col => col.IdParqueo).UseIdentityColumn().ValueGeneratedOnAdd();

                tb.Property(col => col.NombreParqueo).HasMaxLength(25).IsRequired();
                tb.Property(col => col.Ubicacion).HasMaxLength(100).IsRequired();
                tb.Property(col => col.CapacidadAutomoviles).IsRequired();
                tb.Property(col => col.CapacidadMotocicletas).IsRequired();
                tb.Property(col => col.CapacidadLey7600).IsRequired();

            });
            modelBuilder.Entity<Bitacora>(tb =>
            {
                tb.HasKey(col => col.IdBitacora);
                tb.Property(col => col.IdBitacora).UseIdentityColumn().ValueGeneratedOnAdd();
                tb.Property(col => col.TipoIngreso).HasMaxLength(25).IsRequired();
                tb.Property(col => col.FechaHora).IsRequired();
                tb.Property(col => col.NumeroPlaca).HasMaxLength(15);
                tb.HasOne(col => col.Vehiculo).WithMany(col => col.Bitacoras).HasForeignKey(col => col.VehiculosIdVehiculo).OnDelete(DeleteBehavior.NoAction);
                tb.HasOne(col => col.Parqueo).WithMany(col => col.Bitacoras).HasForeignKey(col => col.ParqueoIdParqueo);
            });
            modelBuilder.Entity<Vehiculo>(tb =>
            {
                tb.HasKey(col => col.IdVehiculo);
                tb.Property(col => col.IdVehiculo).UseIdentityColumn().ValueGeneratedOnAdd();
                tb.Property(col => col.NumeroPlaca).HasMaxLength(15).IsRequired();
                tb.Property(col => col.Marca).HasMaxLength(40).IsRequired();
                tb.Property(col => col.Modelo).HasMaxLength(40).IsRequired();
                tb.Property(col => col.Color).HasMaxLength(40).IsRequired();
                tb.Property(col => col.TipoVehiculo).HasMaxLength(40).IsRequired();
                tb.Property(col => col.UsaEspacio7600).IsRequired();
                tb.HasOne(col => col.Usuario).WithMany(col => col.Vehiculos).HasForeignKey(col => col.UsuariosIdUsuario).OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Usuario>(tb =>
            {
                tb.HasKey(col => col.IdUsuario);
                tb.Property(col => col.IdUsuario).UseIdentityColumn().ValueGeneratedOnAdd();
                tb.Property(col => col.Nombre).HasMaxLength(30).IsRequired();
                tb.Property(col => col.Apellido).HasMaxLength(30).IsRequired();
                tb.Property(col => col.CorreoElectronico).HasMaxLength(100).IsRequired();
                tb.Property(col => col.FechaNacimiento).IsRequired();
                tb.Property(col => col.Cedula).HasMaxLength(15).IsRequired();
                tb.Property(col => col.NumeroCarne).HasMaxLength(5).IsRequired();
                tb.Property(col => col.Password).HasMaxLength(256).IsRequired().HasConversion(v => EncryptPassword(v), v =>v);
                tb.Property(col => col.RequiereCambioPassword).IsRequired();
                tb.Property(col => col.Rol).HasMaxLength(20).IsRequired();

            });
        }
        private string EncryptPassword(string password)
        { 
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=tcp:adis-server.database.windows.net,1433;Initial Catalog=adisDB;Persist Security Info=False;User ID=jmunozc023;Password=M3t4l112$;");
        }
    }
}
