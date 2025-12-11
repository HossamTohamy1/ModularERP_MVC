// Modules/Finance/Services/ITreasuryService.cs
using ModularERP.Common.Enum.Finance_Enum;
using ModulerERP_MVC_.Areas.Finance.ViewModels;

namespace ModulerERP_MVC_.Areas.Finance.Services
{
    public interface ITreasuryService
    {
        Task<IEnumerable<TreasuryViewModel>> GetAllAsync(TreasuryStatus? status = null, string? searchTerm = null);
        Task<TreasuryViewModel?> GetByIdAsync(Guid id);
        Task<TreasuryViewModel> CreateAsync(TreasuryViewModel viewModel);
        Task<TreasuryViewModel?> UpdateAsync(Guid id, TreasuryViewModel viewModel);
        Task<bool> DeleteAsync(Guid id);
        Task<TreasuryBalanceViewModel?> GetBalanceAsync(Guid id);
        Task<bool> TreasuryExistsAsync(Guid id);
        Task<bool> IsDuplicateNameAsync(Guid companyId, string name, Guid? excludeId = null);
    }
}