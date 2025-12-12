using ModulerERP_MVC_.Common.Enums.Finance_Enum;
using ModulerERP_MVC_.Finance.GlAccounts.ViewModels;

namespace ModulerERP_MVC_.Finance.GlAccounts.Services
{
    public interface  IGlAccountService
    {
        Task<IEnumerable<GlAccountViewModel>> GetAllAsync(AccountType? type = null, string? searchTerm = null, bool? isLeaf = null);
        Task<GlAccountViewModel?> GetByIdAsync(Guid id);
        Task<GlAccountViewModel> CreateAsync(GlAccountViewModel viewModel);
        Task<GlAccountViewModel?> UpdateAsync(Guid id, GlAccountViewModel viewModel);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> GlAccountExistsAsync(Guid id);
        Task<bool> IsDuplicateCodeAsync(Guid companyId, string code, Guid? excludeId = null);
    }
}