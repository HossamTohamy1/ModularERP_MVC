using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ModularERP.Common.Models;
using ModularERP.Modules.Inventory.Features.TaxManagement.Models;
using ModulerERP_MVC_.Models.Finance;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ModulerERP_MVC_.Data
{
    public class ModulesDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ModulesDbContext(DbContextOptions<ModulesDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Company> Companies { get; set; }
        public DbSet<Treasury> Treasuries { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<GlAccount> GlAccounts { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<VoucherTax> VoucherTaxes { get; set; }
        public DbSet<LedgerEntry> LedgerEntries { get; set; }
        public DbSet<VoucherAttachment> VoucherAttachments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<RecurringSchedule> RecurringSchedules { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<TaxProfile> TaxProfiles { get; set; }
        public DbSet<TaxComponent> TaxComponents { get; set; }
        public DbSet<TaxProfileComponent> TaxProfileComponents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all configurations from assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ModulesDbContext).Assembly);

            // Configure Identity tables with custom prefix (optional)
            modelBuilder.Entity<ApplicationUser>(b =>
            {
                b.ToTable("Users");
            });

            modelBuilder.Entity<IdentityRole<Guid>>(b =>
            {
                b.ToTable("Roles");
            });

            modelBuilder.Entity<IdentityUserRole<Guid>>(b =>
            {
                b.ToTable("UserRoles");
            });

            modelBuilder.Entity<IdentityUserClaim<Guid>>(b =>
            {
                b.ToTable("UserClaims");
            });

            modelBuilder.Entity<IdentityUserLogin<Guid>>(b =>
            {
                b.ToTable("UserLogins");
            });

            modelBuilder.Entity<IdentityRoleClaim<Guid>>(b =>
            {
                b.ToTable("RoleClaims");
            });

            modelBuilder.Entity<IdentityUserToken<Guid>>(b =>
            {
                b.ToTable("UserTokens");
            });

            // ⭐ Configure Currency FIRST (لازم يتعرف قبل ما نعمل relationships)
            ConfigureCurrency(modelBuilder);

            // Global Query Filters for Soft Delete
            modelBuilder.Entity<Treasury>().HasQueryFilter(t => !t.IsDeleted);
            modelBuilder.Entity<BankAccount>().HasQueryFilter(b => !b.IsDeleted);
            modelBuilder.Entity<GlAccount>().HasQueryFilter(g => !g.IsDeleted);
            modelBuilder.Entity<Voucher>().HasQueryFilter(v => !v.IsDeleted);
            modelBuilder.Entity<Vendor>().HasQueryFilter(v => !v.IsDeleted);
            modelBuilder.Entity<Customer>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Company>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<TaxProfileComponent>().HasQueryFilter(tpc => !tpc.IsDeleted);

            // Configure relationships and additional constraints
            ConfigureRelationships(modelBuilder);
            ConfigureIndexes(modelBuilder);
        }

        // ⭐ Method منفصل لـ Currency Configuration
        private void ConfigureCurrency(ModelBuilder modelBuilder)
        {

            // ⭐ Primary Key هو Code مش Id
            modelBuilder.Entity<Currency>(entity =>
            {
                // Primary Key هو Code
                entity.HasKey(c => c.Code);

                // Properties Configuration
                entity.Property(c => c.Code)
                    .HasMaxLength(3)
                    .IsRequired();

                entity.Property(c => c.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(c => c.Symbol)
                    .HasMaxLength(5)
                    .IsRequired();

                entity.Property(c => c.Decimals)
                    .HasDefaultValue(2);

                entity.Property(c => c.IsActive)
                    .HasDefaultValue(true);

                entity.Property(c => c.IsDeleted)
                    .HasDefaultValue(false);

                entity.Property(c => c.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // ⭐ Global Query Filter - إخفاء الممسوح
                entity.HasQueryFilter(c => !c.IsDeleted);

                // ⭐ Indexes للأداء
                entity.HasIndex(c => c.Name);
                entity.HasIndex(c => c.IsDeleted);
                entity.HasIndex(c => c.IsActive);
            });

        }
            


        private void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            // ========================================
            // ⭐ Company relationships
            // ========================================
            modelBuilder.Entity<Company>()
                .HasMany(c => c.Treasuries)
                .WithOne(t => t.Company)
                .HasForeignKey(t => t.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Company>()
                .HasMany(c => c.BankAccounts)
                .WithOne(b => b.Company)
                .HasForeignKey(b => b.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Company>()
                .HasMany(c => c.GlAccounts)
                .WithOne(g => g.Company)
                .HasForeignKey(g => g.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Company>()
                .HasMany(c => c.Vouchers)
                .WithOne(v => v.Company)
                .HasForeignKey(v => v.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            // ⭐ Company → Currency (مهم: HasPrincipalKey)
            modelBuilder.Entity<Company>()
                .HasOne(c => c.Currency)
                .WithMany() // مفيش Collection في Currency
                .HasForeignKey(c => c.CurrencyCode)
                .HasPrincipalKey(c => c.Code) // ⭐ لأن Primary Key هو Code
                .OnDelete(DeleteBehavior.Restrict);

            // ========================================
            // ⭐ Treasury relationships
            // ========================================
            modelBuilder.Entity<Treasury>()
                .HasOne(t => t.Currency)
                .WithMany(c => c.Treasuries) // Currency عندها كذا Treasury
                .HasForeignKey(t => t.CurrencyCode)
                .HasPrincipalKey(c => c.Code) // ⭐ مهم جداً
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Treasury>()
                .HasOne(t => t.JournalAccount)
                .WithMany()
                .HasForeignKey(t => t.JournalAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========================================
            // ⭐ BankAccount relationships
            // ========================================
            modelBuilder.Entity<BankAccount>()
                .HasOne(b => b.Currency)
                .WithMany(c => c.BankAccounts)
                .HasForeignKey(b => b.CurrencyCode)
                .HasPrincipalKey(c => c.Code) // ⭐ مهم جداً
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BankAccount>()
                .HasOne(b => b.JournalAccount)
                .WithMany()
                .HasForeignKey(b => b.JournalAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========================================
            // ⭐ Voucher relationships
            // ========================================
            modelBuilder.Entity<Voucher>()
                .HasOne(v => v.CategoryAccount)
                .WithMany(g => g.CategoryVouchers)
                .HasForeignKey(v => v.CategoryAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Voucher>()
                .HasOne(v => v.JournalAccount)
                .WithMany(g => g.JournalVouchers)
                .HasForeignKey(v => v.JournalAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // ⭐ Voucher → Currency
            modelBuilder.Entity<Voucher>()
                .HasOne(v => v.Currency)
                .WithMany(c => c.Vouchers)
                .HasForeignKey(v => v.CurrencyCode)
                .HasPrincipalKey(c => c.Code)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Voucher>()
                .HasOne(v => v.Creator)
                .WithMany(u => u.CreatedVouchers)
                .HasForeignKey(v => v.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Voucher>()
                .HasOne(v => v.Poster)
                .WithMany(u => u.PostedVouchers)
                .HasForeignKey(v => v.PostedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Voucher>()
                .HasOne(v => v.Reverser)
                .WithMany(u => u.ReversedVouchers)
                .HasForeignKey(v => v.ReversedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Voucher>()
                .HasOne(v => v.RecurringSchedule)
                .WithMany(r => r.Vouchers)
                .HasForeignKey(v => v.RecurrenceId)
                .OnDelete(DeleteBehavior.SetNull);

            // ========================================
            // ⭐ VoucherTax relationships
            // ========================================
            modelBuilder.Entity<VoucherTax>()
                .HasOne(vt => vt.Voucher)
                .WithMany(v => v.VoucherTaxes)
                .HasForeignKey(vt => vt.VoucherId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VoucherTax>()
                .HasOne(vt => vt.TaxProfile)
                .WithMany()
                .HasForeignKey(vt => vt.TaxProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VoucherTax>()
                .HasOne(vt => vt.TaxComponent)
                .WithMany()
                .HasForeignKey(vt => vt.TaxComponentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========================================
            // ⭐ LedgerEntry relationships
            // ========================================
            modelBuilder.Entity<LedgerEntry>()
                .HasOne(le => le.Voucher)
                .WithMany(v => v.LedgerEntries)
                .HasForeignKey(le => le.VoucherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LedgerEntry>()
                .HasOne(le => le.GlAccount)
                .WithMany(g => g.LedgerEntries)
                .HasForeignKey(le => le.GlAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // ⭐ LedgerEntry → Currency
            modelBuilder.Entity<LedgerEntry>()
                .HasOne(le => le.Currency)
                .WithMany(c => c.LedgerEntries)
                .HasForeignKey(le => le.CurrencyCode)
                .HasPrincipalKey(c => c.Code) // ⭐ مهم جداً
                .OnDelete(DeleteBehavior.Restrict);

            // ========================================
            // ⭐ VoucherAttachment relationships
            // ========================================
            modelBuilder.Entity<VoucherAttachment>()
                .HasOne(va => va.Voucher)
                .WithMany(v => v.Attachments)
                .HasForeignKey(va => va.VoucherId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VoucherAttachment>()
                .HasOne(va => va.UploadedByUser)
                .WithMany(u => u.UploadedAttachments)
                .HasForeignKey(va => va.UploadedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // ========================================
            // ⭐ AuditLog relationships
            // ========================================
            modelBuilder.Entity<AuditLog>()
                .HasOne(al => al.Voucher)
                .WithMany(v => v.AuditLogs)
                .HasForeignKey(al => al.VoucherId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AuditLog>()
                .HasOne(al => al.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========================================
            // ⭐ RecurringSchedule relationships
            // ========================================
            modelBuilder.Entity<RecurringSchedule>()
                .HasOne(rs => rs.Creator)
                .WithMany(u => u.CreatedSchedules)
                .HasForeignKey(rs => rs.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // ========================================
            // ⭐ Vendor and Customer relationships
            // ========================================
            modelBuilder.Entity<Vendor>()
                .HasOne(v => v.Company)
                .WithMany()
                .HasForeignKey(v => v.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Customer>()
                .HasOne(c => c.Company)
                .WithMany()
                .HasForeignKey(c => c.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========================================
            // ⭐ TaxProfileComponent relationships (Many-to-Many)
            // ========================================
            modelBuilder.Entity<TaxProfileComponent>()
                .HasKey(tpc => new { tpc.TaxProfileId, tpc.TaxComponentId });

            modelBuilder.Entity<TaxProfileComponent>()
                .HasOne(tpc => tpc.TaxProfile)
                .WithMany(tp => tp.TaxProfileComponents)
                .HasForeignKey(tpc => tpc.TaxProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaxProfileComponent>()
                .HasOne(tpc => tpc.TaxComponent)
                .WithMany(tc => tc.TaxProfileComponents)
                .HasForeignKey(tpc => tpc.TaxComponentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Performance Index على Priority
            modelBuilder.Entity<TaxProfileComponent>()
                .HasIndex(tpc => new { tpc.TaxProfileId, tpc.Priority });
        }

        private void ConfigureIndexes(ModelBuilder modelBuilder)
        {
            // ========================================
            // ⭐ Unique indexes with Soft Delete Filter
            // ========================================
            modelBuilder.Entity<Treasury>()
                .HasIndex(t => new { t.CompanyId, t.Name })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            modelBuilder.Entity<BankAccount>()
                .HasIndex(b => new { b.CompanyId, b.AccountNumber })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            modelBuilder.Entity<GlAccount>()
                .HasIndex(g => new { g.CompanyId, g.Code })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            modelBuilder.Entity<Voucher>()
                .HasIndex(v => new { v.CompanyId, v.Code })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            modelBuilder.Entity<Vendor>()
                .HasIndex(v => new { v.CompanyId, v.Code })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            modelBuilder.Entity<Customer>()
                .HasIndex(c => new { c.CompanyId, c.Code })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            // ========================================
            // ⭐ Performance indexes
            // ========================================
            modelBuilder.Entity<Voucher>()
                .HasIndex(v => v.Date);

            modelBuilder.Entity<Voucher>()
                .HasIndex(v => v.Status);

            modelBuilder.Entity<Voucher>()
                .HasIndex(v => new { v.CompanyId, v.Type, v.Status });

            modelBuilder.Entity<LedgerEntry>()
                .HasIndex(le => le.EntryDate);

            modelBuilder.Entity<LedgerEntry>()
                .HasIndex(le => new { le.GlAccountId, le.EntryDate });

            modelBuilder.Entity<LedgerEntry>()
                .HasIndex(le => le.VoucherId);

            modelBuilder.Entity<AuditLog>()
                .HasIndex(al => new { al.VoucherId, al.CreatedAt });

            modelBuilder.Entity<VoucherAttachment>()
                .HasIndex(va => va.VoucherId);
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<Currency>()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}