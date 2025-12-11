using Microsoft.Extensions.Caching.Memory;
using ModulerERP_MVC_.Common.Models;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly IMasterDbService _masterDbService;
    private readonly ILogger<TenantService> _logger;
    private readonly string? _tenantId;

    public TenantService(
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        IMemoryCache cache,
        IMasterDbService masterDbService,
        ILogger<TenantService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _cache = cache;
        _masterDbService = masterDbService;
        _logger = logger;
        _tenantId = ResolveTenantId();
    }

    public string? GetCurrentTenantId()
    {
        return _tenantId;
    }

    private string? ResolveTenantId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return null;

        // استراتيجية الحصول على TenantId من الـ Configuration
        var strategy = _configuration["MultiTenant:TenantResolution"] ?? "Header";

        return strategy.ToLower() switch
        {
            "header" => ResolveFromHeader(httpContext),
            "subdomain" => ResolveFromSubdomain(httpContext),
            "claim" => ResolveFromClaim(httpContext),
            "session" => ResolveFromSession(httpContext),
            _ => ResolveFromHeader(httpContext)
        };
    }

    private string? ResolveFromHeader(HttpContext httpContext)
    {
        // للـ API Calls
        if (httpContext.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantHeader))
        {
            return tenantHeader.FirstOrDefault();
        }
        return null;
    }

    private string? ResolveFromSubdomain(HttpContext httpContext)
    {
        var host = httpContext.Request.Host.Host;
        if (!string.IsNullOrEmpty(host) && host.Contains('.'))
        {
            var subdomain = host.Split('.')[0];
            if (subdomain != "www" && subdomain != "localhost")
            {
                return subdomain;
            }
        }
        return null;
    }

    private string? ResolveFromClaim(HttpContext httpContext)
    {
        return httpContext.User?.FindFirst("tenant_id")?.Value;
    }

    private string? ResolveFromSession(HttpContext httpContext)
    {
        // للـ MVC - من الـ Session
        return httpContext.Session.GetString("TenantId");
    }

    public async Task<bool> ValidateTenantAsync(string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
            return false;

        var cacheKey = $"tenant_valid_{tenantId}";
        if (_cache.TryGetValue(cacheKey, out bool isValid))
        {
            return isValid;
        }

        try
        {
            if (Guid.TryParse(tenantId, out var companyId))
            {
                isValid = await _masterDbService.CompanyExistsAsync(companyId);
            }
            else
            {
                var company = await _masterDbService.GetCompanyByNameAsync(tenantId);
                isValid = company != null;
            }

            _cache.Set(cacheKey, isValid, TimeSpan.FromMinutes(5));
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating tenant {TenantId}", tenantId);
            return false;
        }
    }

    public async Task<MasterCompany?> GetTenantAsync(string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
            return null;

        var cacheKey = $"tenant_{tenantId}";
        if (_cache.TryGetValue(cacheKey, out MasterCompany? cachedCompany))
        {
            return cachedCompany;
        }

        try
        {
            MasterCompany? company = null;

            if (Guid.TryParse(tenantId, out var companyId))
            {
                company = await _masterDbService.GetCompanyAsync(companyId);
            }
            else
            {
                company = await _masterDbService.GetCompanyByNameAsync(tenantId);
            }

            if (company != null)
            {
                _cache.Set(cacheKey, company, TimeSpan.FromMinutes(10));
            }

            return company;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant {TenantId}", tenantId);
            return null;
        }
    }

    public string GetConnectionString(string tenantId)
    {
        try
        {
            var cacheKey = $"connectionstring_{tenantId}";
            if (_cache.TryGetValue(cacheKey, out string? cachedConnectionString))
            {
                return cachedConnectionString!;
            }

            var tenant = GetTenantAsync(tenantId).GetAwaiter().GetResult();
            if (tenant?.DatabaseName != null)
            {
                var template = _configuration.GetConnectionString("TenantTemplate")!;
                var connectionString = template.Replace("{DatabaseName}", tenant.DatabaseName);

                _cache.Set(cacheKey, connectionString, TimeSpan.FromHours(1));
                return connectionString;
            }

            return _configuration.GetConnectionString("DefaultConnection")!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting connection string for tenant {TenantId}", tenantId);
            return _configuration.GetConnectionString("DefaultConnection")!;
        }
    }

    public async Task<string> GetConnectionStringAsync(string tenantId)
    {
        try
        {
            var cacheKey = $"connectionstring_{tenantId}";
            if (_cache.TryGetValue(cacheKey, out string? cachedConnectionString))
            {
                return cachedConnectionString!;
            }

            var tenant = await GetTenantAsync(tenantId);
            if (tenant?.DatabaseName != null)
            {
                var template = _configuration.GetConnectionString("TenantTemplate")!;
                var connectionString = template.Replace("{DatabaseName}", tenant.DatabaseName);

                _cache.Set(cacheKey, connectionString, TimeSpan.FromHours(1));
                return connectionString;
            }

            return _configuration.GetConnectionString("DefaultConnection")!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting connection string for tenant {TenantId}", tenantId);
            return _configuration.GetConnectionString("DefaultConnection")!;
        }
    }
}
