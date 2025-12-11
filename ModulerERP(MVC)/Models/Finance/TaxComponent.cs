using ModularERP.Common.Models;
using ModularERP.Modules.Inventory.Features.TaxManagement.Models;
using ModulerERP_MVC_.Common.Enums.Finance_Enum;
using System.ComponentModel.DataAnnotations;

namespace ModulerERP_MVC_.Models.Finance
{
    public class TaxComponent : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public TaxRateType RateType { get; set; } // Percentage or Fixed

        [Required]
        [Range(0, 100)]
        public decimal RateValue { get; set; }

        [Required]
        public TaxIncludedType IncludedType { get; set; } // Inclusive or Exclusive

        public TaxAppliesOn AppliesOn { get; set; } = TaxAppliesOn.Both; // Sales, Purchases, or Both


        // Navigation Properties
        public virtual ICollection<TaxProfileComponent> TaxProfileComponents { get; set; } = new List<TaxProfileComponent>();

    }
}
