using Microsoft.AspNetCore.Mvc;
using ISpanShop.MVC.Areas.Admin.Controllers;
using ISpanShop.MVC.Middleware;
using ISpanShop.Models.DTOs.Inventories;
using ISpanShop.Models.DTOs.Common;
using ISpanShop.Services.Products;
using ISpanShop.Services.Inventories;

namespace ISpanShop.MVC.Areas.Admin.Controllers.Inventories
{
    [RequirePermission("store_manage")]
    public class InventoryController : AdminBaseController
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        /// <summary>
        /// 庫存監控（唯讀）：以商品為群組的收合式列表，支援篩選、排序、分頁
        /// </summary>
        public IActionResult Index(
            string? stockStatus,
            string? keyword,
            int?    parentCategoryId,
            int?    categoryId,
            int?    storeId,
            int?    minStock,
            int?    maxStock,
            string? sortBy,
            int     page = 1)
        {
            var criteria = new InventorySearchCriteria
            {
                StockStatus      = stockStatus,
                Keyword          = keyword,
                ParentCategoryId = parentCategoryId,
                CategoryId       = categoryId,
                StoreId          = storeId,
                MinStock         = minStock,
                MaxStock         = maxStock,
                SortBy           = sortBy,
                PageNumber       = page,
                PageSize         = 10   // 商品層級分頁，每頁 10 筆商品
            };

            var result = _inventoryService.GetInventoryGroupedPaged(criteria);

            var totalVariants   = _inventoryService.GetTotalVariantCount();
            var lowStockCount   = _inventoryService.GetLowStockCount();
            var zeroStockCount  = _inventoryService.GetZeroStockCount();
            var normalCount     = totalVariants - lowStockCount;

            ViewBag.StockStatus      = stockStatus ?? string.Empty;
            ViewBag.Keyword          = keyword;
            ViewBag.ParentCategoryId = parentCategoryId;
            ViewBag.CategoryId       = categoryId;
            ViewBag.StoreId          = storeId;
            ViewBag.MinStock         = minStock;
            ViewBag.MaxStock         = maxStock;
            ViewBag.SortBy           = sortBy ?? string.Empty;
            ViewBag.TotalVariants    = totalVariants;
            ViewBag.LowStockCount    = lowStockCount;
            ViewBag.ZeroStockCount   = zeroStockCount;
            ViewBag.NormalCount      = normalCount < 0 ? 0 : normalCount;
            ViewBag.MainCategories   = _inventoryService.GetMainCategories().ToList();
            ViewBag.Stores           = _inventoryService.GetStoreOptions().ToList();

            if (parentCategoryId.HasValue)
            {
                ViewBag.SubCategories = _inventoryService.GetSubCategories(parentCategoryId.Value).ToList();
            }
            else
            {
                ViewBag.SubCategories = new List<(int Id, string Name)>();
            }

            return View(result);
        }

        [HttpGet]
        public IActionResult GetSubCategories(int parentId)
        {
            var subs = _inventoryService.GetSubCategories(parentId)
                .Select(c => new { id = c.Id, name = c.Name });
            return Json(subs);
        }
    }
}
