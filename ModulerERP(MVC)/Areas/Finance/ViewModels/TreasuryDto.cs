// Areas/Finance/ViewModels/TreasuryViewModel.cs
using ModularERP.Common.Enum.Finance_Enum;
using System.ComponentModel.DataAnnotations;

namespace ModulerERP_MVC_.Areas.Finance.ViewModels
{
    public class TreasuryViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "اسم الخزينة مطلوب")]
        [StringLength(100, ErrorMessage = "الاسم لا يمكن أن يتجاوز 100 حرف")]
        [Display(Name = "اسم الخزينة")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "الشركة مطلوبة")]
        [Display(Name = "الشركة")]
        public Guid CompanyId { get; set; }

        [Display(Name = "اسم الشركة")]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "العملة مطلوبة")]
        [Display(Name = "العملة")]
        public string CurrencyCode { get; set; } = "EGP";

        [Display(Name = "رمز العملة")]
        public string CurrencySymbol { get; set; } = string.Empty;

        [Required(ErrorMessage = "الحالة مطلوبة")]
        [Display(Name = "الحالة")]
        public TreasuryStatus Status { get; set; } = TreasuryStatus.Active;

        [Display(Name = "الوصف")]
        [StringLength(500, ErrorMessage = "الوصف لا يمكن أن يتجاوز 500 حرف")]
        public string? Description { get; set; }

        [Display(Name = "حساب اليومية")]
        public Guid? JournalAccountId { get; set; }

        [Display(Name = "اسم حساب اليومية")]
        public string? JournalAccountName { get; set; }

        [Display(Name = "الرصيد")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Balance { get; set; }

        [Display(Name = "تاريخ الإنشاء")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
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

        [Display(Name = "اسم الخزينة")]
        public string TreasuryName { get; set; } = string.Empty;

        [Display(Name = "العملة")]
        public string CurrencyCode { get; set; } = string.Empty;

        [Display(Name = "رمز العملة")]
        public string CurrencySymbol { get; set; } = string.Empty;

        [Display(Name = "الرصيد الحالي")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Balance { get; set; }

        [Display(Name = "إجمالي الإيرادات")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalIncome { get; set; }

        [Display(Name = "إجمالي المصروفات")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalExpense { get; set; }

        [Display(Name = "تاريخ آخر عملية")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime LastTransactionDate { get; set; }

        [Display(Name = "عدد العمليات")]
        public int TransactionCount { get; set; }
    }
}