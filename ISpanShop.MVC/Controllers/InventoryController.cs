using Microsoft.AspNetCore.Mvc;
using ISpanShop.Models.DTOs;
using ISpanShop.Services.Interfaces;

namespace ISpanShop.MVC.Controllers
{
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        /// <summary>
        /// 庫存監控（唯讀）：支援關鍵字搜尋、庫存狀態篩選、分類/賣家/庫存範圍篩選、排序、分頁
        /// </summary>
        public IActionResult Index(
            string? stockStatus,
            string? keyword,
            int?    categoryId,
            int?    storeId,
            int?    minStock,
            int?    maxStock,
            string? sortBy,
            int     page = 1)
        {
            var criteria = new InventorySearchCriteria
            {
                StockStatus = stockStatus,
                Keyword     = keyword,
                CategoryId  = categoryId,
                StoreId     = storeId,
                MinStock    = minStock,
                MaxStock    = maxStock,
                SortBy      = sortBy,
                PageNumber  = page,
                PageSize    = 20
            };

            var result = _inventoryService.GetInventoryPaged(criteria);

            var totalVariants   = _inventoryService.GetTotalVariantCount();
            var lowStockCount   = _inventoryService.GetLowStockCount();
            var zeroStockCount  = _inventoryService.GetZeroStockCount();
            var normalCount     = totalVariants - lowStockCount;

            ViewBag.StockStatus    = stockStatus ?? string.Empty;
            ViewBag.Keyword        = keyword;
            ViewBag.CategoryId     = categoryId;
            ViewBag.StoreId        = storeId;
            ViewBag.MinStock       = minStock;
            ViewBag.MaxStock       = maxStock;
            ViewBag.SortBy         = sortBy ?? string.Empty;
            ViewBag.TotalVariants  = totalVariants;
            ViewBag.LowStockCount  = lowStockCount;
            ViewBag.ZeroStockCount = zeroStockCount;
            ViewBag.NormalCount    = normalCount < 0 ? 0 : normalCount;
            ViewBag.Categories     = _inventoryService.GetCategoryOptions().ToList();
            ViewBag.Stores         = _inventoryService.GetStoreOptions().ToList();

            return View(result);
        }
    }
}
