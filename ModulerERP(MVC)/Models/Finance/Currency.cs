using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModulerERP_MVC_.Models.Finance
{
    public class Currency
    {
        // ⭐ Primary Key - Code (3 أحرف)
        [Key]
        [MaxLength(3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Code { get; set; } = string.Empty;

        // ⭐ خصائص العملة الأساسية
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(5)]
        public string Symbol { get; set; } = string.Empty;

        public int Decimals { get; set; } = 2;

        // ⭐ حالة العملة
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        // ⭐ Audit Fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public Guid? CreatedById { get; set; }
        public Guid? UpdatedById { get; set; }

        // ⭐ Navigation properties
        public virtual ICollection<Voucher> Vouchers { get; set; } = new List<Voucher>();
        public virtual ICollection<Treasury> Treasuries { get; set; } = new List<Treasury>();
        public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
        public virtual ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
    }
}