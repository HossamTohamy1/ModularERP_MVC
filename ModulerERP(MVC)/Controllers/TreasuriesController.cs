// Areas/Finance/Controllers/TreasuriesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ModulerERP_MVC_.Modules.Data;
using ModulerERP_MVC_.Areas.Finance.Treasuries.Services;
using ModulerERP_MVC_.Areas.Finance.Treasuries.ViewModels;
using ModulerERP_MVC_.Common.Enums.Finance_Enum;

namespace ModulerERP_MVC_.Areas.Finance.Controllers
{
    [Area("Finance")]
    public class TreasuriesController : Controller
    {
        private readonly ITreasuryService _treasuryService;
        private readonly ModulesDbContext _context;
        private readonly ILogger<TreasuriesController> _logger;

        public TreasuriesController(
            ITreasuryService treasuryService,
            ModulesDbContext context,
            ILogger<TreasuriesController> logger)
        {
            _treasuryService = treasuryService;
            _context = context;
            _logger = logger;
        }

        // GET: Finance/Treasuries
        public async Task<IActionResult> Index(TreasuryStatus? status, string searchTerm)
        {
            try
            {
                var treasuries = await _treasuryService.GetAllAsync(status, searchTerm);

                var viewModel = new TreasuryListViewModel
                {
                    Treasuries = treasuries,
                    FilterStatus = status,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading treasuries");
                TempData["Error"] = "حدث خطأ أثناء تحميل الخزائن";
                return View(new TreasuryListViewModel());
            }
        }

        // GET: Finance/Treasuries/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var viewModel = await _treasuryService.GetByIdAsync(id.Value);

                if (viewModel == null)
                {
                    return NotFound();
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading treasury details");
                TempData["Error"] = "حدث خطأ أثناء تحميل تفاصيل الخزينة";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Finance/Treasuries/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View(new TreasuryViewModel { Status = TreasuryStatus.Active });
        }

        // POST: Finance/Treasuries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TreasuryViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _treasuryService.CreateAsync(viewModel);
                    TempData["Success"] = "تم إنشاء الخزينة بنجاح";
                    return RedirectToAction(nameof(Index));
                }

                await PopulateDropdownsAsync(viewModel.CompanyId, viewModel.CurrencyCode);
                return View(viewModel);
            }
            catch (InvalidOperationException ex)
            {
                // Duplicate name error
                ModelState.AddModelError("Name", ex.Message);
                await PopulateDropdownsAsync(viewModel.CompanyId, viewModel.CurrencyCode);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating treasury");
                TempData["Error"] = "حدث خطأ أثناء إنشاء الخزينة";
                await PopulateDropdownsAsync();
                return View(viewModel);
            }
        }

        // GET: Finance/Treasuries/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var viewModel = await _treasuryService.GetByIdAsync(id.Value);

                if (viewModel == null)
                {
                    return NotFound();
                }

                await PopulateDropdownsAsync(viewModel.CompanyId, viewModel.CurrencyCode, viewModel.JournalAccountId);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading treasury for edit");
                TempData["Error"] = "حدث خطأ أثناء تحميل بيانات الخزينة";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Finance/Treasuries/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, TreasuryViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _treasuryService.UpdateAsync(id, viewModel);

                    if (result == null)
                    {
                        return NotFound();
                    }

                    TempData["Success"] = "تم تحديث الخزينة بنجاح";
                    return RedirectToAction(nameof(Index));
                }

                await PopulateDropdownsAsync(viewModel.CompanyId, viewModel.CurrencyCode, viewModel.JournalAccountId);
                return View(viewModel);
            }
            catch (InvalidOperationException ex)
            {
                // Duplicate name error
                ModelState.AddModelError("Name", ex.Message);
                await PopulateDropdownsAsync(viewModel.CompanyId, viewModel.CurrencyCode, viewModel.JournalAccountId);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating treasury");
                TempData["Error"] = "حدث خطأ أثناء تحديث الخزينة";
                await PopulateDropdownsAsync(viewModel.CompanyId, viewModel.CurrencyCode);
                return View(viewModel);
            }
        }

        // GET: Finance/Treasuries/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var viewModel = await _treasuryService.GetByIdAsync(id.Value);

                if (viewModel == null)
                {
                    return NotFound();
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading treasury for delete");
                TempData["Error"] = "حدث خطأ أثناء تحميل بيانات الخزينة";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Finance/Treasuries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var result = await _treasuryService.DeleteAsync(id);

                if (!result)
                {
                    return NotFound();
                }

                TempData["Success"] = "تم حذف الخزينة بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                // Has vouchers error
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting treasury");
                TempData["Error"] = "حدث خطأ أثناء حذف الخزينة";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Finance/Treasuries/Balance/5
        public async Task<IActionResult> Balance(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var viewModel = await _treasuryService.GetBalanceAsync(id.Value);

                if (viewModel == null)
                {
                    return NotFound();
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading treasury balance");
                TempData["Error"] = "حدث خطأ أثناء تحميل رصيد الخزينة";
                return RedirectToAction(nameof(Index));
            }
        }

        #region Helper Methods

        private async Task PopulateDropdownsAsync(Guid? selectedCompanyId = null, string selectedCurrencyCode = null, Guid? selectedAccountId = null)
        {
            ViewBag.Companies = new SelectList(
                await _context.Companies.Where(c => c.IsActive && !c.IsDeleted).ToListAsync(),
                "Id",
                "Name",
                selectedCompanyId);

            ViewBag.Currencies = new SelectList(
                await _context.Currencies.Where(c => c.IsActive && !c.IsDeleted).ToListAsync(),
                "Code",
                "Name",
                selectedCurrencyCode);

            ViewBag.GlAccounts = new SelectList(
                await _context.GlAccounts
                    .Where(g => g.Type == AccountType.Asset && g.IsLeaf && g.IsActive && !g.IsDeleted)
                    .ToListAsync(),
                "Id",
                "Name",
                selectedAccountId);
        }

        #endregion
    }
}