using ModulerERP_MVC_.Common.ViewModel;
using ModulerERP_MVC_.Finance.Company.ViewModels;

namespace ModulerERP_MVC_.Finance.Company.Services
{
    public interface ICompanyService
    {
        Task<ResponseViewModel<IEnumerable<CompanyListViewModel>>> GetAllCompaniesAsync();
        Task<ResponseViewModel<CompanyDetailsViewModel>> GetCompanyByIdAsync(Guid id);
        Task<ResponseViewModel<CompanyDetailsViewModel>> CreateCompanyAsync(CreateCompanyViewModel model);
        Task<ResponseViewModel<CompanyDetailsViewModel>> UpdateCompanyAsync(Guid id, UpdateCompanyViewModel model);
        Task<ResponseViewModel<bool>> DeleteCompanyAsync(Guid id);
        Task<ResponseViewModel<UpdateCompanyViewModel>> GetCompanyForEditAsync(Guid id);
    }
}
