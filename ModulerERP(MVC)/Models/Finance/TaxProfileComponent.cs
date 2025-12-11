using ModulerERP_MVC_.Models.Finance;
using System.ComponentModel.DataAnnotations;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.Models
{
    /// <summary>
    /// Many-to-Many relationship between TaxProfile and TaxComponent
    /// Composite Primary Key: (TaxProfileId, TaxComponentId)
    /// </summary>
    public class TaxProfileComponent
    {
        // Composite Key - Part 1
        public Guid TaxProfileId { get; set; }

        // Composite Key - Part 2
        public Guid TaxComponentId { get; set; }

        /// <summary>
        /// Priority determines the order of tax calculation
        /// Lower number = calculated first
        /// </summary>
        [Range(1, 100)]
        public int Priority { get; set; } = 1;

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Soft Delete
        public bool IsDeleted { get; set; }

        // Navigation Properties
        public virtual TaxProfile TaxProfile { get; set; } = null!;
        public virtual TaxComponent TaxComponent { get; set; } = null!;
    }
}