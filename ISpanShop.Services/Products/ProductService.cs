using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISpanShop.Models.DTOs.Products;
using ISpanShop.Models.DTOs.Categories;
using ISpanShop.Models.DTOs.Common;
using ISpanShop.Models.EfModels;
using ISpanShop.Repositories.Products;
using ISpanShop.Services.ContentModeration;
using ISpanShop.Services.Products;

namespace ISpanShop.Services.Products
{
    /// <summary>
    /// 商品 Service 實作 - 處理商品相關的商業邏輯
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ISensitiveWordService _sensitiveWordService;

        public ProductService(IProductRepository productRepository, ISensitiveWordService sensitiveWordService)
        {
            _productRepository   = productRepository;
            _sensitiveWordService = sensitiveWordService;
        }

        /// <summary>
        /// 建立新商品
        /// </summary>
        public int CreateProduct(ProductCreateDto dto)
        {
            var product = new ISpanShop.Models.EfModels.Product
            {
                StoreId            = dto.StoreId,
                CategoryId         = dto.CategoryId,
                BrandId            = dto.BrandId,
                Name               = dto.Name,
                Description        = dto.Description,
                VideoUrl           = dto.VideoUrl,
                SpecDefinitionJson = dto.SpecDefinitionJson,
                CreatedAt          = DateTime.Now,
                UpdatedAt          = DateTime.Now,
                Status             = dto.Status,
                ReviewStatus       = dto.ReviewStatus
            };

            if (dto.Variants != null && dto.Variants.Count > 0)
            {
                product.MinPrice = dto.Variants.Min(v => v.Price);
                product.MaxPrice = dto.Variants.Max(v => v.Price);
                product.ProductVariants = dto.Variants.Select(variantDto =>
                    new ISpanShop.Models.EfModels.ProductVariant
                    {
                        SkuCode       = string.IsNullOrWhiteSpace(variantDto.SkuCode)
                                        ? $"{Guid.NewGuid().ToString("N")[..8].ToUpper()}"
                                        : variantDto.SkuCode,
                        VariantName   = variantDto.VariantName,
                        SpecValueJson = variantDto.SpecValueJson,
                        Price         = variantDto.Price,
                        Stock         = variantDto.Stock,
                        SafetyStock   = variantDto.SafetyStock,
                    }).ToList();
            }

            _productRepository.AddProduct(product);
            return product.Id;
        }

        /// <summary>
        /// 分頁取得商品列表
        /// </summary>
        public PagedResult<ProductListDto> GetProductsPaged(ProductSearchCriteria criteria)
        {
            var (items, totalCount) = _productRepository.GetProductsPaged(criteria);
            var dtos = items.Select(p => new ProductListDto
            {
                Id           = p.Id,
                StoreName    = p.Store?.StoreName ?? "未知商店",
                CategoryName = p.Category?.Name ?? "未分類",
                BrandName    = p.Brand?.Name ?? "未設定",
                Name         = p.Name,
                MinPrice     = p.MinPrice,
                MaxPrice     = p.MaxPrice,
                Status       = p.Status,
                CreatedAt    = p.CreatedAt,
                ReviewStatus = p.ReviewStatus,
                ReviewedBy   = p.ReviewedBy,
                ReviewDate   = p.ReviewDate,
                RejectReason = p.RejectReason,
                TotalStock   = p.ProductVariants
                    ?.Where(v => v.IsDeleted != true)
                    .Sum(v => v.Stock ?? 0) ?? 0,
                TotalSales   = p.TotalSales,
                ViewCount    = p.ViewCount,
                MainImageUrl = p.ProductImages
                    ?.FirstOrDefault(img => img.IsMain == true)?.ImageUrl
                    ?? p.ProductImages?.FirstOrDefault()?.ImageUrl
                    ?? "https://via.placeholder.com/400x400?text=No+Image",
                IsDeleted    = p.IsDeleted == true
            }).ToList();
            return PagedResult<ProductListDto>.Create(dtos, totalCount, criteria.PageNumber, criteria.PageSize);
        }

