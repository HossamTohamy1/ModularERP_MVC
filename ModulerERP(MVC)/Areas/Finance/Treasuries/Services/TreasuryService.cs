// Modules/Finance/Services/TreasuryService.cs
using Microsoft.EntityFrameworkCore;
using ModulerERP_MVC_.Models.Finance;
using ModulerERP_MVC_.Modules.Data;
using ModulerERP_MVC_.Areas.Finance.Treasuries.ViewModels;
using ModulerERP_MVC_.Common.Enums.Finance_Enum;

namespace ModulerERP_MVC_.Areas.Finance.Treasuries.Services
{
    public class TreasuryService : ITreasuryService
    {
        private readonly ModulesDbContext _context;
        private readonly ILogger<TreasuryService> _logger;

        public TreasuryService(ModulesDbContext context, ILogger<TreasuryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<TreasuryViewModel>> GetAllAsync(TreasuryStatus? status = null, string? searchTerm = null)
        {
            try
            {
                var query = _context.Treasuries
                    .Include(t => t.Currency)
                    .Include(t => t.Company)
                    .Where(t => !t.IsDeleted)
                    .AsQueryable();

                if (status.HasValue)
                {
                    query = query.Where(t => t.Status == status.Value);
                }

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(t => t.Name.Contains(searchTerm) ||
                                           (t.Description != null && t.Description.Contains(searchTerm)));
                }

                var treasuries = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();

                return treasuries.Select(MapToViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving treasuries");
                throw;
            }
        }

        public async Task<TreasuryViewModel?> GetByIdAsync(Guid id)
        {
            try
            {
                var treasury = await _context.Treasuries
                    .Include(t => t.Currency)
                    .Include(t => t.Company)
                    .Include(t => t.JournalAccount)
                    .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

                return treasury == null ? null : MapToViewModel(treasury);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving treasury {TreasuryId}", id);
                throw;
            }
        }

        public async Task<TreasuryViewModel> CreateAsync(TreasuryViewModel viewModel)
        {
            try
            {
                // Validate company exists
                var companyExists = await _context.Companies
                    .AnyAsync(c => c.Id == viewModel.CompanyId && c.IsActive && !c.IsDeleted);
                if (!companyExists)
                {
                    throw new ArgumentException($"الشركة غير موجودة أو غير نشطة");
                }

                // Validate currency exists
                var currencyExists = await _context.Currencies
                    .AnyAsync(c => c.Code == viewModel.CurrencyCode && c.IsActive && !c.IsDeleted);
                if (!currencyExists)
                {
                    throw new ArgumentException($"العملة غير موجودة أو غير نشطة");
                }

                // Check for duplicate name
                if (await IsDuplicateNameAsync(viewModel.CompanyId, viewModel.Name))
                {
                    throw new InvalidOperationException($"يوجد خزينة بنفس الاسم '{viewModel.Name}' في هذه الشركة");
                }

                var treasury = new Treasury
                {
                    Id = Guid.NewGuid(),
                    CompanyId = viewModel.CompanyId,
                    Name = viewModel.Name,
                    CurrencyCode = viewModel.CurrencyCode,
                    Status = viewModel.Status,
                    Description = viewModel.Description,
                    JournalAccountId = viewModel.JournalAccountId,
                    DepositAcl = "{}",
                    WithdrawAcl = "{}",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                _context.Treasuries.Add(treasury);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Treasury {TreasuryName} created successfully with ID {TreasuryId}",
                    treasury.Name, treasury.Id);

                return (await GetByIdAsync(treasury.Id))!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating treasury");
                throw;
            }
        }

        public async Task<TreasuryViewModel?> UpdateAsync(Guid id, TreasuryViewModel viewModel)
        {
            try
            {
                var treasury = await _context.Treasuries.FindAsync(id);
                if (treasury == null || treasury.IsDeleted)
                {
                    return null;
                }

                // Check for duplicate name (excluding current treasury)
                if (await IsDuplicateNameAsync(viewModel.CompanyId, viewModel.Name, id))
                {
                    throw new InvalidOperationException($"يوجد خزينة بنفس الاسم '{viewModel.Name}' في هذه الشركة");
                }

                // Validate currency exists
                var currencyExists = await _context.Currencies
                    .AnyAsync(c => c.Code == viewModel.CurrencyCode && c.IsActive && !c.IsDeleted);
                if (!currencyExists)
                {
                    throw new ArgumentException($"العملة غير موجودة أو غير نشطة");
                }

                treasury.Name = viewModel.Name;
                treasury.CurrencyCode = viewModel.CurrencyCode;
                treasury.Status = viewModel.Status;
                treasury.Description = viewModel.Description;
                treasury.JournalAccountId = viewModel.JournalAccountId;
                treasury.UpdatedAt = DateTime.UtcNow;

                _context.Update(treasury);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Treasury {TreasuryId} updated successfully", id);

                return await GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating treasury {TreasuryId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var treasury = await _context.Treasuries.FindAsync(id);
                if (treasury == null || treasury.IsDeleted)
                {
                    return false;
                }

                // Check if treasury has any vouchers
                var hasVouchers = await _context.Vouchers
                    .AnyAsync(v => v.WalletId == id &&
                                  v.WalletType == WalletType.Treasury &&
                                  !v.IsDeleted);

                if (hasVouchers)
                {
                    throw new InvalidOperationException(
                        "لا يمكن حذف الخزينة لوجود عمليات مرتبطة بها. يمكنك تعطيلها بدلاً من ذلك");
                }

                // Soft delete
                treasury.IsDeleted = true;
                treasury.IsActive = false;
                treasury.UpdatedAt = DateTime.UtcNow;

                _context.Update(treasury);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Treasury {TreasuryId} deleted successfully", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting treasury {TreasuryId}", id);
                throw;
            }
        }

        public async Task<TreasuryBalanceViewModel?> GetBalanceAsync(Guid id)
        {
            try
            {
                var treasury = await _context.Treasuries
                    .Include(t => t.Currency)
                    .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

                if (treasury == null)
                {
                    return null;
                }

                var vouchers = await _context.Vouchers
                    .Where(v => v.WalletId == id &&
                               v.WalletType == WalletType.Treasury &&
                               v.Status == VoucherStatus.Posted &&
                               !v.IsDeleted)
                    .ToListAsync();

                var totalIncome = vouchers
                    .Where(v => v.Type == VoucherType.Income)
                    .Sum(v => v.Amount);

                var totalExpense = vouchers
                    .Where(v => v.Type == VoucherType.Expense)
                    .Sum(v => v.Amount);

                var balance = totalIncome - totalExpense;

                var lastTransaction = vouchers
                    .OrderByDescending(v => v.PostedAt)
                    .FirstOrDefault();

                return new TreasuryBalanceViewModel
                {
                    TreasuryId = treasury.Id,
                    TreasuryName = treasury.Name,
                    CurrencyCode = treasury.CurrencyCode,
                    CurrencySymbol = treasury.Currency.Symbol,
                    Balance = balance,
                    TotalIncome = totalIncome,
                    TotalExpense = totalExpense,
                    LastTransactionDate = lastTransaction?.PostedAt ?? treasury.CreatedAt,
                    TransactionCount = vouchers.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving treasury balance for {TreasuryId}", id);
                throw;
            }
        }

        public async Task<bool> TreasuryExistsAsync(Guid id)
        {
            return await _context.Treasuries.AnyAsync(t => t.Id == id && !t.IsDeleted);
        }

        public async Task<bool> IsDuplicateNameAsync(Guid companyId, string name, Guid? excludeId = null)
        {
            var query = _context.Treasuries
                .Where(t => t.CompanyId == companyId &&
                           t.Name == name &&
                           !t.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(t => t.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        #region Private Methods

        private decimal CalculateBalance(Guid treasuryId)
        {
            var vouchers = _context.Vouchers
                .Where(v => v.WalletId == treasuryId &&
                           v.WalletType == WalletType.Treasury &&
                           v.Status == VoucherStatus.Posted &&
                           !v.IsDeleted)
                .ToList();

            var income = vouchers.Where(v => v.Type == VoucherType.Income).Sum(v => v.Amount);
            var expense = vouchers.Where(v => v.Type == VoucherType.Expense).Sum(v => v.Amount);

            return income - expense;
        }

        private TreasuryViewModel MapToViewModel(Treasury treasury)
        {
            return new TreasuryViewModel
            {
                Id = treasury.Id,
                Name = treasury.Name,
                CompanyId = treasury.CompanyId,
                CompanyName = treasury.Company?.Name ?? "",
                CurrencyCode = treasury.CurrencyCode,
                CurrencySymbol = treasury.Currency?.Symbol ?? "",
                Status = treasury.Status,
                Description = treasury.Description,
                JournalAccountId = treasury.JournalAccountId,
                JournalAccountName = treasury.JournalAccount?.Name ?? "",
                Balance = CalculateBalance(treasury.Id),
                CreatedAt = treasury.CreatedAt
            };
        }

        #endregion
    }
}