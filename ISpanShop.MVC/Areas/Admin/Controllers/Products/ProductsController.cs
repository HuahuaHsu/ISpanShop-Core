using Microsoft.AspNetCore.Mvc;
using ISpanShop.MVC.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ISpanShop.Models.DTOs.Products;
using ISpanShop.Models.DTOs.Common;
using ISpanShop.Services.Products;
using ISpanShop.MVC.Areas.Admin.Models.Products;
using Microsoft.AspNetCore.Http;
using ISpanShop.MVC.Middleware;

namespace ISpanShop.MVC.Areas.Admin.Controllers.Products
{
    [RequirePermission("product_manage")]
    /// <summary>
    /// 商品監管控制器 - 處理後台商品審核、下架等管理功能
    /// </summary>
    public class ProductsController : AdminBaseController

    {
        private readonly IProductService _productService;
        private readonly IWebHostEnvironment _env;

        private static readonly string[] _allowedExts = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        public ProductsController(IProductService productService, IWebHostEnvironment env)
        {
            _productService = productService;
            _env = env;
        }

        /// <summary>
        /// 處理主圖上傳。優先使用 ImageFile；若無，保留 urlFallback。
        /// 回傳最終要存入 DB 的圖片路徑/URL，或 null。
        /// </summary>
        private async Task<(string? url, string? error)> HandleImageUploadAsync(IFormFile? file, string? urlFallback)
        {
            if (file != null && file.Length > 0)
            {
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_allowedExts.Contains(ext))
                    return (null, "僅支援 jpg、jpeg、png、webp 格式的圖片。");
                if (file.Length > MaxFileSize)
                    return (null, "圖片檔案大小不可超過 5 MB。");

                var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "products");
                Directory.CreateDirectory(uploadDir);

                var fileName = $"{Guid.NewGuid()}{ext}";
                var fullPath = Path.Combine(uploadDir, fileName);
                using var stream = System.IO.File.Create(fullPath);
                await file.CopyToAsync(stream);

