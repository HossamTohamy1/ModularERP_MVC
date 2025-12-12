using ModulerERP_MVC_.Common.Enums.Finance_Enum;
using System.ComponentModel.DataAnnotations;

namespace ModulerERP_MVC_.Finance.GlAccounts.ViewModels
{
    public class GlAccountViewModel
    {
        public Guid Id { get; set; }

    [Required(ErrorMessage = "Account code is required")]
    [StringLength(20, ErrorMessage = "Code cannot exceed 20 characters")]
    [Display(Name = "Account Code")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Account name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    [Display(Name = "Account Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Account type is required")]
    [Display(Name = "Account Type")]
    public AccountType Type { get; set; }

    [Display(Name = "Is Leaf Account")]
    public bool IsLeaf { get; set; } = true;

    [Required(ErrorMessage = "Company is required")]
    [Display(Name = "Company")]
    public Guid CompanyId { get; set; }

    [Display(Name = "Company Name")]
    public string CompanyName { get; set; } = string.Empty;

    [Display(Name = "Created Date")]
    [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy HH:mm}")]
    public DateTime CreatedAt { get; set; }
}

public class GlAccountListViewModel
{
    public IEnumerable<GlAccountViewModel> GlAccounts { get; set; } = new List<GlAccountViewModel>();
    public AccountType? FilterType { get; set; }
    public string? SearchTerm { get; set; }
    public bool? IsLeaf { get; set; }
}
}