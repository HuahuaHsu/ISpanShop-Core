using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
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
        /// 商品列表 - 全站商品總覽（支援多維度篩選：分類、關鍵字、商家、狀態 與 分頁）
        /// </summary>
        /// <param name="parentCategoryId">主分類 ID</param>
        /// <param name="categoryId">子分類 ID</param>
        /// <param name="keyword">關鍵字搜尋</param>
        /// <param name="storeId">商家 ID</param>
        /// <param name="status">商品狀態</param>
        /// <param name="page">頁碼（從 1 開始）</param>
        /// <returns>商品列表 View</returns>
        public IActionResult Index(int? parentCategoryId, int? categoryId, string? keyword, int? storeId, int? brandId, int? status, DateTime? startDate, DateTime? endDate, int page = 1)
        {
            var criteria = new ProductSearchCriteria
            {
                ParentCategoryId = parentCategoryId,
                CategoryId = categoryId,
                Keyword = keyword,
                StoreId = storeId,
                BrandId = brandId,
                Status = status,
                StartDate = startDate,
                EndDate = endDate,
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
                    MainImageUrl = dto.MainImageUrl,
                    CreatedAt = dto.CreatedAt
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

            // 取得商家清單並傳給前端
            var stores = _productService.GetStoreOptions().ToList();
            ViewBag.Stores = stores;

            // 取得品牌清單並傳給前端
            var brands = _productService.GetBrandOptions().ToList();
            ViewBag.Brands = brands;

            ViewBag.CurrentKeyword = keyword;
            ViewBag.CurrentStoreId = storeId;
            ViewBag.CurrentBrandId = brandId;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentStartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentEndDate = endDate?.ToString("yyyy-MM-dd");

            return View(pagedVm);
        }

        /// <summary>
        /// 待審核商品列表
        /// </summary>
        /// <returns>待審核商品列表 View</returns>
        public IActionResult PendingReview()
        {
            // 待審核商品（Status == 2）
            var pendingProductDtos = _productService.GetPendingProducts();
            var viewModels = pendingProductDtos.Select(dto => new ProductReviewListVm
            {
                Id          = dto.Id,
                StoreId     = dto.StoreId,
                CategoryName = dto.CategoryName,
                BrandName   = dto.BrandName,
                StoreName   = dto.StoreName,
                Name        = dto.Name,
                Description = dto.Description,
                Status      = dto.Status,
                CreatedAt   = dto.CreatedAt,
                UpdatedAt   = dto.UpdatedAt,
                MainImageUrl = dto.MainImageUrl
            }).ToList();

            // 近期退回紀錄（Status == 3，取最新 10 筆）
            var rejectedDtos = _productService.GetRecentRejectedProducts(10);
            ViewBag.RejectedRecords = rejectedDtos.Select(dto => new ProductReviewListVm
            {
                Id           = dto.Id,
                StoreId      = dto.StoreId,
                StoreName    = dto.StoreName,
                CategoryName = dto.CategoryName,
                Name         = dto.Name,
                Status       = dto.Status,
                RejectReason = dto.RejectReason,
                UpdatedAt    = dto.UpdatedAt,
                MainImageUrl = dto.MainImageUrl
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
        /// [AJAX] 核准商品審核 - 將狀態設為 1 (上架)
        /// </summary>
        [HttpPost]
        public IActionResult ApproveProduct(int id)
        {
            _productService.ApproveProduct(id);
            return Json(new { success = true, message = "商品已核准上架。" });
        }

        /// <summary>
        /// [AJAX] 退回商品審核 - 將狀態設為 3 (審核退回)
        /// </summary>
        [HttpPost]
        public IActionResult RejectProduct(int id, string reason)
        {
            _productService.RejectProduct(id, reason);
            return Json(new { success = true, message = $"商品已退回。退回原因：{reason}" });
        }

        /// <summary>
        /// [AJAX] 根據主分類取得對應子分類清單，供篩選器連動使用
        /// </summary>
        /// <param name="parentId">主分類 ID</param>
        /// <returns>JSON 格式子分類清單 [{ id, name }]</returns>
        [HttpGet]
        public IActionResult GetSubCategories(int parentId)
        {
            var subs = _productService.GetAllCategories()
                .Where(c => c.ParentId == parentId)
                .Select(c => new { id = c.Id, name = c.Name });
            return Json(subs);
        }

        /// <summary>
        /// [AJAX] 根據子分類取得對應品牌清單，供篩選器連動使用
        /// </summary>
        /// <param name="categoryId">子分類 ID；為 null 時回傳全部品牌</param>
        /// <returns>JSON 格式品牌清單 [{ id, name }]</returns>
        [HttpGet]
        public IActionResult GetBrandsByCategory(int? categoryId)
        {
            var brands = _productService.GetBrandsByCategory(categoryId)
                .Select(b => new { id = b.Id, name = b.Name });
            return Json(brands);
        }

        /// <summary>
        /// [AJAX] 批次更新商品上下架狀態
        /// </summary>
        /// <param name="dto">包含商品 ID 集合與目標狀態</param>
        /// <returns>JSON 格式結果 { success, message, count }</returns>
        [HttpPost]
        public async Task<IActionResult> BatchUpdateStatus([FromBody] ProductBatchUpdateStatusDto dto)
        {
            if (dto == null || dto.ProductIds == null || dto.ProductIds.Count == 0)
                return Json(new { success = false, message = "請至少勾選一筆商品。", count = 0 });

            var count = await _productService.UpdateBatchStatusAsync(dto.ProductIds, dto.TargetStatus);
            var action = dto.TargetStatus == 1 ? "上架" : "下架";
            return Json(new { success = true, message = $"成功將 {count} 筆商品設為{action}。", count });
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
