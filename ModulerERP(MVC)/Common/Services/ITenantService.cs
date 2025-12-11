using ModulerERP_MVC_.Common.Models;

public interface ITenantService
{
    string? GetCurrentTenantId();
    Task<bool> ValidateTenantAsync(string tenantId);
    Task<MasterCompany?> GetTenantAsync(string tenantId);
    string GetConnectionString(string tenantId);
    Task<string> GetConnectionStringAsync(string tenantId);
}
