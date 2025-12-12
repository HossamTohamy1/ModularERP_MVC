namespace ModulerERP_MVC_.Finance.Company.ViewModels
{
    public class CompanyListViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public string CurrencyName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int TreasuriesCount { get; set; }
        public int BankAccountsCount { get; set; }
        public int VouchersCount { get; set; }
    }
}
