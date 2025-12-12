namespace ModulerERP_MVC_.Finance.Company.Repositories
{
    public interface ICompanyRepository
    {
        Task<IEnumerable<Models.Finance.Company>> GetAllCompaniesAsync();
        Task<Models.Finance.Company?> GetCompanyByIdAsync(Guid id);
        Task<Models.Finance.Company?> GetCompanyByNameAsync(string name);
        Task<bool> CompanyExistsByNameAsync(string name, Guid? excludeId = null);
        Task<Models.Finance.Company> CreateCompanyAsync(Models.Finance.Company company);
        Task UpdateCompanyAsync(Models.Finance.Company company);
        Task DeleteCompanyAsync(Guid id);
        Task<int> GetCompaniesCountAsync();
    }
}
