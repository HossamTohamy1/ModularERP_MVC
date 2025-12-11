using ModularERP.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace ModulerERP_MVC_.Models.Finance
{
    public class Currency : BaseEntity
    {
        [Key, MaxLength(3)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(5)]
        public string Symbol { get; set; } = string.Empty;

        public int Decimals { get; set; } = 2;

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; }

        // Navigation properties
        public virtual ICollection<Voucher> Vouchers { get; set; } = new List<Voucher>();
        public virtual ICollection<Treasury> Treasuries { get; set; } = new List<Treasury>();
        public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
        public virtual ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
    }
}
