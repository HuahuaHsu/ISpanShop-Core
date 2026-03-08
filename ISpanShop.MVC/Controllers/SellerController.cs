using ISpanShop.Services.Products;
using Microsoft.AspNetCore.Mvc;

namespace ISpanShop.MVC.Controllers
{
    public class SellerController : Controller
    {
        private readonly IProductService _productService;

        public SellerController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// 賣家商品管理 - 顯示該賣家所有商品（含審核狀態與退回警告）
        /// </summary>
        public IActionResult Products(int? storeId)
        {
            var criteria = new ISpanShop.Models.DTOs.Products.ProductSearchCriteria
            {
                StoreId    = storeId,
                PageNumber = 1,
                PageSize   = 50
            };
            var paged = _productService.GetProductsPaged(criteria);
            return View(paged.Data);
        }
    }
}