        /// <summary>
        /// 更新商品基本資料
        /// </summary>
        public void UpdateProduct(ProductUpdateDto dto)
            => _productRepository.UpdateProduct(dto);

        /// <summary>
        /// 軟刪除商品（IsDeleted = true）
        /// </summary>
        public void SoftDeleteProduct(int id)
            => _productRepository.SoftDeleteProduct(id);

        /// <summary>
        /// 根據 ID 取得規格詳情
        /// </summary>
        public ProductVariantDetailDto? GetVariantById(int id)
        {
            var v = _productRepository.GetVariantById(id);
            if (v == null) return null;
            return new ProductVariantDetailDto
            {
                Id            = v.Id,
                ProductId     = v.ProductId,
                SkuCode       = v.SkuCode,
                VariantName   = v.VariantName,
                Price         = v.Price,
                Stock         = v.Stock,
                SafetyStock   = v.SafetyStock,
                SpecValueJson = v.SpecValueJson,
                IsDeleted     = v.IsDeleted
            };
        }

        /// <summary>
        /// 為商品新增規格（自動產生 SKU 若未提供）
        /// </summary>
        public string? AddVariant(int productId, ProductVariantCreateDto dto)
        {
            string skuCode;
            if (string.IsNullOrWhiteSpace(dto.SkuCode))
            {
                int attempts = 0;
                do { skuCode = Guid.NewGuid().ToString("N")[..8].ToUpper(); attempts++; }
                while (_productRepository.IsSkuExists(skuCode) && attempts < 5);
            }
            else
            {
                skuCode = dto.SkuCode.Trim();
                if (_productRepository.IsSkuExists(skuCode))
                    return "此 SKU 代碼已被使用，請更換。";
            }

            var variant = new ISpanShop.Models.EfModels.ProductVariant
            {
                ProductId     = productId,
                SkuCode       = skuCode,
                VariantName   = dto.VariantName,
                SpecValueJson = dto.SpecValueJson,
                Price         = dto.Price,
                Stock         = dto.Stock,
                SafetyStock   = dto.SafetyStock,
                IsDeleted     = false
            };

            try { _productRepository.AddVariant(variant); }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
                when (ex.InnerException?.Message.Contains("UNIQUE") == true ||
                      ex.InnerException?.Message.Contains("unique") == true ||
                      ex.InnerException?.Message.Contains("duplicate") == true)
            { return "此 SKU 代碼已被使用，請更換。"; }

            return null;
        }

        /// <summary>
        /// 更新規格
        /// </summary>
        public void UpdateVariant(ProductVariantUpdateDto dto)
            => _productRepository.UpdateVariant(dto);

        /// <summary>
        /// 軟刪除規格
        /// </summary>
        public void SoftDeleteVariant(int id)
            => _productRepository.SoftDeleteVariant(id);

        /// <summary>
        /// 變更商品狀態
        /// </summary>
        public void ChangeProductStatus(int id, byte newStatus)
        {
            _productRepository.UpdateProductStatus(id, newStatus);
        }

