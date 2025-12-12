using Microsoft.AspNetCore.Mvc;
using ModulerERP_MVC_.Common.Extensions;
using ModulerERP_MVC_.Finance.Company.Services;
using ModulerERP_MVC_.Finance.Company.ViewModels;
using ModulerERP_MVC_.Finance.Currencies.Services;
using ModulerERP_MVC_.Finance.Currencies.ViewModels;

namespace ModulerERP_MVC_.Finance.Company.Controllers
{
    [Route("Finance/[controller]")]
    public class CompaniesController : Controller
    {
        private readonly ICompanyService _companyService;
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<CompaniesController> _logger;

        public CompaniesController(
            ICompanyService companyService,
            ICurrencyService currencyService,
            ILogger<CompaniesController> logger)
        {
            _companyService = companyService;
            _currencyService = currencyService;
            _logger = logger;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Controller: Getting all companies");

                var response = await _companyService.GetAllCompaniesAsync();

                if (!response.IsSuccess)
                {
                    TempData["ErrorMessage"] = response.Message;
                    return View(new List<CompanyListViewModel>());
                }

                TempData["SuccessMessage"] = response.Message;
                return View(response.Data);
            }
            catch (BusinessLogicException ex)
            {
                _logger.LogWarning(ex, "Controller: Business logic error in Index");
                TempData["ErrorMessage"] = ex.Message;
                return View(new List<CompanyListViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Controller: Unexpected error in Index");
                TempData["ErrorMessage"] = "An unexpected error occurred while loading companies";
                return View(new List<CompanyListViewModel>());
            }
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                _logger.LogInformation("Controller: Getting company details for ID: {CompanyId}", id);

                var response = await _companyService.GetCompanyByIdAsync(id);

                if (!response.IsSuccess)
                {
                    TempData["ErrorMessage"] = response.Message;
                    return RedirectToAction(nameof(Index));
                }

                return View(response.Data);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Controller: Company not found: {CompanyId}", id);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Controller: Error in Details for ID: {CompanyId}", id);
                TempData["ErrorMessage"] = "An unexpected error occurred";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            try
            {
                _logger.LogInformation("Controller: Loading create company form");

                // ✅ استخدم الـ Method الجديد
                var currenciesResponse = await _currencyService.GetAllCurrenciesForViewAsync();
                ViewBag.Currencies = currenciesResponse.Data ?? new List<CurrencyListViewModel>();

                return View(new CreateCompanyViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Controller: Error loading create form");
                TempData["ErrorMessage"] = "Error loading form";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCompanyViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Controller: Invalid model state for create company");

                    var currenciesResponse = await _currencyService.GetAllCurrenciesForViewAsync();
                    ViewBag.Currencies = currenciesResponse.Data ?? new List<CurrencyListViewModel>();

                    return View(model);
                }

                _logger.LogInformation("Controller: Creating company: {CompanyName}", model.Name);

                var response = await _companyService.CreateCompanyAsync(model);

                if (!response.IsSuccess)
                {
                    TempData["ErrorMessage"] = response.Message;

                    var currenciesResponse = await _currencyService.GetAllCurrenciesForViewAsync();
                    ViewBag.Currencies = currenciesResponse.Data ?? new List<CurrencyListViewModel>();

                    return View(model);
                }

                TempData["SuccessMessage"] = response.Message;
                return RedirectToAction(nameof(Details), new { id = response.Data.Id });
            }
            catch (BusinessLogicException ex)
            {
                _logger.LogWarning(ex, "Controller: Business logic error in Create");
                TempData["ErrorMessage"] = ex.Message;

                var currenciesResponse = await _currencyService.GetAllCurrenciesForViewAsync();
                ViewBag.Currencies = currenciesResponse.Data ?? new List<CurrencyListViewModel>();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Controller: Unexpected error in Create");
                TempData["ErrorMessage"] = "An unexpected error occurred while creating the company";

                var currenciesResponse = await _currencyService.GetAllCurrenciesForViewAsync();
                ViewBag.Currencies = currenciesResponse.Data ?? new List<CurrencyListViewModel>();

                return View(model);
            }
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                _logger.LogInformation("Controller: Loading edit form for company: {CompanyId}", id);

