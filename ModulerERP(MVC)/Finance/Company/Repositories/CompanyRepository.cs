using Microsoft.EntityFrameworkCore;
using ModulerERP_MVC_.Common.Repositories.Interfaces;
using ModulerERP_MVC_.Data;

namespace ModulerERP_MVC_.Finance.Company.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly IGeneralRepository<Models.Finance.Company> _generalRepository;
        private readonly ModulesDbContext _context;
        private readonly ILogger<CompanyRepository> _logger;

        public CompanyRepository(
            IGeneralRepository<Models.Finance.Company> generalRepository,
            ModulesDbContext context,
            ILogger<CompanyRepository> logger)
        {
            _generalRepository = generalRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Models.Finance.Company>> GetAllCompaniesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all companies from database");

                return await _generalRepository
                    .GetAll()
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all companies");
                throw;
            }
        }

        public async Task<Models.Finance.Company?> GetCompanyByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Fetching company with ID: {CompanyId}", id);

                return await _generalRepository.GetByID(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching company with ID: {CompanyId}", id);
                throw;
            }
        }

        public async Task<Models.Finance.Company?> GetCompanyByNameAsync(string name)
        {
            try
            {
                _logger.LogInformation("Fetching company with name: {CompanyName}", name);

                return await _generalRepository
                    .Get(c => c.Name == name)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching company with name: {CompanyName}", name);
                throw;
            }
        }

        public async Task<bool> CompanyExistsByNameAsync(string name, Guid? excludeId = null)
        {
            try
            {
                _logger.LogInformation("Checking if company exists with name: {CompanyName}", name);

                if (excludeId.HasValue)
                {
                    return await _generalRepository
                        .AnyAsync(c => c.Name == name && c.Id != excludeId.Value);
                }

                return await _generalRepository
                    .AnyAsync(c => c.Name == name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking company existence with name: {CompanyName}", name);
                throw;
            }
        }

        public async Task<Models.Finance.Company> CreateCompanyAsync(Models.Finance.Company company)
        {
            try
            {
                _logger.LogInformation("Creating new company: {CompanyName}", company.Name);

                company.CreatedAt = DateTime.UtcNow;
                await _generalRepository.AddAsync(company);
                await _generalRepository.SaveChanges();

                _logger.LogInformation("Company created successfully with ID: {CompanyId}", company.Id);

                return company;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating company: {CompanyName}", company.Name);
                throw;
            }
        }

        public async Task UpdateCompanyAsync(Models.Finance.Company company)
        {
            try
            {
                _logger.LogInformation("Updating company with ID: {CompanyId}", company.Id);

                await _generalRepository.Update(company);

                _logger.LogInformation("Company updated successfully: {CompanyId}", company.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating company with ID: {CompanyId}", company.Id);
                throw;
            }
        }

        public async Task DeleteCompanyAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting company with ID: {CompanyId}", id);

                await _generalRepository.Delete(id);

                _logger.LogInformation("Company deleted successfully: {CompanyId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting company with ID: {CompanyId}", id);
                throw;
            }
        }

        public async Task<int> GetCompaniesCountAsync()
        {
            try
            {
                _logger.LogInformation("Getting total companies count");

                return await _generalRepository.GetAll().CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting companies count");
                throw;
            }
        }
    }
}
