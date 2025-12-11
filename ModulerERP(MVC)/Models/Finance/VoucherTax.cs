using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Models;
using ModularERP.Modules.Inventory.Features.TaxManagement.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModulerERP_MVC_.Models.Finance
{
    public class VoucherTax : BaseEntity
    {
        public Guid Id { get; set; }
        public Guid VoucherId { get; set; }

        // Replace the old TaxId with these three properties
        public Guid TaxProfileId { get; set; }
        public Guid TaxComponentId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        public bool IsWithholding { get; set; } = false;
        public TaxDirection Direction { get; set; }

        // Optional: Store the tax rate at the time of transaction for audit trail
        [Column(TypeName = "decimal(18,4)")]
        public decimal AppliedRate { get; set; }

        // Navigation properties
        public virtual Voucher Voucher { get; set; } = null!;
        public virtual TaxProfile TaxProfile { get; set; } = null!;
        public virtual TaxComponent TaxComponent { get; set; } = null!;
    }
}