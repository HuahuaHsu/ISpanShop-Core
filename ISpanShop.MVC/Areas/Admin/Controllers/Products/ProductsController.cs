using Microsoft.AspNetCore.Mvc;
using ISpanShop.MVC.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ISpanShop.Models.DTOs.Products;
using ISpanShop.Models.DTOs.Categories;
using ISpanShop.Models.DTOs.Common;
using ISpanShop.Services.Products;
using ISpanShop.Services.Inventories;
using ISpanShop.MVC.Areas.Admin.Models.Products;
using ISpanShop.MVC.Areas.Admin.Models.Categories;
using Microsoft.AspNetCore.Http;

namespace ISpanShop.MVC.Areas.Admin.Controllers.Products
{
    /// <summary>
    /// 商品管理控制器 - 提供 MVC 後台商品管理功能
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
            ViewBag.CurrentSort = sort;

            // 統計摘要卡片（全站，不受篩選影響）
            var counts = await _productService.GetProductStatusCountsAsync();
            ViewBag.CountTotal      = counts.Total;
            ViewBag.CountPublished  = counts.Published;
            ViewBag.CountUnpublished= counts.Unpublished;
            ViewBag.CountPending    = counts.Pending;

            return View(pagedVm);
        }

        /// <summary>
        /// 待審核商品列表
        /// </summary>
        /// <returns>待審核商品列表 View</returns>
        public async Task<IActionResult> PendingReview(int pendingPage = 1, int rejectedPage = 1)
        {
            const int pageSize = 10;

            // 分頁取待審核 (ReviewStatus == 0)
            var pendingPaged = await _productService.GetPendingProductsPagedAsync(pendingPage, pageSize);
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
                pendingPaged.TotalCount, pendingPage, pageSize);

            // 分頁取已退回 (ReviewStatus == 2)
            var rejectedPaged = await _productService.GetRejectedProductsPagedAsync(rejectedPage, pageSize);
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
                    RejectReason = dto.RejectReason,
                    UpdatedAt    = dto.UpdatedAt,
                    MainImageUrl = dto.MainImageUrl
                }).ToList(),
                rejectedPaged.TotalCount, rejectedPage, pageSize);

            ViewBag.RejectedPage = rejectedPage;

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
        /// [AJAX] 管理員強制下架商品（違規處理）
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ForceUnpublish([FromBody] RejectDto dto)
        {
            if (dto == null || dto.Id <= 0)
                return Json(new { success = false, message = "無效的請求資料。" });
            try
            {
                await _productService.ForceUnpublishAsync(dto.Id, dto.Reason);
                return Json(new { success = true, message = "商品已強制下架。" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"操作失敗：{ex.Message}" });
            }
        }

        // ────────────────────────────────────────────────────────
        //  [已移至前台] 管理員商品新增（暫留供前台參考）
        // ────────────────────────────────────────────────────────