                var response = await _companyService.GetCompanyForEditAsync(id);

                if (!response.IsSuccess)
                {
                    TempData["ErrorMessage"] = response.Message;
                    return RedirectToAction(nameof(Index));
                }

                var currenciesResponse = await _currencyService.GetAllCurrenciesForViewAsync();
                ViewBag.Currencies = currenciesResponse.Data ?? new List<CurrencyListViewModel>();

                return View(response.Data);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Controller: Company not found for edit: {CompanyId}", id);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Controller: Error loading edit form for: {CompanyId}", id);
                TempData["ErrorMessage"] = "Error loading edit form";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateCompanyViewModel model)
        {
            try
            {
                if (id != model.Id)
                {
                    _logger.LogWarning("Controller: ID mismatch in Edit");
                    TempData["ErrorMessage"] = "Invalid request";
                    return RedirectToAction(nameof(Index));
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Controller: Invalid model state for edit company");

                    var currenciesResponse = await _currencyService.GetAllCurrenciesForViewAsync();
                    ViewBag.Currencies = currenciesResponse.Data ?? new List<CurrencyListViewModel>();

                    return View(model);
                }

                _logger.LogInformation("Controller: Updating company: {CompanyId}", id);

                var response = await _companyService.UpdateCompanyAsync(id, model);

                if (!response.IsSuccess)
                {
                    TempData["ErrorMessage"] = response.Message;

                    var currenciesResponse = await _currencyService.GetAllCurrenciesForViewAsync();
                    ViewBag.Currencies = currenciesResponse.Data ?? new List<CurrencyListViewModel>();

                    return View(model);
                }

                TempData["SuccessMessage"] = response.Message;
                return RedirectToAction(nameof(Details), new { id = response.Data.Id });
            }
            catch (BusinessLogicException ex)
            {
                _logger.LogWarning(ex, "Controller: Business logic error in Edit");
                TempData["ErrorMessage"] = ex.Message;

                var currenciesResponse = await _currencyService.GetAllCurrenciesForViewAsync();
                ViewBag.Currencies = currenciesResponse.Data ?? new List<CurrencyListViewModel>();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Controller: Unexpected error in Edit for: {CompanyId}", id);
                TempData["ErrorMessage"] = "An unexpected error occurred while updating the company";

                var currenciesResponse = await _currencyService.GetAllCurrenciesForViewAsync();
                ViewBag.Currencies = currenciesResponse.Data ?? new List<CurrencyListViewModel>();

                return View(model);
            }
        }

        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                _logger.LogInformation("Controller: Loading delete confirmation for company: {CompanyId}", id);

                var response = await _companyService.GetCompanyByIdAsync(id);

                if (!response.IsSuccess)
                {
                    TempData["ErrorMessage"] = response.Message;
                    return RedirectToAction(nameof(Index));
                }

                return View(response.Data);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Controller: Company not found for delete: {CompanyId}", id);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Controller: Error loading delete form for: {CompanyId}", id);
                TempData["ErrorMessage"] = "Error loading delete confirmation";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("Delete/{id}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                _logger.LogInformation("Controller: Deleting company: {CompanyId}", id);

                var response = await _companyService.DeleteCompanyAsync(id);

                if (!response.IsSuccess)
                {
                    TempData["ErrorMessage"] = response.Message;
                    return RedirectToAction(nameof(Details), new { id });
                }

                TempData["SuccessMessage"] = response.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (BusinessLogicException ex)
            {
                _logger.LogWarning(ex, "Controller: Business logic error in Delete");
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Controller: Unexpected error in Delete for: {CompanyId}", id);
                TempData["ErrorMessage"] = "An unexpected error occurred while deleting the company";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}