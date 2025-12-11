using Microsoft.AspNetCore.Mvc;
using ModulerERP_MVC_.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace ModulerERP_MVC_.Controllers
{
    public class TenantController : Controller
    {
        private readonly IMasterDbService _masterDbService;
        private readonly ITenantService _tenantService;
        private readonly MasterDbContext _masterDbContext;
        private readonly ILogger<TenantController> _logger;

        public TenantController(
            IMasterDbService masterDbService,
            ITenantService tenantService,
            MasterDbContext masterDbContext,
            ILogger<TenantController> logger)
        {
            _masterDbService = masterDbService;
            _tenantService = tenantService;
            _masterDbContext = masterDbContext;
            _logger = logger;
        }

        // ============================================
        // صفحة اختيار الشركة (Tenant)
        // ============================================
        [HttpGet]
        public async Task<IActionResult> Select()
        {
            var companies = await _masterDbContext.MasterCompanies
                .Where(c => c.Status == ModulerERP_MVC_.Common.Enums.Finance_Enum.CompanyStatus.Active)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(companies);
        }

        // ============================================
        // اختيار Tenant وحفظه في الـ Session
        // ============================================
        [HttpPost]
        public async Task<IActionResult> SetTenant(string tenantId)
        {
            if (string.IsNullOrEmpty(tenantId))
            {
                TempData["Error"] = "Please select a company";
                return RedirectToAction(nameof(Select));
            }

            // Validate Tenant
            var isValid = await _tenantService.ValidateTenantAsync(tenantId);
            if (!isValid)
            {
                TempData["Error"] = "Invalid company selected";
                return RedirectToAction(nameof(Select));
            }

            // حفظ TenantId في الـ Session
            HttpContext.Session.SetString("TenantId", tenantId);

            // Get Company Info
            var company = await _tenantService.GetTenantAsync(tenantId);
            if (company != null)
            {
                HttpContext.Session.SetString("CompanyName", company.Name);
                HttpContext.Session.SetString("CurrencyCode", company.CurrencyCode);
            }

            _logger.LogInformation("User selected tenant: {TenantId}", tenantId);

            TempData["Success"] = $"Company switched to: {company?.Name}";
            return RedirectToAction("Index", "Home");
        }

        // ============================================
        // Switch Tenant (تغيير الشركة)
        // ============================================
        [HttpGet]
        public IActionResult Switch()
        {
            HttpContext.Session.Remove("TenantId");
            HttpContext.Session.Remove("CompanyName");
            HttpContext.Session.Remove("CurrencyCode");

            return RedirectToAction(nameof(Select));
        }

        // ============================================
        // عرض معلومات الـ Tenant الحالي
        // ============================================
        [HttpGet]
        public async Task<IActionResult> Current()
        {
            var tenantId = _tenantService.GetCurrentTenantId();

            if (string.IsNullOrEmpty(tenantId))
            {
                return Json(new { error = "No tenant selected" });
            }

            var tenant = await _tenantService.GetTenantAsync(tenantId);

            return Json(new
            {
                tenantId = tenant?.Id,
                name = tenant?.Name,
                currencyCode = tenant?.CurrencyCode,
                databaseName = tenant?.DatabaseName,
                status = tenant?.Status.ToString()
            });
        }

        // ============================================
        // إنشاء Tenant جديد
        // ============================================
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTenantViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var company = await _masterDbService.CreateCompanyAsync(model.Name, model.CurrencyCode);

                TempData["Success"] = $"Company '{company.Name}' created successfully!";
                return RedirectToAction(nameof(Select));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create tenant: {TenantName}", model.Name);
                ModelState.AddModelError("", "Failed to create company. Please try again.");
                return View(model);
            }
        }
    }

    // ============================================
    // View Model
    // ============================================
    public class CreateTenantViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = "EGP";
    }
}