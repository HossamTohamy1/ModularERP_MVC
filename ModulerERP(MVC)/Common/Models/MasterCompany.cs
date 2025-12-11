using ModulerERP_MVC_.Common.Enums.Finance_Enum;
using System.ComponentModel.DataAnnotations;

namespace ModulerERP_MVC_.Common.Models
{
    public class MasterCompany
    {
        public Guid Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [Required, MaxLength(3)]
        public string CurrencyCode { get; set; } = "EGP";
        public CompanyStatus Status { get; set; } = CompanyStatus.Active;
        public string? DatabaseName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
