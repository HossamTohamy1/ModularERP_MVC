using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModulerERP_MVC_.Common.Enums.Finance_Enum;
using ModulerERP_MVC_.Data;
using ModulerERP_MVC_.Finance.GlAccounts.ViewModels;
using ModulerERP_MVC_.Models.Finance;

namespace ModulerERP_MVC_.Finance.GlAccounts.Services
{
    public class GlAccountService : IGlAccountService
    {
        private readonly ModulesDbContext _context;
        private readonly ILogger<GlAccountService> _logger;

        public GlAccountService(ModulesDbContext context, ILogger<GlAccountService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<GlAccountViewModel>> GetAllAsync(AccountType? type = null, string? searchTerm = null, bool? isLeaf = null)
        {
            try
            {
                _logger.LogInformation("Fetching GL accounts with filters: Type={Type}, Search={SearchTerm}, IsLeaf={IsLeaf}",
                    type, searchTerm, isLeaf);

                var query = _context.GlAccounts
                    .Where(g => !g.IsDeleted && g.IsActive)
                    .AsQueryable();

                if (type.HasValue)
                {
                    query = query.Where(g => g.Type == type.Value);
                }

                if (isLeaf.HasValue)
                {
                    query = query.Where(g => g.IsLeaf == isLeaf.Value);
                }

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(g => g.Code.Contains(searchTerm) || g.Name.Contains(searchTerm));
                }

                var glAccounts = await query
                    .OrderBy(g => g.Code)
                    .Select(g => new GlAccountViewModel
                    {
                        Id = g.Id,
                        Code = g.Code,
                        Name = g.Name,
                        Type = g.Type,
                        IsLeaf = g.IsLeaf,
                        CompanyId = g.CompanyId,
                        CompanyName = g.Company.Name,
                        CreatedAt = g.CreatedAt
                    })
                    .ToListAsync();

                _logger.LogInformation("Successfully retrieved {Count} GL accounts", glAccounts.Count);
                return glAccounts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving GL accounts");
                throw;
            }
        }

        public async Task<GlAccountViewModel?> GetByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Fetching GL account with ID {GlAccountId}", id);

                var glAccount = await _context.GlAccounts
                    .Where(g => g.Id == id && !g.IsDeleted)
                    .Select(g => new GlAccountViewModel
                    {
                        Id = g.Id,
                        Code = g.Code,
                        Name = g.Name,
                        Type = g.Type,
                        IsLeaf = g.IsLeaf,
                        CompanyId = g.CompanyId,
                        CompanyName = g.Company.Name,
                        CreatedAt = g.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                if (glAccount == null)
                {
                    _logger.LogWarning("GL account with ID {GlAccountId} not found", id);
                }

                return glAccount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving GL account {GlAccountId}", id);
                throw;
            }
        }

        public async Task<GlAccountViewModel> CreateAsync(GlAccountViewModel viewModel)
        {
            try
            {
                _logger.LogInformation("Creating new GL account {Code} - {Name}", viewModel.Code, viewModel.Name);

                var companyExists = await _context.Companies
                    .AnyAsync(c => c.Id == viewModel.CompanyId && c.IsActive && !c.IsDeleted);
                if (!companyExists)
                {
                    throw new ArgumentException("Company not found or inactive");
                }

                if (await IsDuplicateCodeAsync(viewModel.CompanyId, viewModel.Code))
                {
                    throw new InvalidOperationException($"A GL account with code '{viewModel.Code}' already exists in this company");
                }

                var glAccount = new GlAccount
                {
                    Id = Guid.NewGuid(),
                    Code = viewModel.Code,
                    Name = viewModel.Name,
                    Type = viewModel.Type,
                    IsLeaf = viewModel.IsLeaf,
                    CompanyId = viewModel.CompanyId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                _context.GlAccounts.Add(glAccount);
                await _context.SaveChangesAsync();

                _logger.LogInformation("GL account {Code} created successfully with ID {GlAccountId}",
                    glAccount.Code, glAccount.Id);

                return (await GetByIdAsync(glAccount.Id))!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating GL account");
                throw;
            }
        }

        public async Task<GlAccountViewModel?> UpdateAsync(Guid id, GlAccountViewModel viewModel)
        {
            try
            {
                _logger.LogInformation("Updating GL account {GlAccountId}", id);

                var glAccount = await _context.GlAccounts.FindAsync(id);
                if (glAccount == null || glAccount.IsDeleted)
                {
                    _logger.LogWarning("GL account {GlAccountId} not found for update", id);
                    return null;
                }

                if (await IsDuplicateCodeAsync(viewModel.CompanyId, viewModel.Code, id))
                {
                    throw new InvalidOperationException($"A GL account with code '{viewModel.Code}' already exists in this company");
                }

                glAccount.Code = viewModel.Code;
                glAccount.Name = viewModel.Name;
                glAccount.Type = viewModel.Type;
                glAccount.IsLeaf = viewModel.IsLeaf;
                glAccount.UpdatedAt = DateTime.UtcNow;

                _context.Update(glAccount);
                await _context.SaveChangesAsync();

                _logger.LogInformation("GL account {GlAccountId} updated successfully", id);

                return await GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating GL account {GlAccountId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting GL account {GlAccountId}", id);

                var glAccount = await _context.GlAccounts.FindAsync(id);
                if (glAccount == null || glAccount.IsDeleted)
                {
                    return false;
                }

                var hasVouchers = await _context.Vouchers
                    .AnyAsync(v => v.CategoryAccountId == id && !v.IsDeleted);

                if (hasVouchers)
                {
                    throw new InvalidOperationException(
                        "Cannot delete GL account with associated transactions. You can deactivate it instead");
                }

                glAccount.IsDeleted = true;
                glAccount.IsActive = false;
                glAccount.UpdatedAt = DateTime.UtcNow;

                _context.Update(glAccount);
                await _context.SaveChangesAsync();

                _logger.LogInformation("GL account {GlAccountId} deleted successfully", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting GL account {GlAccountId}", id);
                throw;
            }
        }

        public async Task<bool> GlAccountExistsAsync(Guid id)
        {
            return await _context.GlAccounts.AnyAsync(g => g.Id == id && !g.IsDeleted);
        }

        public async Task<bool> IsDuplicateCodeAsync(Guid companyId, string code, Guid? excludeId = null)
        {
            var query = _context.GlAccounts
                .Where(g => g.CompanyId == companyId && g.Code == code && !g.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(g => g.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}