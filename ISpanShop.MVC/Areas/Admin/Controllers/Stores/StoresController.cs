using ISpanShop.Models.DTOs.Stores;
using ISpanShop.MVC.Areas.Admin.Models.Stores;
using ISpanShop.Services.Stores;
using Microsoft.AspNetCore.Mvc;
using ISpanShop.MVC.Areas.Admin.Controllers;
using ISpanShop.MVC.Middleware;
using System.Linq;

namespace ISpanShop.MVC.Areas.Admin.Controllers.Stores
{
    [RequirePermission("store_manage")]
    public class StoresController : AdminBaseController
    {
        private readonly IStoreService _storeService;

        public StoresController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        // GET: Admin/Stores
        public IActionResult Index(string? keyword, string verifyStatus = "all", string blockStatus = "all",
                                 int? storeStatusFilter = null,
                                 string sortColumn = "CreatedAt", string sortDirection = "desc",
                                 int page = 1, int pageSize = 20)
        {
            var stores = _storeService.GetAllStores(
                keyword, verifyStatus, blockStatus, storeStatusFilter, sortColumn, sortDirection, page, pageSize, out int totalCount).ToList();
var stats = _storeService.GetStoreStats();

            var vm = new StoreIndexVm
            {
                Stores = stores.ToList(),
                Keyword = keyword,
                VerifyStatus = verifyStatus,
                BlockStatus = blockStatus,
                StoreStatusFilter = storeStatusFilter,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
                TotalCount = totalCount,
                CurrentPage = page,
                PageSize = pageSize,
                VerifiedCount = stats.Verified,
                PendingCount = stats.Pending,
                RejectedCount = stats.Rejected,
                BlockedCount = stats.Blocked,
                Message = TempData["Message"]?.ToString()
            };
            return View(vm);
        }

        // GET: Admin/Stores/Edit/5 (AJAX Modal 用)
        public IActionResult Edit(int storeId)
        {
            var store = _storeService.GetStoreById(storeId);
            if (store == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var vm = new StoreDetailVm
            {
                Store = store
            };

            return PartialView("_EditPartial", vm);
        }

        // POST: Admin/Stores/ToggleVerified
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleVerified(int storeId, bool isVerified)
        {
            var result = _storeService.ToggleVerified(storeId, isVerified);
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var store = _storeService.GetStoreById(storeId);
				return Json(new { success = result.IsSuccess, message = result.Message, store = store });
			}

            TempData["Message"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Stores/ToggleBlacklist
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleBlacklist(int storeId, bool isBlacklisted)
        {
            var result = _storeService.ToggleBlacklist(storeId, isBlacklisted);
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var store = _storeService.GetStoreById(storeId); 
                return Json(new { success = result.IsSuccess, message = result.Message, store = store });
			}

            TempData["Message"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStoreStatus(int storeId, int status)
        {
            var result = _storeService.UpdateStoreStatus(storeId, status);
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var store = _storeService.GetStoreById(storeId);
				return Json(new { success = result.IsSuccess, message = result.Message, store = store });
			}

            TempData["Message"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
