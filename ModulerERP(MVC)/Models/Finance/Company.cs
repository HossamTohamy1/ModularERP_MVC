using ModularERP.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace ModulerERP_MVC_.Models.Finance
{
    public class Company : BaseEntity
    {

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(3)]
        public string CurrencyCode { get; set; } = "EGP";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Treasury> Treasuries { get; set; } = new List<Treasury>();
        public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
        public virtual ICollection<GlAccount> GlAccounts { get; set; } = new List<GlAccount>();
        public virtual ICollection<Voucher> Vouchers { get; set; } = new List<Voucher>();
    }
}