                return ($"/uploads/products/{fileName}", null);
            }
            return (urlFallback, null);
        }

        /// <summary>
        /// 商品總覽 - 管理員唯讀監控視角（支援多維度篩選：分類、關鍵字、商家、狀態 與 分頁）
        /// </summary>
        /// <param name="parentCategoryId">主分類 ID</param>
        /// <param name="categoryId">子分類 ID</param>
        /// <param name="keyword">關鍵字搜尋</param>
        /// <param name="storeId">商家 ID</param>
        /// <param name="status">商品狀態</param>
        /// <param name="page">頁碼（從 1 開始）</param>
        /// <returns>商品列表 View</returns>
        public async Task<IActionResult> Index(int? parentCategoryId, int? categoryId, string? keyword, int? storeId, int? brandId, int? status, DateTime? startDate, DateTime? endDate, string? sort = null, int page = 1)
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
                SortOrder = sort,
                PageNumber = page,
                PageSize = 10
            };

            // 取得分頁商品列表（async + 投影 + SQL 分頁）
            var pagedDtos = await _productService.GetProductsPagedAsync(criteria);

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
                    CreatedAt = dto.CreatedAt,
                    ReviewStatus = dto.ReviewStatus,
                    ReviewedBy = dto.ReviewedBy,
                    ReviewDate = dto.ReviewDate
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
            ViewBag.CurrentSort = sort;

            // 統計摘要卡片（全站，不受篩選影響）
            var counts = await _productService.GetProductStatusCountsAsync();
            ViewBag.CountTotal          = counts.Total;
            ViewBag.CountPublished      = counts.Published;
            ViewBag.CountUnpublished    = counts.Unpublished;
            ViewBag.CountPending        = counts.Pending;
            ViewBag.CountForcedOffShelf = counts.ForcedOffShelf;

            return View(pagedVm);
        }

        /// <summary>
        /// 待審核商品列表
        /// </summary>
        public async Task<IActionResult> PendingReview()
        {
            const int pageSize = 999; // 前端自行分頁，伺服器一次載入全量資料

            // 全量取待審核 (ReviewStatus == 0)
            var pendingPaged = await _productService.GetPendingProductsPagedAsync(1, pageSize);
            var pendingVm = PagedResult<ProductReviewListVm>.Create(
                pendingPaged.Data.Select(dto => new ProductReviewListVm
                {
                    Id           = dto.Id,
                    StoreId      = dto.StoreId,
                    CategoryName = dto.CategoryName,
                    BrandName    = dto.BrandName,
                    StoreName    = dto.StoreName,
                    Name         = dto.Name,
                    Description  = dto.Description,
                    Status       = dto.Status,
                    ReviewStatus = dto.ReviewStatus,
                    ReviewedBy   = dto.ReviewedBy,
                    ReviewDate   = dto.ReviewDate,
                    CreatedAt    = dto.CreatedAt,
                    UpdatedAt    = dto.UpdatedAt,
                    MainImageUrl = dto.MainImageUrl
                }).ToList(),
                pendingPaged.TotalCount, 1, pageSize);

            // 全量取已退回 (ReviewStatus == 2)
            var rejectedPaged = await _productService.GetRejectedProductsPagedAsync(1, pageSize);
            ViewBag.RejectedRecords = PagedResult<ProductReviewListVm>.Create(
                rejectedPaged.Data.Select(dto => new ProductReviewListVm
                {
                    Id           = dto.Id,
                    StoreId      = dto.StoreId,
                    StoreName    = dto.StoreName,
                    CategoryName = dto.CategoryName,
                    Name         = dto.Name,
                    Status       = dto.Status,
                    ReviewStatus = dto.ReviewStatus,
                    ReviewedBy   = dto.ReviewedBy,
                    ReviewDate   = dto.ReviewDate,
                    RejectReason        = dto.RejectReason,
                    ForceOffShelfReason = dto.ForceOffShelfReason,
                    UpdatedAt    = dto.UpdatedAt,
                    MainImageUrl = dto.MainImageUrl
                }).ToList(),
                rejectedPaged.TotalCount, 1, pageSize);

            // 重新申請審核 (ReviewStatus == 3)
            var reApplyPaged = await _productService.GetReApplyProductsPagedAsync(1, pageSize);
            ViewBag.ReApplyRecords = PagedResult<ProductReviewListVm>.Create(
                reApplyPaged.Data.Select(dto => new ProductReviewListVm
                {
                    Id                  = dto.Id,
                    StoreId             = dto.StoreId,
                    StoreName           = dto.StoreName,
                    CategoryName        = dto.CategoryName,
                    BrandName           = dto.BrandName ?? string.Empty,
                    Name                = dto.Name,
                    Description         = dto.Description,
                    Status              = dto.Status,
                    ReviewStatus        = dto.ReviewStatus,
                    ReviewedBy          = dto.ReviewedBy,
                    ReviewDate          = dto.ReviewDate,
                    RejectReason        = dto.RejectReason,
                    ForceOffShelfReason = dto.ForceOffShelfReason,
                    ForceOffShelfDate   = dto.ForceOffShelfDate,
                    ForceOffShelfBy     = dto.ForceOffShelfBy,
                    ReApplyDate         = dto.ReApplyDate,
                    CreatedAt           = dto.CreatedAt,
                    UpdatedAt           = dto.UpdatedAt,
                    MainImageUrl        = dto.MainImageUrl
                }).ToList(),
                reApplyPaged.TotalCount, 1, pageSize);

            // 近期審核通過（24 小時內 ReviewStatus == 1）全量
            var approvedPaged = await _productService.GetRecentlyApprovedPagedAsync(1, pageSize, 24);
            ViewBag.ApprovedRecords = PagedResult<ProductReviewListVm>.Create(
                approvedPaged.Data.Select(dto => new ProductReviewListVm
                {
                    Id           = dto.Id,
                    StoreId      = dto.StoreId,
                    StoreName    = dto.StoreName,
                    CategoryName = dto.CategoryName,
                    BrandName    = dto.BrandName ?? string.Empty,
                    Name         = dto.Name,
                    Status       = dto.Status,
                    ReviewStatus = dto.ReviewStatus,
                    ReviewedBy   = dto.ReviewedBy,
                    ReviewDate   = dto.ReviewDate,
                    CreatedAt    = dto.CreatedAt,
                    MainImageUrl = dto.MainImageUrl
                }).ToList(),
                approvedPaged.TotalCount, 1, pageSize);

            return View(pendingVm);
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
        public async Task<IActionResult> ApproveProduct([FromBody] RejectDto dto)
        {
            if (dto == null || dto.Id <= 0)
                return Json(new { success = false, message = "無效的請求資料。" });
            try
            {
                var adminId = User.Identity?.Name ?? "Admin";
                await _productService.ApproveProductAsync(dto.Id, adminId);
                return Json(new { success = true, message = "商品已核准上架。" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"操作失敗：{ex.Message}" });
            }
        }

        /// <summary>
        /// [AJAX] 退回商品審核 - 將狀態設為 3 (審核退回)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RejectProduct([FromBody] RejectDto dto)
        {
            if (dto == null || dto.Id <= 0)
                return Json(new { success = false, message = "無效的請求資料。" });
            try
            {
                var adminId = User.Identity?.Name ?? "Admin";
                await _productService.RejectProductAsync(dto.Id, adminId, dto.Reason ?? string.Empty);
                return Json(new { success = true, message = $"商品已退回。退回原因：{dto.Reason}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"操作失敗：{ex.Message}" });
            }
        }

        /// <summary>
        /// [AJAX] 批次審核通過 - 批次核准商品上架
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BatchApprove([FromBody] BatchReviewDto dto)
        {
            if (dto == null || dto.Ids == null || dto.Ids.Count == 0)
                return Json(new { success = false, message = "請至少選擇一筆商品。" });
            try
            {
                var adminId = User.Identity?.Name ?? "Admin";
                int count = 0;
                foreach (var id in dto.Ids)
                {
                    await _productService.ApproveProductAsync(id, adminId);
                    count++;
                }
                return Json(new { success = true, message = $"成功核准 {count} 筆商品上架。" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"操作失敗：{ex.Message}" });
            }
        }

        /// <summary>
        /// [AJAX] 批次審核退回 - 批次退回商品並記錄原因
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BatchReject([FromBody] BatchRejectDto dto)
        {
            if (dto == null || dto.Ids == null || dto.Ids.Count == 0)
                return Json(new { success = false, message = "請至少選擇一筆商品。" });
            if (string.IsNullOrWhiteSpace(dto.Reason))
                return Json(new { success = false, message = "退回原因不可為空。" });
            try
            {
                var adminId = User.Identity?.Name ?? "Admin";
                int count = 0;
                foreach (var id in dto.Ids)
                {
                    await _productService.RejectProductAsync(id, adminId, dto.Reason);
                    count++;
                }
                return Json(new { success = true, message = $"成功退回 {count} 筆商品。" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"操作失敗：{ex.Message}" });
            }
        }

        /// <summary>
        /// [AJAX] 批次重新審核 - 批次將退回商品移回待審核
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BatchUndoReject([FromBody] BatchReviewDto dto)
        {
            if (dto == null || dto.Ids == null || dto.Ids.Count == 0)
                return Json(new { success = false, message = "未選取任何商品。" });
            try
            {
                int count = 0;
                foreach (var id in dto.Ids)
                {
                    await _productService.ResetToPendingAsync(id);
                    count++;
                }
                return Json(new { success = true, message = $"已將 {count} 筆商品移回待審核。" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"操作失敗：{ex.Message}" });
            }
        }

        /// <summary>
        /// [AJAX] 批次模擬賣家送審 - 將多筆 ReviewStatus=2（已退回）的商品移至 ReviewStatus=3（待重新審核）
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BatchSimulateResubmit([FromBody] BatchReviewDto dto)
        {
            if (dto == null || dto.Ids == null || dto.Ids.Count == 0)
                return Json(new { success = false, message = "未選取任何商品。" });
            try
            {
                int count = 0;
                foreach (var id in dto.Ids)
                {
                    await _productService.SimulateSellerResubmitAsync(id);
                    count++;
                }
                return Json(new { success = true, message = $"已模擬賣家重新送審 {count} 筆商品，商品已移至「重新申請審核」列表。" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"操作失敗：{ex.Message}" });
            }
        }

        /// <summary>
        /// [AJAX] 重新審核 - 將已退回商品重設為待審核（清空審核結果欄位）
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UndoReject([FromBody] RejectDto dto)
        {
            if (dto == null || dto.Id <= 0)
                return Json(new { success = false, message = "無效的請求資料。" });
            try
            {
                await _productService.ResetToPendingAsync(dto.Id);
                return Json(new { success = true, message = "商品已移回待審核佇列。" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"操作失敗：{ex.Message}" });
            }
        }

        /// <summary>
        /// [AJAX] 模擬賣家修改後重新送審 — 將 ReviewStatus=2（已退回）的商品移至 ReviewStatus=3（待重新審核）。
        /// 商品將出現在「重新申請審核」頁籤，展示完整的退回→修改→重新送審流程。
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SimulateSellerResubmit([FromBody] RejectDto dto)
        {
            if (dto == null || dto.Id <= 0)
                return Json(new { success = false, message = "無效的請求資料。" });
            try
            {
                await _productService.SimulateSellerResubmitAsync(dto.Id);
                return Json(new { success = true, message = "已模擬賣家重新送審，商品已移至「重新申請審核」列表。" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"操作失敗：{ex.Message}" });
            }
        }

        /// <summary>
        /// [AJAX] 重新送審 - 前台賣家或管理員呼叫，效果同 UndoReject
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ResetToPending([FromBody] RejectDto dto)
        {
            if (dto == null || dto.Id <= 0)
                return Json(new { success = false, message = "無效的請求資料。" });
            try
            {
                await _productService.ResetToPendingAsync(dto.Id);
                return Json(new { success = true, message = "商品已重新送審，等待審核中。" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"操作失敗：{ex.Message}" });
            }
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
        /// [AJAX] 管理員強制下架商品（違規處理，Status→4）
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ForceUnpublish([FromBody] RejectDto dto)
        {
            if (dto == null || dto.Id <= 0)
                return Json(new { success = false, message = "無效的請求資料。" });
            try
            {
                var adminBy = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var uid) ? uid : (int?)null;
                await _productService.ForceUnpublishAsync(dto.Id, dto.Reason, adminBy);
                return Json(new { success = true, message = "商品已強制下架。" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"操作失敗：{ex.Message}" });
            }
        }

        /// <summary>
        /// [AJAX] 批次強制下架
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BatchForceOffShelf([FromBody] BatchRejectDto dto)
        {
            if (dto == null || dto.Ids == null || dto.Ids.Count == 0)
                return Json(new { success = false, message = "請至少選擇一筆商品。" });
            if (string.IsNullOrWhiteSpace(dto.Reason))
                return Json(new { success = false, message = "下架原因不可為空。" });
            try
            {
                var adminBy = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var uid2) ? uid2 : (int?)null;
                int count = await _productService.BatchForceOffShelfAsync(dto.Ids, dto.Reason, adminBy);
                return Json(new { success = true, message = $"成功強制下架 {count} 筆商品。", count });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"操作失敗：{ex.Message}" });
            }
        }

        /// <summary>
        /// [AJAX] 賣家申請重新上架（ReviewStatus→3）
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ReApply([FromBody] RejectDto dto)
        {
            if (dto == null || dto.Id <= 0)
                return Json(new { success = false, message = "無效的請求資料。" });
            try
            {
                await _productService.ReApplyAsync(dto.Id);
                return Json(new { success = true, message = "已申請重新上架，等待管理員審核。" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"操作失敗：{ex.Message}" });
            }
        }

        /// <summary>
        /// [AJAX] 管理員核准強制下架商品重新上架
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ApproveForcedProduct([FromBody] RejectDto dto)
        {
            if (dto == null || dto.Id <= 0)
                return Json(new { success = false, message = "無效的請求資料。" });
            try
            {
                var adminId = User.Identity?.Name ?? "Admin";
                await _productService.ApproveForcedProductAsync(dto.Id, adminId);
                return Json(new { success = true, message = "商品已核准重新上架。" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"操作失敗：{ex.Message}" });
            }
        }

        /// <summary>
        /// [AJAX] 管理員駁回重新申請
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RejectForcedProduct([FromBody] RejectDto dto)
        {
            if (dto == null || dto.Id <= 0)
                return Json(new { success = false, message = "無效的請求資料。" });
            if (string.IsNullOrWhiteSpace(dto.Reason))
                return Json(new { success = false, message = "駁回原因不可為空。" });
            try
            {
                var adminId = User.Identity?.Name ?? "Admin";
                await _productService.RejectForcedProductAsync(dto.Id, adminId, dto.Reason);
                return Json(new { success = true, message = "重新申請已駁回。" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"操作失敗：{ex.Message}" });
            }
        }

        // ────────────────────────────────────────────────────────
        //  [Demo 專用] 強制清理過期退回商品
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Demo 觸發端點：立即軟刪除所有已退回商品。
        /// 傳入 days=0 代表「只要被退回就視為過期」，方便 Demo 展示。
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ForceCleanupExpiredProducts(int expirationSeconds = 60)
        {
            try
            {
                int count = await _productService.CleanupExpiredRejectedProductsAsync(expirationSeconds);
                return Json(new { success = true, cleaned = count, message = $"已清理 {count} 筆過期退回商品。" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"清理失敗：{ex.Message}" });
            }
        }


        /// <summary>
        /// 商品詳情 - 顯示完整的商品資訊、圖片與規格
        /// </summary>
        /// <param name="id">商品 ID</param>
        /// <returns>商品詳情 View</returns>
        public IActionResult Details(int id, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

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
                Id                 = productDto.Id,
                Name               = productDto.Name,
                StoreName          = productDto.StoreName,
                CategoryName       = productDto.CategoryName,
                BrandName          = productDto.BrandName,
                Description        = productDto.Description,
                Status             = productDto.Status,
                MinPrice           = productDto.MinPrice,
                MaxPrice           = productDto.MaxPrice,
                TotalSales         = productDto.TotalSales,
                ViewCount          = productDto.ViewCount,
                RejectReason       = productDto.RejectReason,
                SpecDefinitionJson = productDto.SpecDefinitionJson,
                CreatedAt          = productDto.CreatedAt,
                UpdatedAt          = productDto.UpdatedAt,
                ReviewStatus       = productDto.ReviewStatus,
                ReviewedBy         = productDto.ReviewedBy,
                ReviewDate         = productDto.ReviewDate,
                Images             = productDto.Images,
                Variants = productDto.Variants.Select(v => new ProductVariantDetailVm
                {
                    Id            = v.Id,
                    ProductId     = v.ProductId,
                    SkuCode       = v.SkuCode,
                    VariantName   = v.VariantName,
                    Price         = v.Price,
                    Stock         = v.Stock,
                    SafetyStock   = v.SafetyStock,
                    SpecValueJson = v.SpecValueJson,
                    IsDeleted     = v.IsDeleted ?? false
                }).ToList()
            };

            return View(viewModel);
        }

        /// <summary>
        /// [AJAX] 取得商品詳情 Partial View（供 Offcanvas 側邊欄使用）
        /// </summary>
        public IActionResult GetProductDetailsPartial(int id, bool isReviewMode = false)
        {
            var productDto = _productService.GetProductDetail(id);
            if (productDto == null)
                return NotFound();

            var vm = new ProductDetailVm
            {
                Id                  = productDto.Id,
                Name                = productDto.Name,
                StoreName           = productDto.StoreName,
                CategoryName        = productDto.CategoryName,
                BrandName           = productDto.BrandName,
                Description         = productDto.Description,
                Status              = productDto.Status,
                MinPrice            = productDto.MinPrice,
                MaxPrice            = productDto.MaxPrice,
                TotalSales          = productDto.TotalSales,
                ViewCount           = productDto.ViewCount,
                RejectReason        = productDto.RejectReason,
                SpecDefinitionJson  = productDto.SpecDefinitionJson,
                CreatedAt           = productDto.CreatedAt,
                UpdatedAt           = productDto.UpdatedAt,
                ReviewStatus        = productDto.ReviewStatus,
                ReviewedBy          = productDto.ReviewedBy,
                ReviewDate          = productDto.ReviewDate,
                ForceOffShelfReason = productDto.ForceOffShelfReason,
                ForceOffShelfDate   = productDto.ForceOffShelfDate,
                ForceOffShelfBy     = productDto.ForceOffShelfBy,
                ReApplyDate         = productDto.ReApplyDate,
                Images              = productDto.Images,
                Variants = productDto.Variants.Select(v => new ProductVariantDetailVm
                {
                    Id            = v.Id,
                    ProductId     = v.ProductId,
                    SkuCode       = v.SkuCode,
                    VariantName   = v.VariantName,
                    Price         = v.Price,
                    Stock         = v.Stock,
                    SafetyStock   = v.SafetyStock,
                    SpecValueJson = v.SpecValueJson,
                    IsDeleted     = v.IsDeleted ?? false
                }).ToList()
            };

            ViewBag.IsReviewMode = isReviewMode;
            return PartialView("_ProductDetailsPartial", vm);
        }

        /// <summary>
        /// [AJAX] 生成 15 筆測試用待審核商品（乾淨 / 高風險 / 邊緣 各 5 筆）
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GenerateTestProducts()
        {
            try
            {
                var result = await _productService.GenerateTestProductsAsync();
                // 生成後取最新待審核總筆數，供前端動態更新分頁器
                var pendingPaged = await _productService.GetPendingProductsPagedAsync(1, 1);
                return Json(new
                {
                    success          = true,
                    count            = result.TotalCount,
                    clean            = result.CleanCount,
                    highRisk         = result.HighRiskCount,
                    borderline       = result.BorderlineCount,
                    pendingTotalCount = pendingPaged.TotalCount,
                    message          = $"已生成 {result.TotalCount} 筆測試商品（乾淨 {result.CleanCount} 筆 ／ 高風險 {result.HighRiskCount} 筆 ／ 邊緣 {result.BorderlineCount} 筆），全部設為待審核。",
                    products         = result.CreatedProducts.Select(p => new
                    {
                        id       = p.Id,
                        name     = p.Name,
                        store    = p.StoreName,
                        img      = p.MainImageUrl ?? "",
                        category = p.CategoryName,
                        brand    = p.BrandName,
                        created  = p.CreatedAt?.ToString("yyyy/MM/dd") ?? "-"
                    })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"生成失敗：{ex.Message}" });
            }
        }

        /// <summary>
        /// [AJAX] 對目前所有待審核商品執行敏感字自動比對（敏感字從資料庫讀取）
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SimulateAutoReview()
        {
            try
            {
                var result = await _productService.SimulateAutoReviewAsync();
                return Json(new
                {
                    success      = true,
                    approved     = result.ApprovedCount,
                    rejected     = result.RejectedCount,
                    manualReview = result.ManualReviewCount,
                    items        = result.Items.Select(i => new
                    {
                        productId    = i.ProductId,
                        productName  = i.ProductName,
                        outcome      = i.Outcome,
                        matchedWords = i.MatchedWords
                    })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"模擬失敗：{ex.Message}" });
            }
        }
    }
}
