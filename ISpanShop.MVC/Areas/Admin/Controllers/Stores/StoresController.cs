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
        public IActionResult Index()
        {
            var stores = _storeService.GetAllStores().ToList();
            var vm = new StoreIndexVm
            {
                Stores = stores,
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
            TempData["Message"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Stores/ToggleBlacklist
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleBlacklist(int storeId, bool isBlacklisted)
        {
            var result = _storeService.ToggleBlacklist(storeId, isBlacklisted);
            TempData["Message"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
