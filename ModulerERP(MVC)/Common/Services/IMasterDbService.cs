using ModulerERP_MVC_.Common.Models;

public interface IMasterDbService
{
    Task<MasterCompany?> GetCompanyAsync(Guid companyId);
    Task<MasterCompany?> GetCompanyByNameAsync(string companyName);
    Task<bool> CompanyExistsAsync(Guid companyId);
    Task<MasterCompany> CreateCompanyAsync(string name, string currencyCode = "EGP");
    Task<bool> CreateTenantDatabaseAsync(Guid companyId);
}