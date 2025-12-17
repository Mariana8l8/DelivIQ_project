using DelivIQ.Models;
using DelivIQ.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DelivIQ.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<Courier> Couriers => Set<Courier>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderHistory> OrderHistories => Set<OrderHistory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var encRequired = new ValueConverter<string, string>(
                v => EncryptionService.Encrypt(v),
                v => EncryptionService.Decrypt(v)
            );

            var encOptional = new ValueConverter<string?, string?>(
                v => v == null ? null : EncryptionService.Encrypt(v),
                v => v == null ? null : EncryptionService.Decrypt(v)
            );

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.Property(c => c.FullName).HasConversion(encRequired);
                entity.Property(c => c.Phone).HasConversion(encRequired);
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.Property(l => l.Name).HasConversion(encRequired);
                entity.Property(l => l.ContactPhone).HasConversion(encOptional);
            });

            modelBuilder.Entity<Courier>(entity =>
            {
                entity.Property(c => c.FullName).HasConversion(encRequired);
                entity.Property(c => c.Phone).HasConversion(encRequired);
                entity.Property(c => c.TransportType).HasConversion(encRequired);
            });

            modelBuilder.Entity<OrderHistory>(entity =>
            {
                entity.Property(h => h.CustomerName).HasConversion(encRequired);
                entity.Property(h => h.CustomerPhone).HasConversion(encRequired);

                entity.Property(h => h.LocationName).HasConversion(encOptional);

                entity.Property(h => h.CourierName).HasConversion(encOptional);
                entity.Property(h => h.CourierPhone).HasConversion(encOptional);
            });
        }
    }
}
