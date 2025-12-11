using ModularERP.Common.Models;
using ModulerERP_MVC_.Common.Enums.Finance_Enum;
using ModulerERP_MVC_.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModulerERP_MVC_.Models.Finance
{
    public class Voucher : BaseEntity
    {

        public VoucherType Type { get; set; }

        [Required, MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        public VoucherStatus Status { get; set; } = VoucherStatus.Draft;

        public DateTime Date { get; set; } = DateTime.Today;

        [Required, MaxLength(3)]
        public string CurrencyCode { get; set; } = "EGP";

        [Column(TypeName = "decimal(18,6)")]
        public decimal FxRate { get; set; } = 1.0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public string? Description { get; set; }

        public Guid CategoryAccountId { get; set; }

        public WalletType WalletType { get; set; }

        public Guid WalletId { get; set; }

        public CounterpartyType? CounterpartyType { get; set; }

        public Guid? CounterpartyId { get; set; }

        public Guid JournalAccountId { get; set; }

        public Guid? RecurrenceId { get; set; }

        // Audit fields
        public Guid CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid? PostedBy { get; set; }

        public DateTime? PostedAt { get; set; }

        public Guid? ReversedBy { get; set; }

        public DateTime? ReversedAt { get; set; }

        public int Revision { get; set; } = 0;

        public Guid CompanyId { get; set; }

        // Navigation properties
        public virtual Company Company { get; set; } = null!;
        public virtual GlAccount CategoryAccount { get; set; } = null!;
        public virtual GlAccount JournalAccount { get; set; } = null!;
        public virtual Currency Currency { get; set; } = null!;
        public virtual ApplicationUser Creator { get; set; } = null!;
        public virtual ApplicationUser? Poster { get; set; }
        public virtual ApplicationUser? Reverser { get; set; }
        public virtual RecurringSchedule? RecurringSchedule { get; set; }

        public virtual ICollection<VoucherTax> VoucherTaxes { get; set; } = new List<VoucherTax>();
        public virtual ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
        public virtual ICollection<VoucherAttachment> Attachments { get; set; } = new List<VoucherAttachment>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

        // Computed properties for polymorphic wallet relationship
        public object? GetWallet(ModulesDbContext context)
        {
            return WalletType switch
            {
                WalletType.Treasury => context.Treasuries.Find(WalletId),
                WalletType.BankAccount => context.BankAccounts.Find(WalletId),
                _ => null
            };
        }
    }
}
