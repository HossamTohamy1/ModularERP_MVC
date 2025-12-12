using ModulerERP_MVC_.Models.Finance;

namespace ModulerERP_MVC_.Finance.Currencies.Repositories
{
    public interface ICurrencyRepository
    {
        Task<IEnumerable<Currency>> GetAllAsync();
        Task<Currency?> GetCurrencyByCodeAsync(string code);
        Task<bool> CodeExistsAsync(string code);
        Task<Currency?> GetCurrencyByCodeIncludingDeletedAsync(string code);

        Task AddAsync(Currency currency);
        Task UpdateAsync(Currency currency);

        Task DeleteAsync(string code);
        Task HardDeleteAsync(string code);
        Task<bool> IsInUseAsync(string code);

        Task<bool> RestoreAsync(string code);

        // ✅ إضافة الـ Method الناقص (alias for CodeExistsAsync)
        Task<bool> CurrencyExistsByCodeAsync(string code);
    }
}

// ✅ Implementation في CurrencyRepository
// أضف في CurrencyRepository.cs:
// public async Task<bool> CurrencyExistsByCodeAsync(string code)
// {
//     return await CodeExistsAsync(code);
// }