#pragma warning disable CS0809
        [Obsolete("已移至前台 WebAPI。此 Action 暫不開放，保留程式碼供前台實作參考。")]
        public IActionResult Create()
        {
            return NotFound();
        }

        [Obsolete("已移至前台 WebAPI。")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateVm vm)
        {
            return NotFound();
            if (!ModelState.IsValid)
            {
                LoadAdminDropdowns();
                return View(vm);
            }

            var (imageUrl, uploadError) = await HandleImageUploadAsync(vm.ImageFile, vm.MainImageUrl);
            if (uploadError != null)
            {
                ModelState.AddModelError("ImageFile", uploadError);
                LoadAdminDropdowns();
                return View(vm);
            }

            _productService.CreateProductAdmin(new ProductAdminCreateDto
            {
                StoreId      = vm.StoreId,
                CategoryId   = vm.CategoryId,
                BrandId      = vm.BrandId,
                Name         = vm.Name,
                Description  = vm.Description,
                Price        = vm.Price,
                MainImageUrl = imageUrl
            });

            TempData["SuccessMessage"] = $"商品「{vm.Name}」已成功新增並上架。";
            return RedirectToAction(nameof(Index));
        }

        // ────────────────────────────────────────────────────────
        //  [已移至前台] 管理員商品編輯（暫留供前台參考）
        // ────────────────────────────────────────────────────────

        [Obsolete("已移至前台 WebAPI。此 Action 暫不開放。")]
        public IActionResult Edit(int id, string? returnUrl = null)
        {
            return NotFound();
            var dto = _productService.GetProductDetail(id);
            if (dto == null) return NotFound();

            var vm = new ProductEditVm
            {
                Id                 = dto.Id,
                Name               = dto.Name,
                Description        = dto.Description,
                CategoryId         = dto.CategoryId,
                BrandId            = dto.BrandId,
                MainImageUrl       = dto.Images?.FirstOrDefault(),
                SpecDefinitionJson = string.IsNullOrWhiteSpace(dto.SpecDefinitionJson)
                                     ? "[]" : dto.SpecDefinitionJson,
                ReturnUrl          = returnUrl
            };

            var activeVariants = dto.Variants?.Where(v => v.IsDeleted != true).ToList();
            ViewBag.VariantCount = activeVariants?.Count ?? 0;
            ViewBag.VariantNames = activeVariants?.Select(v => v.VariantName).ToList();
            ViewBag.FirstVariantSpecValueJson = activeVariants?
                .FirstOrDefault(v => !string.IsNullOrEmpty(v.SpecValueJson))?.SpecValueJson ?? "{}";

            LoadAdminDropdowns();
            return View(vm);
        }

        [Obsolete("已移至前台 WebAPI。")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductEditVm vm)
        {
            return NotFound();
            if (!ModelState.IsValid)
            {
                LoadAdminDropdowns();
                return View(vm);
            }

            var (imageUrl, uploadError) = await HandleImageUploadAsync(vm.ImageFile, vm.MainImageUrl);
            if (uploadError != null)
            {
                ModelState.AddModelError("ImageFile", uploadError);
                LoadAdminDropdowns();
                return View(vm);
            }

            _productService.UpdateProduct(new ProductUpdateDto
            {
                Id                 = vm.Id,
                Name               = vm.Name,
                Description        = vm.Description,
                CategoryId         = vm.CategoryId,
                BrandId            = vm.BrandId,
                MainImageUrl       = imageUrl,
                SpecDefinitionJson = vm.SpecDefinitionJson
            });

            TempData["SuccessMessage"] = $"商品「{vm.Name}」已成功更新。";
            if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);
            return RedirectToAction(nameof(Index));
        }

        // ────────────────────────────────────────────────────────
        //  [已移至前台] 軟刪除商品（暫留供前台參考）
        // ────────────────────────────────────────────────────────

        [Obsolete("已移至前台 WebAPI。管理員請使用 ForceUnpublish 強制下架。")]
        [HttpPost]
        public IActionResult DeleteProduct([FromBody] RejectDto dto)
        {
            return NotFound();
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


        private void LoadAdminDropdowns()
        {
            ViewBag.AllCategories = _productService.GetAllCategories().ToList();
            ViewBag.Stores        = _productService.GetStoreOptions().ToList();
            ViewBag.Brands        = _productService.GetBrandOptions().ToList();
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

        // ────────────────────────────────────────────────────────
        //  [已移至前台] 規格管理（暫留供前台參考）
        // ────────────────────────────────────────────────────────

        [Obsolete("已移至前台 WebAPI。")]
        public IActionResult AddVariant(int productId)
        {
            return NotFound();
            var product = _productService.GetProductDetail(productId);
            if (product == null) return NotFound();

            // 優先用 SpecDefinitionJson；若空則從現有 variants 的 SpecValueJson 推導
            string specDefJson;
            if (!string.IsNullOrWhiteSpace(product.SpecDefinitionJson) && product.SpecDefinitionJson != "[]")
            {
                specDefJson = product.SpecDefinitionJson;
            }
            else
            {
                var activeVariants = product.Variants?.Where(v => v.IsDeleted != true).ToList();
                specDefJson = SynthesizeSpecDefinition(activeVariants);
            }

            var vm = new ProductVariantCreateVm
            {
                ProductId          = productId,
                ProductName        = product.Name,
                SpecDefinitionJson = specDefJson
            };
            return View(vm);
        }

        /// <summary>從 variants 的 SpecValueJson 推導出 SpecDefinitionJson 格式</summary>
        private static string SynthesizeSpecDefinition(
            IList<ISpanShop.Models.DTOs.Products.ProductVariantDetailDto>? variants)
        {
            if (variants == null || variants.Count == 0) return "[]";

            var dimOptions = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>();
            foreach (var v in variants)
            {
                if (string.IsNullOrWhiteSpace(v.SpecValueJson)) continue;
                try
                {
                    var sv = System.Text.Json.JsonSerializer
                        .Deserialize<System.Collections.Generic.Dictionary<string, string>>(v.SpecValueJson);
                    if (sv == null) continue;
                    foreach (var (key, value) in sv)
                    {
                        if (!dimOptions.ContainsKey(key))
                            dimOptions[key] = new System.Collections.Generic.List<string>();
                        if (!dimOptions[key].Contains(value))
                            dimOptions[key].Add(value);
                    }
                }
                catch { }
            }

            if (dimOptions.Count == 0) return "[]";

            var dims = dimOptions.Select(kvp => new { name = kvp.Key, options = kvp.Value });
            return System.Text.Json.JsonSerializer.Serialize(dims);
        }

        [Obsolete("已移至前台 WebAPI。")]
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult AddVariant(ProductVariantCreateVm vm)
        {
            return NotFound();
            if (!ModelState.IsValid) return View(vm);

            var error = _productService.AddVariant(vm.ProductId, new ProductVariantCreateDto
            {
                SkuCode       = vm.SkuCode,
                VariantName   = vm.VariantName,
                SpecValueJson = vm.SpecValueJson,
                Price         = vm.Price,
                Stock         = vm.Stock,
                SafetyStock   = vm.SafetyStock
            });

            if (error != null)
            {
                ModelState.AddModelError(nameof(vm.SkuCode), error);
                return View(vm);
            }

            TempData["SuccessMessage"] = $"規格「{vm.VariantName}」已成功新增。";
            return RedirectToAction(nameof(Details), new { id = vm.ProductId });
        }

        [Obsolete("已移至前台 WebAPI。")]
        public IActionResult EditVariant(int id)
        {
            return NotFound();
            var dto = _productService.GetVariantById(id);
            if (dto == null) return NotFound();

            var product = _productService.GetProductDetail(dto.ProductId);

            var vm = new ProductVariantEditVm
            {
                Id          = dto.Id,
                ProductId   = dto.ProductId,
                ProductName = product?.Name ?? string.Empty,
                VariantName = dto.VariantName,
                SkuCode     = dto.SkuCode,
                Price       = dto.Price,
                Stock       = dto.Stock ?? 0,
                SafetyStock = dto.SafetyStock ?? 0
            };
            return View(vm);
        }

        [Obsolete("已移至前台 WebAPI。")]
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult EditVariant(ProductVariantEditVm vm)
        {
            return NotFound();
            if (!ModelState.IsValid) return View(vm);

            _productService.UpdateVariant(new ProductVariantUpdateDto
            {
                Id          = vm.Id,
                SkuCode     = vm.SkuCode,
                Price       = vm.Price,
                Stock       = vm.Stock,
                SafetyStock = vm.SafetyStock
            });

            TempData["SuccessMessage"] = $"規格「{vm.VariantName}」已成功更新。";
            return RedirectToAction(nameof(Details), new { id = vm.ProductId });
        }

        [Obsolete("已移至前台 WebAPI。")]
        [HttpPost]
        public IActionResult DeleteVariant([FromBody] RejectDto dto)
        {
            return NotFound();
        }
    }
#pragma warning restore CS0809
}
