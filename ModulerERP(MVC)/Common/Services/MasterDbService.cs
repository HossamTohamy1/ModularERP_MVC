using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ModulerERP_MVC_.Common.Data;
using ModulerERP_MVC_.Common.Enums.Finance_Enum;
using ModulerERP_MVC_.Common.Models;
using ModulerERP_MVC_.Data;

public class MasterDbService : IMasterDbService
{
    private readonly MasterDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MasterDbService> _logger;

    public MasterDbService(
        MasterDbContext context,
        IConfiguration configuration,
        ILogger<MasterDbService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<MasterCompany?> GetCompanyAsync(Guid companyId)
    {
        return await _context.MasterCompanies
            .FirstOrDefaultAsync(c => c.Id == companyId && c.Status == CompanyStatus.Active);
    }

    public async Task<MasterCompany?> GetCompanyByNameAsync(string companyName)
    {
        return await _context.MasterCompanies
            .FirstOrDefaultAsync(c => c.Name == companyName && c.Status == CompanyStatus.Active);
    }

    public async Task<bool> CompanyExistsAsync(Guid companyId)
    {
        return await _context.MasterCompanies
            .AnyAsync(c => c.Id == companyId && c.Status == CompanyStatus.Active);
    }

    public async Task<MasterCompany> CreateCompanyAsync(string name, string currencyCode = "EGP")
    {
        var company = new MasterCompany
        {
            Id = Guid.NewGuid(),
            Name = name,
            CurrencyCode = currencyCode,
            DatabaseName = $"ModularERP_Tenant_{Guid.NewGuid():N}",
            Status = CompanyStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _context.MasterCompanies.Add(company);
        await _context.SaveChangesAsync();

        // إنشاء قاعدة البيانات الخاصة بالـ Tenant
        await CreateTenantDatabaseAsync(company.Id);

        return company;
    }

    public async Task<bool> CreateTenantDatabaseAsync(Guid companyId)
    {
        try
        {
            var company = await GetCompanyAsync(companyId);
            if (company == null)
            {
                _logger.LogWarning("Company not found: {CompanyId}", companyId);
                return false;
            }

            var databaseName = company.DatabaseName!;
            var serverConnectionString = GetServerConnectionString();

            // التحقق من وجود قاعدة البيانات
            using (var serverConnection = new SqlConnection(serverConnectionString))
            {
                await serverConnection.OpenAsync();

                var checkDbCommand = new SqlCommand($@"
                        IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{databaseName}')
                            SELECT 1
                        ELSE
                            SELECT 0", serverConnection);

                var dbExists = (int)await checkDbCommand.ExecuteScalarAsync()! == 1;

                if (dbExists)
                {
                    _logger.LogInformation("Database {DatabaseName} already exists, skipping creation", databaseName);
                    return true;
                }

                // إنشاء قاعدة البيانات
                var createDbCommand = new SqlCommand($@"
                        CREATE DATABASE [{databaseName}]", serverConnection);

                await createDbCommand.ExecuteNonQueryAsync();
                _logger.LogInformation("Created database: {DatabaseName}", databaseName);
            }

            // تطبيق الـ Migrations
            var connectionString = GetTenantConnectionString(databaseName);
            var optionsBuilder = new DbContextOptionsBuilder<ModulesDbContext>();
            optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly("ModulerERP(MVC)");
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });

            using (var context = new ModulesDbContext(optionsBuilder.Options))
            {
                var canConnect = await context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    throw new InvalidOperationException($"Cannot connect to database {databaseName}");
                }

                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    _logger.LogInformation("Applying {Count} migrations to {DatabaseName}",
                        pendingMigrations.Count(), databaseName);
                    await context.Database.MigrateAsync();
                }
            }

            _logger.LogInformation("Successfully created and configured tenant database: {DatabaseName}", databaseName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create tenant database for company {CompanyId}", companyId);
            return false;
        }
    }

    private string GetServerConnectionString()
    {
        var template = _configuration.GetConnectionString("TenantTemplate")!;
        // إزالة Initial Catalog للاتصال بالـ Server فقط
        return template.Replace("Initial Catalog={DatabaseName};", "")
                      .Replace("Database={DatabaseName};", "");
    }

    private string GetTenantConnectionString(string databaseName)
    {
        var template = _configuration.GetConnectionString("TenantTemplate")!;
        return template.Replace("{DatabaseName}", databaseName);
    }
}
