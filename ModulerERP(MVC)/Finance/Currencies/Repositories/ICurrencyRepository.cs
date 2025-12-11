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
    }
}
