using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ModularERP.Common.Models;
using ModulerERP_MVC_.Common.Enums.Finance_Enum;

namespace ModulerERP_MVC_.Models.Finance
{
    public class BankAccount : BaseEntity
    {

        public Guid CompanyId { get; set; }

        [Required, MaxLength(100)]
        public string BankName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required, MaxLength(3)]
        public string CurrencyCode { get; set; } = "EGP";

        public BankAccountStatus Status { get; set; } = BankAccountStatus.Active;

        public string? Description { get; set; }

        public string DepositAcl { get; set; } = "{}";

        public string WithdrawAcl { get; set; } = "{}";
        public Guid? JournalAccountId { get; set; }  // FK to GLAccount

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Company Company { get; set; } = null!;
        public virtual Currency Currency { get; set; } = null!;
        public virtual GlAccount? JournalAccount { get; set; }  // Navigation property

        public virtual ICollection<Voucher> Vouchers { get; set; } = new List<Voucher>();
    }
}
