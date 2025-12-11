using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Models;
using ModulerERP_MVC_.Common.Models;

namespace ModulerERP_MVC_.Common.Data
{
    public class MasterDbContext : DbContext
    {
        public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options)
        {
        }

        public DbSet<MasterCompany> MasterCompanies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MasterCompany>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.CurrencyCode)
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasDefaultValue("EGP");

                entity.Property(e => e.DatabaseName)
                    .HasMaxLength(200);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.Name)
                    .IsUnique();

                entity.HasIndex(e => e.DatabaseName)
                    .IsUnique()
                    .HasFilter("[DatabaseName] IS NOT NULL");
            });
        }
    }
}