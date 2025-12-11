using Microsoft.EntityFrameworkCore;
using ModulerERP_MVC_.Data;
using ModulerERP_MVC_.Models.Finance;

namespace ModulerERP_MVC_.Finance.Currencies.Repositories
{
    public class CurrencyRepository : ICurrencyRepository
    {
        private readonly ModulesDbContext _context;

        public CurrencyRepository(ModulesDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Currency>> GetAllAsync()
        {
            // ✅ شيل الـ Where - الفلتر هيتطبق من الـ DbContext تلقائياً
            return await _context.Currencies
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Currency?> GetCurrencyByCodeAsync(string code)
        {
            // ✅ الحل: استخدم FirstOrDefaultAsync مباشرة
            // الـ Global Query Filter هيطبق !IsDeleted تلقائياً
            code = code?.Trim().ToUpper() ?? string.Empty;

            return await _context.Currencies
                .FirstOrDefaultAsync(c => c.Code == code);
        }

        public async Task<bool> CodeExistsAsync(string code)
        {
            code = code?.Trim().ToUpper() ?? string.Empty;

            return await _context.Currencies
                .AnyAsync(c => c.Code == code);
        }

        public async Task<Currency?> GetCurrencyByCodeIncludingDeletedAsync(string code)
        {
            code = code?.Trim().ToUpper() ?? string.Empty;

            return await _context.Currencies
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Code == code);
        }

        public async Task AddAsync(Currency currency)
        {
            // ✅ تنظيف الـ Code قبل الإضافة
            currency.Code = currency.Code?.Trim().ToUpperInvariant() ?? string.Empty;

            _context.Currencies.Add(currency);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Currency currency)
        {
            currency.UpdatedAt = DateTime.UtcNow;
            _context.Currencies.Update(currency);
            await _context.SaveChangesAsync();
        }

        // ✅ Soft Delete (الافتراضي)
        public async Task DeleteAsync(string code)
        {
            var currency = await GetCurrencyByCodeAsync(code);
            if (currency != null)
            {
                currency.IsDeleted = true;
                currency.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        // ✅ Hard Delete (حذف نهائي من Database)
        public async Task HardDeleteAsync(string code)
        {
            // ⭐ استخدم IgnoreQueryFilters عشان تلاقي حتى الممسوح
            var currency = await GetCurrencyByCodeIncludingDeletedAsync(code);
            if (currency != null)
            {
                _context.Currencies.Remove(currency);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsInUseAsync(string code)
        {
            code = code?.Trim().ToUpper() ?? string.Empty;

            var vouchers = await _context.Vouchers.AnyAsync(v => v.CurrencyCode == code);
            var treasuries = await _context.Treasuries.AnyAsync(t => t.CurrencyCode == code);
            var bankAccounts = await _context.BankAccounts.AnyAsync(b => b.CurrencyCode == code);

            return vouchers || treasuries || bankAccounts;
        }

        // ✅ Restore - استرجاع عملة ممسوحة
        public async Task<bool> RestoreAsync(string code)
        {
            var currency = await GetCurrencyByCodeIncludingDeletedAsync(code);
            if (currency != null && currency.IsDeleted)
            {
                currency.IsDeleted = false;
                currency.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task HardDeleteAllAsync()
        {
            var allCurrencies = await _context.Currencies
                .IgnoreQueryFilters()
                .ToListAsync();

            _context.Currencies.RemoveRange(allCurrencies);
            await _context.SaveChangesAsync();
        }
    }
}