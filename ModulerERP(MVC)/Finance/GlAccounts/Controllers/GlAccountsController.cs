using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ModulerERP_MVC_.Common.Enums.Finance_Enum;
using ModulerERP_MVC_.Data;
using ModulerERP_MVC_.Finance.GlAccounts.Services;
using ModulerERP_MVC_.Finance.GlAccounts.ViewModels;

namespace ModulerERP_MVC_.Finance.GlAccounts.Controllers
{
    public class GlAccountsController : Controller
    {
    
        private readonly IGlAccountService _glAccountService;
        private readonly ModulesDbContext _context;
        private readonly ILogger<GlAccountsController> _logger;

        public GlAccountsController(
            IGlAccountService glAccountService,
            ModulesDbContext context,
            ILogger<GlAccountsController> logger)
        {
            _glAccountService = glAccountService;
            _context = context;
            _logger = logger;
        }

        // GET: GlAccounts/Index
        public async Task<IActionResult> Index(AccountType? type, string searchTerm, bool? isLeaf)
        {
            try
            {
                var glAccounts = await _glAccountService.GetAllAsync(type, searchTerm, isLeaf);

                var viewModel = new GlAccountListViewModel
                {
                    GlAccounts = glAccounts,
                    FilterType = type,
                    SearchTerm = searchTerm,
                    IsLeaf = isLeaf
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading GL accounts");
                TempData["Error"] = "An error occurred while loading GL accounts";
                return View(new GlAccountListViewModel());
            }
        }

        // GET: GlAccounts/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var viewModel = await _glAccountService.GetByIdAsync(id.Value);

                if (viewModel == null)
                {
                    return NotFound();
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading GL account details");
                TempData["Error"] = "An error occurred while loading GL account details";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: GlAccounts/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View(new GlAccountViewModel { IsLeaf = true });
        }

        // POST: GlAccounts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GlAccountViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _glAccountService.CreateAsync(viewModel);
                    TempData["Success"] = "GL Account created successfully";
                    return RedirectToAction(nameof(Index));
                }

                await PopulateDropdownsAsync(viewModel.CompanyId);
                return View(viewModel);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("Code", ex.Message);
                await PopulateDropdownsAsync(viewModel.CompanyId);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating GL account");
                TempData["Error"] = "An error occurred while creating the GL account";
                await PopulateDropdownsAsync();
                return View(viewModel);
            }
        }

        // GET: GlAccounts/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var viewModel = await _glAccountService.GetByIdAsync(id.Value);

                if (viewModel == null)
                {
                    return NotFound();
                }

                await PopulateDropdownsAsync(viewModel.CompanyId);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading GL account for edit");
                TempData["Error"] = "An error occurred while loading GL account data";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: GlAccounts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, GlAccountViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _glAccountService.UpdateAsync(id, viewModel);

                    if (result == null)
                    {
                        return NotFound();
                    }

                    TempData["Success"] = "GL Account updated successfully";
                    return RedirectToAction(nameof(Index));
                }

                await PopulateDropdownsAsync(viewModel.CompanyId);
                return View(viewModel);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("Code", ex.Message);
                await PopulateDropdownsAsync(viewModel.CompanyId);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating GL account");
                TempData["Error"] = "An error occurred while updating the GL account";
                await PopulateDropdownsAsync(viewModel.CompanyId);
                return View(viewModel);
            }
        }

        // GET: GlAccounts/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var viewModel = await _glAccountService.GetByIdAsync(id.Value);

                if (viewModel == null)
                {
                    return NotFound();
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading GL account for delete");
                TempData["Error"] = "An error occurred while loading GL account data";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: GlAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var result = await _glAccountService.DeleteAsync(id);

                if (!result)
                {
                    return NotFound();
                }

                TempData["Success"] = "GL Account deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting GL account");
                TempData["Error"] = "An error occurred while deleting the GL account";
                return RedirectToAction(nameof(Index));
            }
        }

        #region Helper Methods

        private async Task PopulateDropdownsAsync(Guid? selectedCompanyId = null)
        {
            ViewBag.Companies = new SelectList(
                await _context.Companies.Where(c => c.IsActive && !c.IsDeleted).ToListAsync(),
                "Id",
                "Name",
                selectedCompanyId);
        }

        #endregion
    }
}