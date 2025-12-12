using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using ModulerERP_MVC_.Common.Enums.Finance_Enum;
using ModulerERP_MVC_.Common.Extensions;
using ModulerERP_MVC_.Common.Repositories.Interfaces;
using ModulerERP_MVC_.Common.ViewModel;
using ModulerERP_MVC_.Finance.Company.Repositories;
using ModulerERP_MVC_.Finance.Company.ViewModels;
using ModulerERP_MVC_.Finance.Currencies.Repositories;

namespace ModulerERP_MVC_.Finance.Company.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IGeneralRepository<Models.Finance.Company> _generalRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CompanyService> _logger;

        public CompanyService(
            ICompanyRepository companyRepository,
            ICurrencyRepository currencyRepository,
            IGeneralRepository<Models.Finance.Company> generalRepository,
            IMapper mapper,
            ILogger<CompanyService> logger)
        {
            _companyRepository = companyRepository;
            _currencyRepository = currencyRepository;
            _generalRepository = generalRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<IEnumerable<CompanyListViewModel>>> GetAllCompaniesAsync()
        {
            try
            {
                _logger.LogInformation("Service: Getting all companies");

                var companies = await _generalRepository
                    .GetAll()
                    .ProjectTo<CompanyListViewModel>(_mapper.ConfigurationProvider)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation("Service: Successfully retrieved {Count} companies", companies.Count);

                return ResponseViewModel<IEnumerable<CompanyListViewModel>>.Success(
                    companies,
                    $"Retrieved {companies.Count} companies successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error occurred while getting all companies");
                throw new BusinessLogicException(
                    "Failed to retrieve companies",
                    "Finance",
                    FinanceErrorCode.DatabaseError);
            }
        }

        public async Task<ResponseViewModel<CompanyDetailsViewModel>> GetCompanyByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Service: Getting company with ID: {CompanyId}", id);

                var company = await _generalRepository
                    .Get(c => c.Id == id)
                    .ProjectTo<CompanyDetailsViewModel>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();

                if (company == null)
                {
                    _logger.LogWarning("Service: Company not found with ID: {CompanyId}", id);
                    throw new NotFoundException(
                        $"Company with ID {id} not found",
                        FinanceErrorCode.CompanyNotFound);
                }

                _logger.LogInformation("Service: Successfully retrieved company: {CompanyId}", id);

                return ResponseViewModel<CompanyDetailsViewModel>.Success(
                    company,
                    "Company retrieved successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error occurred while getting company: {CompanyId}", id);
                throw new BusinessLogicException(
                    "Failed to retrieve company",
                    "Finance",
                    FinanceErrorCode.DatabaseError);
            }
        }

        public async Task<ResponseViewModel<CompanyDetailsViewModel>> CreateCompanyAsync(CreateCompanyViewModel model)
        {
            try
            {
                _logger.LogInformation("Service: Creating new company: {CompanyName}", model.Name);

                // ✅ Normalize currency code - إزالة المسافات وتحويل لـ uppercase
                model.CurrencyCode = model.CurrencyCode?.Trim().ToUpperInvariant() ?? string.Empty;

                // ✅ Log للتأكد من القيمة
                _logger.LogInformation("Service: Normalized CurrencyCode: '{CurrencyCode}' (Length: {Length})",
                    model.CurrencyCode, model.CurrencyCode.Length);

                // ✅ Validate currency code format
                if (string.IsNullOrWhiteSpace(model.CurrencyCode) || model.CurrencyCode.Length != 3)
                {
                    _logger.LogWarning("Service: Invalid currency code format: {CurrencyCode}", model.CurrencyCode);
                    throw new BusinessLogicException(
                        "Currency code must be exactly 3 characters",
                        "Finance",
                        FinanceErrorCode.InvalidCurrencyCode);
                }

                // Validate company name uniqueness
                if (await _companyRepository.CompanyExistsByNameAsync(model.Name))
                {
                    _logger.LogWarning("Service: Company already exists with name: {CompanyName}", model.Name);
                    throw new BusinessLogicException(
                        $"Company with name '{model.Name}' already exists",
                        "Finance",
                        FinanceErrorCode.CompanyAlreadyExists);
                }

                // Validate currency exists
                var currencyExists = await _currencyRepository.CurrencyExistsByCodeAsync(model.CurrencyCode);
                if (!currencyExists)
                {
                    _logger.LogWarning("Service: Currency not found: {CurrencyCode}", model.CurrencyCode);
                    throw new BusinessLogicException(
                        $"Currency with code '{model.CurrencyCode}' not found",
                        "Finance",
                        FinanceErrorCode.CurrencyNotFound);
                }

                // Map and create company
                var company = _mapper.Map<Models.Finance.Company>(model);
                company.Id = Guid.NewGuid();

                var createdCompany = await _companyRepository.CreateCompanyAsync(company);

                // Get full details with projection
                var companyDetails = await _generalRepository
                    .Get(c => c.Id == createdCompany.Id)
                    .ProjectTo<CompanyDetailsViewModel>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();

                _logger.LogInformation("Service: Company created successfully: {CompanyId}", createdCompany.Id);

                return ResponseViewModel<CompanyDetailsViewModel>.Success(
                    companyDetails!,
                    "Company created successfully");
            }
            catch (BusinessLogicException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error occurred while creating company: {CompanyName}", model.Name);
                throw new BusinessLogicException(
                    "Failed to create company",
                    "Finance",
                    FinanceErrorCode.DatabaseError);
            }
        }

        public async Task<ResponseViewModel<CompanyDetailsViewModel>> UpdateCompanyAsync(Guid id, UpdateCompanyViewModel model)
        {
            try
            {
                _logger.LogInformation("Service: Updating company: {CompanyId}", id);

                // ✅ Normalize currency code
                model.CurrencyCode = model.CurrencyCode?.Trim().ToUpperInvariant() ?? string.Empty;

                // ✅ Validate currency code format
                if (string.IsNullOrWhiteSpace(model.CurrencyCode) || model.CurrencyCode.Length != 3)
                {
                    _logger.LogWarning("Service: Invalid currency code format: {CurrencyCode}", model.CurrencyCode);
                    throw new BusinessLogicException(
                        "Currency code must be exactly 3 characters",
                        "Finance",
                        FinanceErrorCode.InvalidCurrencyCode);
                }

                // Check if company exists
                var existingCompany = await _companyRepository.GetCompanyByIdAsync(id);
                if (existingCompany == null)
                {
                    _logger.LogWarning("Service: Company not found: {CompanyId}", id);
                    throw new NotFoundException(
                        $"Company with ID {id} not found",
                        FinanceErrorCode.CompanyNotFound);
                }

                // Validate name uniqueness (excluding current company)
                if (await _companyRepository.CompanyExistsByNameAsync(model.Name, id))
                {
                    _logger.LogWarning("Service: Company already exists with name: {CompanyName}", model.Name);
                    throw new BusinessLogicException(
                        $"Another company with name '{model.Name}' already exists",
                        "Finance",
                        FinanceErrorCode.CompanyAlreadyExists);
                }

                // Validate currency exists
                var currencyExists = await _currencyRepository.CurrencyExistsByCodeAsync(model.CurrencyCode);
                if (!currencyExists)
                {
                    _logger.LogWarning("Service: Currency not found: {CurrencyCode}", model.CurrencyCode);
                    throw new BusinessLogicException(
                        $"Currency with code '{model.CurrencyCode}' not found",
                        "Finance",
                        FinanceErrorCode.CurrencyNotFound);
                }

                // Update company
                existingCompany.Name = model.Name;
                existingCompany.CurrencyCode = model.CurrencyCode;

                await _companyRepository.UpdateCompanyAsync(existingCompany);

                // Get updated details with projection
                var updatedCompany = await _generalRepository
                    .Get(c => c.Id == id)
                    .ProjectTo<CompanyDetailsViewModel>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();

                _logger.LogInformation("Service: Company updated successfully: {CompanyId}", id);

                return ResponseViewModel<CompanyDetailsViewModel>.Success(
                    updatedCompany!,
                    "Company updated successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (BusinessLogicException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error occurred while updating company: {CompanyId}", id);
                throw new BusinessLogicException(
                    "Failed to update company",
                    "Finance",
                    FinanceErrorCode.DatabaseError);
            }
        }

        public async Task<ResponseViewModel<bool>> DeleteCompanyAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Service: Deleting company: {CompanyId}", id);

                var company = await _companyRepository.GetCompanyByIdAsync(id);
                if (company == null)
                {
                    _logger.LogWarning("Service: Company not found: {CompanyId}", id);
                    throw new NotFoundException(
                        $"Company with ID {id} not found",
                        FinanceErrorCode.CompanyNotFound);
                }

                // Check if company has related data
                if (company.Treasuries.Any() || company.BankAccounts.Any() || company.Vouchers.Any())
                {
                    _logger.LogWarning("Service: Cannot delete company with related data: {CompanyId}", id);
                    throw new BusinessLogicException(
                        "Cannot delete company with existing treasuries, bank accounts, or vouchers",
                        "Finance",
                        FinanceErrorCode.CompanyHasRelatedData);
                }

                await _companyRepository.DeleteCompanyAsync(id);

                _logger.LogInformation("Service: Company deleted successfully: {CompanyId}", id);

                return ResponseViewModel<bool>.Success(
                    true,
                    "Company deleted successfully");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (BusinessLogicException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error occurred while deleting company: {CompanyId}", id);
                throw new BusinessLogicException(
                    "Failed to delete company",
                    "Finance",
                    FinanceErrorCode.DatabaseError);
            }
        }

        public async Task<ResponseViewModel<UpdateCompanyViewModel>> GetCompanyForEditAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Service: Getting company for edit: {CompanyId}", id);

                var company = await _companyRepository.GetCompanyByIdAsync(id);
                if (company == null)
                {
                    _logger.LogWarning("Service: Company not found: {CompanyId}", id);
                    throw new NotFoundException(
                        $"Company with ID {id} not found",
                        FinanceErrorCode.CompanyNotFound);
                }

                var viewModel = _mapper.Map<UpdateCompanyViewModel>(company);

                _logger.LogInformation("Service: Successfully retrieved company for edit: {CompanyId}", id);

                return ResponseViewModel<UpdateCompanyViewModel>.Success(
                    viewModel,
                    "Company retrieved for editing");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error occurred while getting company for edit: {CompanyId}", id);
                throw new BusinessLogicException(
                    "Failed to retrieve company for editing",
                    "Finance",
                    FinanceErrorCode.DatabaseError);
            }
        }
    }
}