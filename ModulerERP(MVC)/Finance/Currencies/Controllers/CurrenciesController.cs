using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModulerERP_MVC_.Data;
using ModulerERP_MVC_.Finance.Currencies.Services;
using ModulerERP_MVC_.Models.Finance;
using ModulerERP_MVC_.Models.Finance.DTOs;

namespace ModulerERP_MVC_.Finance.Currencies.Controllers
{
    public class CurrenciesController : Controller
    {
        private readonly ICurrencyService _service;
        private readonly ILogger<CurrenciesController> _logger;
        private readonly ModulesDbContext _context;

        public CurrenciesController(
            ICurrencyService service,
            ILogger<CurrenciesController> logger,
            ModulesDbContext context)
        {
            _service = service;
            _logger = logger;
            _context = context;
        }

        // ⭐ NEW: Hard Delete All & Reset
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetCurrencies()
        {
            try
            {
                // 1️⃣ امسح كل العملات (Hard Delete)
                var allCurrencies = await _context.Currencies
                    .IgnoreQueryFilters()
                    .ToListAsync();

                _context.Currencies.RemoveRange(allCurrencies);
                await _context.SaveChangesAsync();

                // 2️⃣ أضف عملات جديدة
                var currencies = new List<Currency>
                {
                    new Currency
                    {
                        Code = "USD",
                        Name = "US Dollar",
                        Symbol = "$",
                        Decimals = 2,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Currency
                    {
                        Code = "EGP",
                        Name = "Egyptian Pound",
                        Symbol = "E£",
                        Decimals = 2,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Currency
                    {
                        Code = "EUR",
                        Name = "Euro",
                        Symbol = "€",
                        Decimals = 2,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Currency
                    {
                        Code = "GBP",
                        Name = "British Pound",
                        Symbol = "£",
                        Decimals = 2,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Currency
                    {
                        Code = "SAR",
                        Name = "Saudi Riyal",
                        Symbol = "SR",
                        Decimals = 2,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await _context.Currencies.AddRangeAsync(currencies);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Currencies reset successfully! Added {currencies.Count} currencies.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting currencies");
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // 🔍 Diagnostic endpoint
        [HttpGet]


        // GET: /Currencies
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _service.GetAllCurrenciesAsync();

                // Debug info
                var count = response.Data?.Count() ?? 0;
                ViewBag.CurrencyCount = count;
                _logger.LogInformation($"Loaded {count} currencies");

                return View(response.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading currencies index");
                TempData["ErrorMessage"] = "An error occurred while loading currencies";
                return View(new List<CurrencyDto>());
            }
        }

        // GET: /Currencies/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateCurrencyDto());
        }

        // POST: /Currencies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCurrencyDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                dto.Code = dto.Code.ToUpperInvariant().Trim();
                var response = await _service.CreateCurrencyAsync(dto);
                TempData["SuccessMessage"] = response.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating currency");
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // GET: /Currencies/Edit/{code}
        [HttpGet]
        public async Task<IActionResult> Edit(string code)
        {
            try
            {
                var response = await _service.GetCurrencyByCodeAsync(code);

                var updateDto = new UpdateCurrencyDto
                {
                    Code = response.Data.Code,
                    Name = response.Data.Name,
                    Symbol = response.Data.Symbol,
                    Decimals = response.Data.Decimals,
                    IsActive = response.Data.IsActive
                };

                return View(updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading currency for edit: {Code}", code);
                TempData["ErrorMessage"] = "Currency not found";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Currencies/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateCurrencyDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                var response = await _service.UpdateCurrencyAsync(dto);
                TempData["SuccessMessage"] = response.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating currency: {Code}", dto.Code);
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // GET: /Currencies/Details/{code}
        [HttpGet]
        public async Task<IActionResult> Details(string code)
        {
            try
            {
                var response = await _service.GetCurrencyByCodeAsync(code);
                return View(response.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading currency details: {Code}", code);
                TempData["ErrorMessage"] = "Currency not found";
                return RedirectToAction(nameof(Index));
            }
        }

        // ⭐ FIXED: Delete method - استقبل الـ code بشكل صحيح
        [HttpPost("Currencies/Delete/{code}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string code)
        {
            try
            {
                // ⭐ Debug: اطبع الـ code
                _logger.LogInformation($"Attempting to delete currency: '{code}'");

                if (string.IsNullOrWhiteSpace(code))
                {
                    TempData["ErrorMessage"] = "Invalid currency code";
                    return RedirectToAction(nameof(Index));
                }

                var response = await _service.DeleteCurrencyAsync(code);
                TempData["SuccessMessage"] = response.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting currency: {Code}", code);
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}