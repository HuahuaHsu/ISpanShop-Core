using Microsoft.AspNetCore.Mvc;
using System.Linq;
using ISpanShop.Services.Interfaces;
using ISpanShop.MVC.Models.ViewModels;

namespace ISpanShop.MVC.Controllers
{
    /// <summary>
    /// 商品管理控制器 - 提供 MVC 後台商品管理功能
    /// </summary>
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;

        /// <summary>
        /// 建構子 - 注入 ProductService
        /// </summary>
        /// <param name="productService">商品 Service</param>
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// 商品列表 - 全站商品總覽
        /// </summary>
        /// <returns>商品列表 View</returns>
        public IActionResult Index()
        {
            // 從 Service 取得 DTO 集合
            var productDtos = _productService.GetAllProducts();

            // 將 DTO 轉換為 ViewModel
            var viewModels = productDtos.Select(dto => new ProductListVm
            {
                Id = dto.Id,
                StoreName = dto.StoreName,
                CategoryName = dto.CategoryName,
                BrandName = dto.BrandName,
                Name = dto.Name,
                MinPrice = dto.MinPrice,
                MaxPrice = dto.MaxPrice,
                Status = dto.Status,
                MainImageUrl = dto.MainImageUrl
            }).ToList();

            return View(viewModels);
        }

        /// <summary>
        /// 待審核商品列表
        /// </summary>
        /// <returns>待審核商品列表 View</returns>
        public IActionResult PendingReview()
        {
            // 從 Service 取得 DTO 集合
            var pendingProductDtos = _productService.GetPendingProducts();

            // 將 DTO 轉換為 ViewModel
            var viewModels = pendingProductDtos.Select(dto => new ProductReviewListVm
            {
                Id = dto.Id,
                StoreId = dto.StoreId,
                CategoryName = dto.CategoryName,
                BrandName = dto.BrandName,
                Name = dto.Name,
                Status = dto.Status,
                CreatedAt = dto.CreatedAt
            }).ToList();

            return View(viewModels);
        }

        /// <summary>
        /// 變更商品狀態
        /// </summary>
        /// <param name="id">商品 ID</param>
        /// <param name="newStatus">新的狀態值</param>
        /// <returns>重新導向至待審核列表</returns>
        [HttpPost]
        public IActionResult ChangeStatus(int id, byte newStatus)
        {
            _productService.ChangeProductStatus(id, newStatus);
            return RedirectToAction(nameof(PendingReview));
        }
    }
}