        /// <summary>
        /// 取得所有分類清單（含主分類與子分類）
        /// </summary>
        /// <returns>分類 DTO 集合</returns>
        public IEnumerable<CategoryDto> GetAllCategories()
        {
            return _productRepository.GetAllCategories()
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParentId = c.ParentId
                })
                .ToList();
        }

        /// <summary>
        /// 根據 ID 取得商品詳情 - 轉換為 DTO（包含完整圖片與規格列表）
        /// </summary>
        /// <param name="id">商品 ID</param>
        /// <returns>商品詳情 DTO，若不存在則返回 null</returns>
        public ProductDetailDto? GetProductDetail(int id)
        {
            var product = _productRepository.GetProductById(id);

            if (product == null)
            {
                return null;
            }

            return new ProductDetailDto
            {
                Id                 = product.Id,
                Name               = product.Name,
                StoreId            = product.StoreId,
                StoreName          = product.Store?.StoreName ?? "未知商店",
                CategoryId         = product.CategoryId,
                CategoryName       = product.Category?.Name ?? "未分類",
                BrandId            = product.BrandId,
                BrandName          = product.Brand?.Name ?? "未設定",
                Description        = product.Description,
                Status             = product.Status,
                MinPrice           = product.MinPrice,
                MaxPrice           = product.MaxPrice,
                TotalSales         = product.TotalSales,
                ViewCount          = product.ViewCount,
                RejectReason       = product.RejectReason,
                SpecDefinitionJson = product.SpecDefinitionJson,
                CreatedAt          = product.CreatedAt,
                UpdatedAt          = product.UpdatedAt,
                ReviewStatus        = product.ReviewStatus,
                ReviewedBy          = product.ReviewedBy,
                ReviewDate          = product.ReviewDate,
                ForceOffShelfReason = product.ForceOffShelfReason,
                ForceOffShelfDate   = product.ForceOffShelfDate,
                ForceOffShelfBy     = product.ForceOffShelfBy,
                ReApplyDate         = product.ReApplyDate,
                AttributesJson     = product.AttributesJson,
                Images = product.ProductImages?
                    .OrderBy(img => img.SortOrder)
                    .Select(img => img.ImageUrl)
                    .ToList() ?? new List<string>(),
                Variants = product.ProductVariants?
                    .Where(v => v.IsDeleted != true)
                    .Select(v => new ProductVariantDetailDto
                    {
                        Id            = v.Id,
                        ProductId     = v.ProductId,
                        SkuCode       = v.SkuCode,
                        VariantName   = v.VariantName,
                        Price         = v.Price,
                        Stock         = v.Stock,
                        SafetyStock   = v.SafetyStock,
                        SpecValueJson = v.SpecValueJson,
                        IsDeleted     = v.IsDeleted
                    })
                    .ToList() ?? new List<ProductVariantDetailDto>()
            };
        }

        /// <summary>
        /// 取得所有商家清單
        /// </summary>
        /// <returns>商家清單 (Id, Name)</returns>
        public IEnumerable<(int Id, string Name)> GetStoreOptions()
        {
            return _productRepository.GetStoreOptions();
        }

        /// <summary>
        /// 取得所有品牌清單
        /// </summary>
        /// <returns>品牌清單 (Id, Name)</returns>
        public IEnumerable<(int Id, string Name)> GetBrandOptions()
        {
            return _productRepository.GetBrandOptions();
        }

        /// <summary>
        /// 根據子分類取得該分類下商品涵蓋的品牌清單
        /// </summary>
        /// <param name="categoryId">子分類 ID；為 null 時回傳全部品牌</param>
        /// <returns>品牌清單 (Id, Name)</returns>
        public IEnumerable<(int Id, string Name)> GetBrandsByCategory(int? categoryId)
        {
            return _productRepository.GetBrandsByCategory(categoryId);
        }

        /// <summary>
        /// 批次更新商品上下架狀態
        /// </summary>
        public async Task<int> UpdateBatchStatusAsync(List<int> productIds, byte targetStatus)
        {
            if (productIds == null || productIds.Count == 0) return 0;
            return await _productRepository.UpdateBatchStatusAsync(productIds, targetStatus);
        }

        /// <summary>
        /// 批次更新商品審核狀態
        /// </summary>
        public async Task<int> UpdateBatchReviewStatusAsync(List<int> productIds, int targetReviewStatus, string adminId)
        {
            if (productIds == null || productIds.Count == 0) return 0;
            return await _productRepository.UpdateBatchReviewStatusAsync(productIds, targetReviewStatus, adminId);
        }

        // ═══════════════════════════════════════════════════════════
        //  非同步實作（async/await + 投影 + 真分頁）
        // ═══════════════════════════════════════════════════════════

        /// <inheritdoc/>
        public async Task<IEnumerable<ProductReviewDto>> GetPendingProductsAsync()
            => await _productRepository.GetPendingProductsAsync();

        /// <inheritdoc/>
        public async Task<PagedResult<ProductListDto>> GetProductsPagedAsync(ProductSearchCriteria criteria)
        {
            var (items, totalCount) = await _productRepository.GetProductsPagedAsync(criteria);
            return PagedResult<ProductListDto>.Create(items.ToList(), totalCount, criteria.PageNumber, criteria.PageSize);
        }

        /// <inheritdoc/>
        public async Task ApproveProductAsync(int id, string adminId)
            => await _productRepository.ApproveProductAsync(id, adminId);

        /// <inheritdoc/>
        public async Task RejectProductAsync(int id, string adminId, string reason)
            => await _productRepository.RejectProductAsync(id, adminId, reason);

        /// <inheritdoc/>
        public async Task SubmitProductForReviewAsync(int productId)
            => await _productRepository.SubmitProductForReviewAsync(productId);

        /// <inheritdoc/>
        public async Task<int> CleanupExpiredRejectedProductsAsync(int expirationSeconds = 60)
            => await _productRepository.CleanupExpiredRejectedAsync(expirationSeconds);

        /// <inheritdoc/>
        public async Task<IEnumerable<ProductReviewDto>> GetRecentRejectedProductsAsync(int top = 10)
            => await _productRepository.GetRecentRejectedProductsAsync(top);

        /// <inheritdoc/>
        public async Task<PagedResult<ProductReviewDto>> GetPendingProductsPagedAsync(int page, int pageSize)
        {
            var (items, total) = await _productRepository.GetPendingProductsPagedAsync(page, pageSize);
            return PagedResult<ProductReviewDto>.Create(items.ToList(), total, page, pageSize);
        }

        /// <inheritdoc/>
        public async Task<PagedResult<ProductReviewDto>> GetRejectedProductsPagedAsync(int page, int pageSize)
        {
            var (items, total) = await _productRepository.GetRejectedProductsPagedAsync(page, pageSize);
            return PagedResult<ProductReviewDto>.Create(items.ToList(), total, page, pageSize);
        }

        /// <inheritdoc/>
        public async Task ResetToPendingAsync(int productId)
            => await _productRepository.ResetToPendingAsync(productId);

        /// <inheritdoc/>
        public async Task<(int Total, int Published, int Unpublished, int Pending, int ForcedOffShelf)> GetProductStatusCountsAsync()
            => await _productRepository.GetStatusCountsAsync();

        /// <inheritdoc/>
        public async Task ForceUnpublishAsync(int id, string? reason, int? adminBy)
            => await _productRepository.ForceUnpublishAsync(id, reason, adminBy);

        /// <inheritdoc/>
        public async Task<int> BatchForceOffShelfAsync(List<int> ids, string? reason, int? adminBy)
        {
            if (ids == null || ids.Count == 0) return 0;
            return await _productRepository.BatchForceOffShelfAsync(ids, reason, adminBy);
        }

        /// <inheritdoc/>
        public async Task<PagedResult<ProductReviewDto>> GetReApplyProductsPagedAsync(int page, int pageSize)
        {
            var (items, total) = await _productRepository.GetReApplyProductsPagedAsync(page, pageSize);
            return PagedResult<ProductReviewDto>.Create(items.ToList(), total, page, pageSize);
        }

        /// <inheritdoc/>
        public async Task ReApplyAsync(int id)
            => await _productRepository.ReApplyAsync(id);

        /// <inheritdoc/>
        public async Task SimulateSellerResubmitAsync(int id)
            => await _productRepository.SimulateSellerResubmitAsync(id);

        /// <inheritdoc/>
        public async Task ApproveForcedProductAsync(int id, string adminId)
            => await _productRepository.ApproveForcedProductAsync(id, adminId);

        /// <inheritdoc/>
        public async Task RejectForcedProductAsync(int id, string adminId, string reason)
            => await _productRepository.RejectForcedProductAsync(id, adminId, reason);

        /// <inheritdoc/>
        /// <summary>
        /// 對目前所有待審核商品執行敏感字自動比對。
        /// 審核結果判斷規則：
        ///   Rejected  - 商品名稱含敏感字，或描述中出現敏感字累計 2 次以上
        ///   Uncertain - 描述中僅出現敏感字 1 次（疑似，交人工複審）
        ///   Approved  - 名稱與描述均無敏感字
        /// </summary>
        public async Task<SimulateAutoReviewResult> SimulateAutoReviewAsync()
        {
            // 1. 從 DB 取得所有啟用中的敏感字
            var sensitiveWords = await _sensitiveWordService.GetActiveWordListAsync();

            // 2. 取得所有待審核商品
            var pendingProducts = (await _productRepository.GetPendingProductsAsync()).ToList();

            var result = new SimulateAutoReviewResult();

            foreach (var product in pendingProducts)
            {
                var matchedWords = new List<string>();
                bool nameHit     = false;
                int  descHits    = 0;

                foreach (var word in sensitiveWords)
                {
                    bool inName = product.Name?.Contains(word, StringComparison.OrdinalIgnoreCase) == true;
                    bool inDesc = product.Description?.Contains(word, StringComparison.OrdinalIgnoreCase) == true;

                    if (inName)
                    {
                        nameHit = true;
                        if (!matchedWords.Contains(word)) matchedWords.Add(word);
                    }
                    if (inDesc)
                    {
                        descHits++;
                        if (!matchedWords.Contains(word)) matchedWords.Add(word);
                    }
                }

                var item = new AutoReviewItemResult
                {
                    ProductId    = product.Id,
                    ProductName  = product.Name ?? string.Empty,
                    MatchedWords = matchedWords
                };

                if (nameHit || descHits >= 2)
                {
                    // 嚴重違規 → 直接退回
                    var reason = $"系統自動攔截：內容含違規詞彙【{string.Join("、", matchedWords)}】";
                    await _productRepository.RejectProductAsync(product.Id, "系統自動審核", reason);
                    item.Outcome = "Rejected";
                    result.RejectedCount++;
                }
                else if (descHits == 1)
                {
                    // 疑似 → 維持待審核，由人工複審（不改動 DB 狀態）
                    item.Outcome = "Uncertain";
                    result.ManualReviewCount++;
                }
                else
                {
                    // 清白 → 自動上架
                    await _productRepository.ApproveProductAsync(product.Id, "系統自動審核");
                    item.Outcome = "Approved";
                    result.ApprovedCount++;
                }

                result.Items.Add(item);
            }

            return result;
        }

        /// <inheritdoc/>
        /// <summary>
        /// 生成 15 筆測試用待審核商品，涵蓋三種情境：
        ///   Group A (5 筆) - 乾淨商品，無任何敏感字，預期自動審核通過
        ///   Group B (5 筆) - 高風險：敏感字出現在商品名稱，預期自動審核退回
        ///   Group C (5 筆) - 邊緣：敏感字僅出現在描述一次，預期標記待人工複審
        /// 敏感字從 DB 動態讀取，若 DB 無任何敏感字則使用示範用語。
        /// </summary>
        public async Task<GenerateTestProductsResult> GenerateTestProductsAsync()
        {
            // 1. 從 DB 依風險等級分組取得敏感字
            var (highRiskWords, lowRiskWords) = await _sensitiveWordService.GetActiveWordsByRiskAsync();

            // 安全 fallback：DB 無資料時使用預設字
            if (highRiskWords.Count == 0) highRiskWords = new List<string> { "違禁品", "仿冒品", "詐騙" };
            if (lowRiskWords.Count == 0)  lowRiskWords  = new List<string>(highRiskWords);

            // 2. 從 DB 隨機取得 15 筆真實商品（含商品層級圖片）
            var sourceProducts = await _productRepository.GetRandomProductsWithImagesAsync(15);
            if (sourceProducts.Count == 0)
                throw new InvalidOperationException("資料庫中無任何商品，請先建立至少一筆商品後再使用此功能。");

            // 若取得數量不足 15，允許循環補齊
            var rng = new Random();
            while (sourceProducts.Count < 15)
                sourceProducts.Add(sourceProducts[rng.Next(sourceProducts.Count)]);

            var now      = DateTime.Now;
            var products = new List<Product>();

            // ── Group A（索引 0–4）：5 筆乾淨，保留真實名稱與描述 ─────────────
            // 直接複製真實商品資料（名稱、描述、圖片皆相符），只將狀態改為待審核
            for (int i = 0; i < 5; i++)
            {
                var src = sourceProducts[i];
                products.Add(BuildTestProduct(src, src.Name, src.Description ?? string.Empty, now));
            }

            // ── Group B（索引 5–9）：5 筆高風險，隨機注入高風險敏感字 ──────────
            // 注入策略：
            //   50% → 附加到名稱後綴（名稱命中 = 自動退回）
            //   50% → 在描述中出現兩次（descHits >= 2 = 自動退回）
            for (int i = 0; i < 5; i++)
            {
                var src  = sourceProducts[5 + i];
                var word = highRiskWords[rng.Next(highRiskWords.Count)];
                string name, desc;

                if (rng.Next(2) == 0)
                {
                    // 注入名稱
                    name = $"{src.Name}【{word}】";
                    desc = src.Description ?? string.Empty;
                }
                else
                {
                    // 注入描述兩次（確保 descHits >= 2，觸發自動退回）
                    name = src.Name;
                    desc = $"{src.Description ?? string.Empty} 注意：此商品含有「{word}」成分，附「{word}」使用說明書一份。";
                }

                products.Add(BuildTestProduct(src, name, desc, now));
            }

            // ── Group C（索引 10–14）：5 筆低風險，隨機注入低風險敏感字 ─────────
            // 注入策略：
            //   50% → 附加到名稱（名稱命中 = 自動退回，較高風險的結果）
            //   50% → 僅在描述出現一次（descHits == 1 = 待人工確認 Uncertain）
            for (int i = 0; i < 5; i++)
            {
                var src  = sourceProducts[10 + i];
                var word = lowRiskWords[rng.Next(lowRiskWords.Count)];
                string name, desc;

                if (rng.Next(2) == 0)
                {
                    // 注入名稱
                    name = $"{src.Name}（含{word}相關）";
                    desc = src.Description ?? string.Empty;
                }
                else
                {
                    // 注入描述一次（Uncertain）
                    name = src.Name;
                    desc = $"{src.Description ?? string.Empty} 備註：本商品部分說明含有「{word}」相關文字，僅供參考。";
                }

                products.Add(BuildTestProduct(src, name, desc, now));
            }

            await _productRepository.AddProductsRangeAsync(products);

            // 依 EF Core 自動填入的 ID 查回完整顯示資料（含商店名稱、圖片 URL）
            var createdIds      = products.Select(p => p.Id);
            var createdProducts = await _productRepository.GetProductsByIdsForReviewAsync(createdIds);

            return new GenerateTestProductsResult
            {
                TotalCount      = products.Count,
                CleanCount      = 5,
                HighRiskCount   = 5,
                BorderlineCount = 5,
                CreatedProducts = createdProducts
            };
        }

        /// <summary>建立一筆待審核的測試商品實體，並從來源商品複製商品層級圖片</summary>
        private static Product BuildTestProduct(Product source, string name, string desc, DateTime now)
        {
            var product = new Product
            {
                StoreId      = source.StoreId,
                CategoryId   = source.CategoryId,
                BrandId      = source.BrandId,
                Name         = name,
                Description  = desc,
                MinPrice     = source.MinPrice,
                MaxPrice     = source.MaxPrice,
                Status       = 2,   // 待審核
                ReviewStatus = 0,   // 待審核
                ReviewedBy   = null,
                RejectReason = null,
                ReviewDate   = null,
                CreatedAt    = now,
                UpdatedAt    = now,
                IsDeleted    = false
            };

            // 複製來源商品的圖片（只複製商品層級圖片，排除規格圖）
            foreach (var img in source.ProductImages.Where(i => i.VariantId == null))
            {
                product.ProductImages.Add(new ProductImage
                {
                    ImageUrl  = img.ImageUrl,
                    IsMain    = img.IsMain,
                    SortOrder = img.SortOrder,
                    VariantId = null
                });
            }

            return product;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ProductReviewDto>> GetRecentlyApprovedAsync(int hours = 24)
            => await _productRepository.GetRecentlyApprovedProductsAsync(hours);

        /// <inheritdoc/>
        public async Task<PagedResult<ProductReviewDto>> GetRecentlyApprovedPagedAsync(int page, int pageSize, int hours = 24)
        {
            var (items, total) = await _productRepository.GetRecentlyApprovedProductsPagedAsync(page, pageSize, hours);
            return PagedResult<ProductReviewDto>.Create(items.ToList(), total, page, pageSize);
        }

        // ═══════════════════════════════════════════════════════════
        //  前台商品列表
        // ═══════════════════════════════════════════════════════════

        /// <inheritdoc/>
        public async Task<PagedResult<ProductListDto>> GetFrontActiveProductsAsync(
            int? categoryId, string? keyword, string sortBy, int page, int pageSize,
            int? subCategoryId = null, int[]? brandIds = null,
            decimal? minPrice = null, decimal? maxPrice = null)
        {
            pageSize = Math.Clamp(pageSize, 1, 50);
            var (items, total) = await _productRepository.GetFrontActiveProductsAsync(
                categoryId, keyword, sortBy, page, pageSize,
                subCategoryId, brandIds, minPrice, maxPrice);
            return PagedResult<ProductListDto>.Create(items.ToList(), total, page, pageSize);
        }

        // ═══════════════════════════════════════════════════════════
        //  前台商品詳情頁
        // ═══════════════════════════════════════════════════════════

        /// <inheritdoc/>
        public async Task<(ISpanShop.Models.EfModels.Product? Product, decimal? Rating, int ReviewCount, int StoreProductCount, decimal? StoreRating)>
            GetProductDetailAsync(int id)
        {
            var product = await _productRepository.GetProductDetailAsync(id);

            // 找不到、已刪除、非上架狀態、或賣家被停權 → 回傳 null
            if (product == null || product.Status != 1 || product.Store?.StoreStatus == 3 || product.Store?.User?.IsBlacklisted == true)
                return (null, null, 0, 0, null);

            var (rating, reviewCount) = await _productRepository.GetProductRatingAsync(id);
            var storeCount = await _productRepository.GetStoreActiveProductCountAsync(product.StoreId);
            var storeRating = await _productRepository.GetStoreRatingAsync(product.StoreId);

            return (product, rating, reviewCount, storeCount, storeRating);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ProductListDto>> GetRelatedProductsAsync(
            int productId, int categoryId, int limit)
        {
            limit = Math.Clamp(limit, 1, 50);
            return await _productRepository.GetRelatedProductsAsync(productId, categoryId, limit);
        }

        /// <inheritdoc/>
        public async Task<List<string>> GetHotKeywordsAsync(int limit = 8)
        {
            limit = Math.Clamp(limit, 1, 20);
            return await _productRepository.GetHotKeywordsAsync(limit);
        }

        /// <inheritdoc/>
        public Task IncrementViewCountAsync(int productId)
            => _productRepository.IncrementViewCountAsync(productId);

        /// <inheritdoc/>
        public void AddProductImages(int productId, IEnumerable<ISpanShop.Models.EfModels.ProductImage> images)
        {
            _productRepository.AddProductImages(productId, images);
        }

        /// <inheritdoc/>
        public void DeleteProductImages(int productId, string webRootPath)
        {
            _productRepository.DeleteProductImages(productId, webRootPath);
        }

        /// <inheritdoc/>
        public void DeleteProductImagesExcept(int productId, List<string> keepImageUrls, string webRootPath)
        {
            _productRepository.DeleteProductImagesExcept(productId, keepImageUrls, webRootPath);
        }

        /// <inheritdoc/>
        public void UpdateMainImage(int productId, int mainImageIndex)
        {
            _productRepository.UpdateMainImage(productId, mainImageIndex);
        }
    }
}
