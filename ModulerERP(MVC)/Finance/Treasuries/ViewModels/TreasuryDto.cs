// Areas/Finance/ViewModels/TreasuryViewModel.cs
using ModulerERP_MVC_.Common.Enums.Finance_Enum;
using System.ComponentModel.DataAnnotations;

namespace ModulerERP_MVC_.Finance.Treasuries.ViewModels
{
    public class TreasuryViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Treasury name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Treasury Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Company is required")]
        [Display(Name = "Company")]
        public Guid CompanyId { get; set; }

        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Currency is required")]
        [Display(Name = "Currency")]
        public string CurrencyCode { get; set; } = "EGP";

        [Display(Name = "Currency Symbol")]
        public string CurrencySymbol { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public TreasuryStatus Status { get; set; } = TreasuryStatus.Active;

        [Display(Name = "Description")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Display(Name = "Journal Account")]
        public Guid? JournalAccountId { get; set; }

        [Display(Name = "Journal Account Name")]
        public string? JournalAccountName { get; set; }

        [Display(Name = "Balance")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Balance { get; set; }

        [Display(Name = "Created Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy HH:mm}")]
        public DateTime CreatedAt { get; set; }
    }

    public class TreasuryListViewModel
    {
        public IEnumerable<TreasuryViewModel> Treasuries { get; set; } = new List<TreasuryViewModel>();
        public TreasuryStatus? FilterStatus { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class TreasuryBalanceViewModel
    {
        public Guid TreasuryId { get; set; }

        [Display(Name = "Treasury Name")]
        public string TreasuryName { get; set; } = string.Empty;

        [Display(Name = "Currency")]
        public string CurrencyCode { get; set; } = string.Empty;

        [Display(Name = "Currency Symbol")]
        public string CurrencySymbol { get; set; } = string.Empty;

        [Display(Name = "Current Balance")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Balance { get; set; }

        [Display(Name = "Total Income")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalIncome { get; set; }

        [Display(Name = "Total Expenses")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalExpense { get; set; }

        [Display(Name = "Last Transaction Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy HH:mm}")]
        public DateTime LastTransactionDate { get; set; }

        [Display(Name = "Transaction Count")]
        public int TransactionCount { get; set; }
    }
}