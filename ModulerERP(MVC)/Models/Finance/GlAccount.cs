using ModularERP.Common.Models;
using ModulerERP_MVC_.Common.Enums.Finance_Enum;
using System.ComponentModel.DataAnnotations;

namespace ModulerERP_MVC_.Models.Finance
{
    public class GlAccount : BaseEntity
    {

        [Required, MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public AccountType Type { get; set; }

        public bool IsLeaf { get; set; } = true;

        public Guid CompanyId { get; set; }

        // Navigation properties
        public virtual Company Company { get; set; } = null!;
        public virtual ICollection<Voucher> CategoryVouchers { get; set; } = new List<Voucher>();
        public virtual ICollection<Voucher> JournalVouchers { get; set; } = new List<Voucher>();
        public virtual ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
    }
}
