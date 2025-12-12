using System.ComponentModel.DataAnnotations;

namespace ModulerERP_MVC_.Finance.Company.ViewModels
{
    public class UpdateCompanyViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Company name is required")]
        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters")]
        [Display(Name = "Company Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Currency is required")]
        [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency code must be exactly 3 uppercase letters")]
        [Display(Name = "Currency Code")]
        public string CurrencyCode { get; set; } = "EGP";
    }
}