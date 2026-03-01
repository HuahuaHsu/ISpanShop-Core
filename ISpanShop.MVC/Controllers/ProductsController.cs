using Microsoft.AspNetCore.Mvc;
using System.Linq;
using ISpanShop.Models.DTOs;
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
        /// 商品列表 - 全站商品總覽（支援分類篩選與分頁）
        /// </summary>
        /// <param name="parentCategoryId">主分類 ID</param>
        /// <param name="categoryId">子分類 ID</param>
        /// <param name="page">頁碼（從 1 開始）</param>
        /// <returns>商品列表 View</returns>
        public IActionResult Index(int? parentCategoryId, int? categoryId, int page = 1)
        {
            var criteria = new ProductSearchCriteria
            {
                ParentCategoryId = parentCategoryId,
                CategoryId = categoryId,
                PageNumber = page,
                PageSize = 10
            };

            // 取得分頁商品列表
            var pagedDtos = _productService.GetProductsPaged(criteria);

            // 將 DTO 轉換為 ViewModel
            var pagedVm = PagedResult<ProductListVm>.Create(
                pagedDtos.Data.Select(dto => new ProductListVm
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
                }).ToList(),
                pagedDtos.TotalCount,
                pagedDtos.CurrentPage,
                pagedDtos.PageSize
            );

            // 取得所有分類並區分主/子分類
            var allCategories = _productService.GetAllCategories().ToList();
            ViewBag.ParentCategories = allCategories.Where(c => c.ParentId == null).ToList();
            ViewBag.AllSubCategories = allCategories.Where(c => c.ParentId != null).ToList();
            ViewBag.CurrentParentCategoryId = parentCategoryId;
            ViewBag.CurrentCategoryId = categoryId;

            return View(pagedVm);
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

        /// <summary>
        /// 商品詳情 - 顯示完整的商品資訊、圖片與規格
        /// </summary>
        /// <param name="id">商品 ID</param>
        /// <returns>商品詳情 View</returns>
        public IActionResult Details(int id)
        {
            // 從 Service 取得 DTO
            var productDto = _productService.GetProductDetail(id);

            // 若找不到資料，返回 NotFound
            if (productDto == null)
            {
                return NotFound();
            }

            // 將 DTO 轉換為 ViewModel
            var viewModel = new ProductDetailVm
            {
                Id = productDto.Id,
                Name = productDto.Name,
                StoreName = productDto.StoreName,
                CategoryName = productDto.CategoryName,
                BrandName = productDto.BrandName,
                Description = productDto.Description,
                Status = productDto.Status,
                Images = productDto.Images,
                Variants = productDto.Variants.Select(v => new ProductVariantDetailVm
                {
                    SkuCode = v.SkuCode,
                    VariantName = v.VariantName,
                    Price = v.Price,
                    Stock = v.Stock,
                    SpecValueJson = v.SpecValueJson
                }).ToList()
            };

            return View(viewModel);
        }
    }
}
