using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ISpanShop.Models.DTOs.Products;
using ISpanShop.Models.DTOs.Common;
using ISpanShop.Models.EfModels;
using ISpanShop.Repositories.Products;

namespace ISpanShop.Repositories.Products
{
    /// <summary>
    /// 商品 Repository 實作 - 處理商品的 CRUD 操作
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly ISpanShopDBContext _context;
        private readonly ILogger<ProductRepository> _logger;

        /// <summary>
        /// 建構子 - 注入 DbContext 和 Logger
        /// </summary>
        public ProductRepository(ISpanShopDBContext context, ILogger<ProductRepository> logger)
        {
            _context = context;
            _logger  = logger;
        }

        /// <summary>
        /// 新增商品
        /// </summary>
        public void AddProduct(Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
        }

        /// <summary>
        /// 檢查 SKU 代碼是否已存在
        /// </summary>
        public bool IsSkuExists(string skuCode)
            => _context.ProductVariants.Any(pv => pv.SkuCode == skuCode);

        /// <summary>
        /// 更新商品狀態
        /// </summary>
        public void UpdateProductStatus(int id, byte status)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                product.Status = status;
                product.UpdatedAt = DateTime.Now;
                _context.SaveChanges();
            }
        }

        /// <summary>
        /// 根據 ID 取得商品詳情（包含圖片與規格）
        /// </summary>
        /// <param name="id">商品 ID</param>
        /// <returns>商品實體，若不存在則返回 null</returns>
        public Product? GetProductById(int id)
        {
            return _context.Products
                .Include(p => p.Store)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .FirstOrDefault(p => p.Id == id);
        }

        /// <summary>
        /// 取得所有分類（含父子關聯）
        /// </summary>
        /// <returns>所有分類集合</returns>
        public IEnumerable<Category> GetAllCategories()
        {
            return _context.Categories.ToList();
        }

        /// <summary>
        /// 取得所有商家清單 (Id, StoreName)
        /// </summary>
        /// <returns>商家清單</returns>
        public IEnumerable<(int Id, string Name)> GetStoreOptions()
        {
            return _context.Stores
                .Select(s => new { s.Id, s.StoreName })
                .ToList()
                .Select(s => (s.Id, s.StoreName));
        }
        /// <summary>
        /// 取得所有品牌清單 (Id, Name)
        /// </summary>
        /// <returns>品牌清單</returns>
        public IEnumerable<(int Id, string Name)> GetBrandOptions()
        {
            return _context.Brands
                .Where(b => b.IsDeleted != true)
                .Select(b => new { b.Id, b.Name })
                .ToList()
                .Select(b => (b.Id, b.Name));
        }
        /// <summary>
        /// 根據子分類取得該分類下商品涵蓋的品牌清單
        /// </summary>
        /// <param name="categoryId">子分類 ID；為 null 時回傳全部品牌</param>
        /// <returns>品牌清單（依名稱排序）</returns>
        public IEnumerable<(int Id, string Name)> GetBrandsByCategory(int? categoryId)
        {
            return _context.Products
                .Where(p => categoryId == null || p.CategoryId == categoryId)
                .Where(p => p.BrandId != null && p.Brand != null && p.Brand.IsDeleted != true)
                .Select(p => new { p.Brand!.Id, p.Brand!.Name })
                .Distinct()
                .OrderBy(b => b.Name)
                .ToList()
                .Select(b => (b.Id, b.Name));
        }

        /// <summary>
        /// 批次更新商品上下架狀態
        /// </summary>
        public async Task<int> UpdateBatchStatusAsync(List<int> productIds, byte targetStatus)
        {
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            foreach (var product in products)
            {
                product.Status    = targetStatus;
                product.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return products.Count;
        }

        /// <summary>
        /// 批次更新商品審核狀態
        /// </summary>
        public async Task<int> UpdateBatchReviewStatusAsync(List<int> productIds, int targetReviewStatus, string adminId)
        {
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            foreach (var product in products)
            {
                product.ReviewStatus = targetReviewStatus;
                product.ReviewedBy   = adminId;
                product.ReviewDate   = DateTime.Now;
                product.UpdatedAt    = DateTime.Now;

                // 審核通過 → 狀態改為上架；退回 → 狀態改為審核退回
                if (targetReviewStatus == 1)
                    product.Status = 1; // 上架
                else if (targetReviewStatus == 2)
                    product.Status = 3; // 審核退回
            }

            await _context.SaveChangesAsync();
            return products.Count;
        }

        /// <summary>
        /// 分頁取得商品列表，支援分類篩選與多維度搜尋
        /// </summary>
        public (IEnumerable<Product> Items, int TotalCount) GetProductsPaged(ProductSearchCriteria criteria)
        {
            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.Store)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .AsQueryable();

            if (!criteria.IncludeDeleted)
                query = query.Where(p => p.IsDeleted != true);

            if (criteria.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == criteria.CategoryId.Value);
            else if (criteria.ParentCategoryId.HasValue)
                query = query.Where(p => p.Category.ParentId == criteria.ParentCategoryId.Value);

            if (!string.IsNullOrWhiteSpace(criteria.Keyword))
            {
                var kw = criteria.Keyword.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(kw) ||
                    (p.Description != null && p.Description.ToLower().Contains(kw)) ||
                    (p.Store != null && p.Store.StoreName.ToLower().Contains(kw)) ||
                    (p.Brand != null && p.Brand.Name.ToLower().Contains(kw)) ||
                    (p.Category != null && p.Category.Name.ToLower().Contains(kw)));
            }

            if (criteria.StoreId.HasValue)
                query = query.Where(p => p.StoreId == criteria.StoreId.Value);

            if (criteria.BrandId.HasValue)
                query = query.Where(p => p.BrandId == criteria.BrandId.Value);

            if (criteria.Status.HasValue)
                query = query.Where(p => p.Status == criteria.Status.Value);
            else if (!criteria.StoreId.HasValue)
                // 只在「平台商品總覽」時過濾：已退回商品只在審核中心的近期退回紀錄顯示
                // 但賣家查詢自己的商品時（有 StoreId），不過濾任何狀態，讓賣家看到所有商品
                query = query.Where(p => p.Status != 2 && p.Status != 3);

            if (criteria.StartDate.HasValue)
                query = query.Where(p => p.CreatedAt >= criteria.StartDate.Value);

            if (criteria.EndDate.HasValue)
            {
                var endOfDay = criteria.EndDate.Value.AddDays(1).AddTicks(-1);
                query = query.Where(p => p.CreatedAt <= endOfDay);
            }

            query = criteria.SortOrder switch
            {
                "name_asc"     => query.OrderBy(p => p.Name),
                "name_desc"    => query.OrderByDescending(p => p.Name),
                "price_asc"    => query.OrderBy(p => p.MinPrice),
                "price_desc"   => query.OrderByDescending(p => p.MinPrice),
                "status_asc"   => query.OrderBy(p => p.Status),
                "status_desc"  => query.OrderByDescending(p => p.Status),
                "date_asc"     => query.OrderBy(p => p.CreatedAt),
                "review_desc"  => query.OrderByDescending(p => p.ReviewDate),
                "stock_desc"   => query.OrderByDescending(p => p.ProductVariants.Where(v => v.IsDeleted != true).Sum(v => (int?)v.Stock ?? 0)),
                "stock_asc"    => query.OrderBy(p => p.ProductVariants.Where(v => v.IsDeleted != true).Sum(v => (int?)v.Stock ?? 0)),
                "sales_desc"   => query.OrderByDescending(p => p.TotalSales ?? 0),
                "updated_desc" => query.OrderByDescending(p => p.UpdatedAt),
                "date_desc"    => query.OrderByDescending(p => p.CreatedAt),
                _              => query.OrderBy(p => p.MinPrice)
            };

            int totalCount = query.Count();
            var items = query
                .Skip((criteria.PageNumber - 1) * criteria.PageSize)
                .Take(criteria.PageSize)
                .ToList();

            return (items, totalCount);
        }

        public void UpdateProduct(ProductUpdateDto dto)
        {
            var product = _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefault(p => p.Id == dto.Id);
            if (product == null) return;

            var originalStatus = product.Status;

            product.Name               = dto.Name;
            product.Description        = dto.Description;
            product.CategoryId         = dto.CategoryId;
            product.BrandId            = dto.BrandId;
            product.SpecDefinitionJson = dto.SpecDefinitionJson;
            product.UpdatedAt          = DateTime.Now;

            if (originalStatus == 3) // 審核退回 → 重新送審
            {
                product.Status       = 2;           // 待審核
                product.ReviewStatus = 3;           // 重新申請審核
                product.ReApplyDate  = DateTime.Now;
                product.ReviewedBy   = null;
                product.ReviewDate   = null;
                // 保留 RejectReason，讓後台知道上次退回原因
            }
            // 已上架(1) 或未上架(0)：直接更新內容，狀態與審核記錄維持不變

            if (!string.IsNullOrWhiteSpace(dto.MainImageUrl))
            {
                var mainImg = product.ProductImages.FirstOrDefault(i => i.IsMain == true);
                if (mainImg != null)
                    mainImg.ImageUrl = dto.MainImageUrl;
                else
                    product.ProductImages.Add(new ProductImage
                    {
                        ImageUrl = dto.MainImageUrl, IsMain = true, SortOrder = 0
                    });
            }
            _context.SaveChanges();
        }

        public void SoftDeleteProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return;
            product.IsDeleted = true;
            product.UpdatedAt = DateTime.Now;
            _context.SaveChanges();
        }

        public ProductVariant? GetVariantById(int id)
            => _context.ProductVariants.Include(v => v.Product).FirstOrDefault(v => v.Id == id);

        public void AddVariant(ProductVariant variant)
        {
            _context.ProductVariants.Add(variant);
            _context.SaveChanges();
            RecalcProductPrice(variant.ProductId);
        }

        public void UpdateVariant(ProductVariantUpdateDto dto)
        {
            var variant = _context.ProductVariants.Find(dto.Id);
            if (variant == null) return;
            variant.SkuCode     = dto.SkuCode;
            variant.Price       = dto.Price;
            variant.Stock       = dto.Stock;
            variant.SafetyStock = dto.SafetyStock;
            _context.SaveChanges();
            RecalcProductPrice(variant.ProductId);
        }

        public void SoftDeleteVariant(int id)
        {
            var variant = _context.ProductVariants.Find(id);
            if (variant == null) return;
            variant.IsDeleted = true;
            _context.SaveChanges();
            RecalcProductPrice(variant.ProductId);
        }

        private void RecalcProductPrice(int productId)
        {
            var product = _context.Products.Find(productId);
            if (product == null) return;
            var prices = _context.ProductVariants
                .Where(v => v.ProductId == productId && v.IsDeleted != true)
                .Select(v => v.Price)
                .ToList();
            if (prices.Any())
            {
                product.MinPrice = prices.Min();
                product.MaxPrice = prices.Max();
            }
            product.UpdatedAt = DateTime.Now;
            _context.SaveChanges();
        }

        // ═══════════════════════════════════════════════════════════
        //  非同步實作（async/await + 投影 + 真分頁）
        // ═══════════════════════════════════════════════════════════

        /// <inheritdoc/>
        public async Task<(IEnumerable<ProductListDto> Items, int TotalCount)>
            GetProductsPagedAsync(ProductSearchCriteria criteria)
        {
            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.Store)
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Where(p => p.IsDeleted != true)
                .AsQueryable();

            if (criteria.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == criteria.CategoryId.Value);
            else if (criteria.ParentCategoryId.HasValue)
                query = query.Where(p => p.Category.ParentId == criteria.ParentCategoryId.Value);

            if (!string.IsNullOrWhiteSpace(criteria.Keyword))
            {
                var keyword = criteria.Keyword.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(keyword) ||
                    (p.Description != null && p.Description.ToLower().Contains(keyword)) ||
                    (p.Store != null && p.Store.StoreName.ToLower().Contains(keyword)) ||
                    (p.Brand != null && p.Brand.Name.ToLower().Contains(keyword)) ||
                    (p.Category != null && p.Category.Name.ToLower().Contains(keyword)));
            }

            if (criteria.StoreId.HasValue)
                query = query.Where(p => p.StoreId == criteria.StoreId.Value);

            if (criteria.BrandId.HasValue)
                query = query.Where(p => p.BrandId == criteria.BrandId.Value);

            if (criteria.Status.HasValue)
            {
                var statusByte = (byte)criteria.Status.Value;
                query = query.Where(p => p.Status == statusByte);
            }
            else if (!criteria.StoreId.HasValue)
            {
                // 只在「平台商品總覽」時過濾：已退回商品只在審核中心的近期退回紀錄顯示
                // 但賣家查詢自己的商品時（有 StoreId），不過濾任何狀態，讓賣家看到所有商品
                query = query.Where(p => p.Status != 2 && p.Status != 3);
            }

            if (criteria.StartDate.HasValue)
                query = query.Where(p => p.CreatedAt >= criteria.StartDate.Value);

            if (criteria.EndDate.HasValue)
            {
                var endOfDay = criteria.EndDate.Value.AddDays(1).AddTicks(-1);
                query = query.Where(p => p.CreatedAt <= endOfDay);
            }

            query = criteria.SortOrder switch
            {
                "name_asc"     => query.OrderBy(p => p.Name),
                "name_desc"    => query.OrderByDescending(p => p.Name),
                "price_asc"    => query.OrderBy(p => p.MinPrice),
                "price_desc"   => query.OrderByDescending(p => p.MinPrice),
                "status_asc"   => query.OrderBy(p => p.Status),
                "status_desc"  => query.OrderByDescending(p => p.Status),
                "date_asc"     => query.OrderBy(p => p.CreatedAt),
                "review_desc"  => query.OrderByDescending(p => p.ReviewDate),
                "stock_desc"   => query.OrderByDescending(p => p.ProductVariants.Where(v => v.IsDeleted != true).Sum(v => (int?)v.Stock ?? 0)),
                "stock_asc"    => query.OrderBy(p => p.ProductVariants.Where(v => v.IsDeleted != true).Sum(v => (int?)v.Stock ?? 0)),
                "sales_desc"   => query.OrderByDescending(p => p.TotalSales ?? 0),
                "updated_desc" => query.OrderByDescending(p => p.UpdatedAt),
                "date_desc"    => query.OrderByDescending(p => p.CreatedAt),
                _              => query.OrderBy(p => p.MinPrice)
            };

            // COUNT 在 SQL 端完成
            int totalCount = await query.CountAsync();

            // Skip/Take 接在 IQueryable 後 → SQL 端分頁；Select 投影只取所需欄位
            var items = await query
                .Skip((criteria.PageNumber - 1) * criteria.PageSize)
                .Take(criteria.PageSize)
                .Select(p => new ProductListDto
                {
                    Id           = p.Id,
                    StoreName    = p.Store != null ? p.Store.StoreName : "未知商店",
                    CategoryName = p.Category != null ? p.Category.Name : "未分類",
                    BrandName    = p.Brand != null ? p.Brand.Name : "未設定",
                    Name                = p.Name,
                    MinPrice            = p.MinPrice,
                    MaxPrice            = p.MaxPrice,
                    Status              = p.Status,
                    CreatedAt           = p.CreatedAt,
                    ReviewStatus        = p.ReviewStatus,
                    ReviewedBy          = p.ReviewedBy,
                    ReviewDate          = p.ReviewDate,
                    RejectReason        = p.RejectReason,
                    ForceOffShelfReason = p.ForceOffShelfReason,
                    ReApplyDate         = p.ReApplyDate,
                    MainImageUrl =
                        p.ProductImages.Where(img => img.IsMain == true)
                                       .Select(img => img.ImageUrl).FirstOrDefault()
                        ?? p.ProductImages.Select(img => img.ImageUrl).FirstOrDefault()
                        ?? "https://via.placeholder.com/400x400?text=No+Image"
                })
                .ToListAsync();

            return (items, totalCount);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ProductReviewDto>> GetPendingProductsAsync()
        {
            return await _context.Products
                .AsNoTracking()
                .Where(p => p.ReviewStatus == 0 && p.IsDeleted != true)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new ProductReviewDto
                {
                    Id           = p.Id,
                    StoreId      = p.StoreId,
                    CategoryName = p.Category != null ? p.Category.Name : "未分類",
                    BrandName    = p.Brand != null ? p.Brand.Name : "未設定",
                    StoreName    = p.Store != null ? p.Store.StoreName : "未知商店",
                    Name         = p.Name,
                    Description  = p.Description,
                    Status       = p.Status ?? 0,
                    ReviewStatus = p.ReviewStatus,
                    ReviewedBy   = p.ReviewedBy,
                    ReviewDate   = p.ReviewDate,
                    CreatedAt    = p.CreatedAt,
                    UpdatedAt    = p.UpdatedAt,
                    MainImageUrl = p.ProductImages
                        .Where(img => img.IsMain == true)
                        .Select(img => img.ImageUrl).FirstOrDefault()
                })
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<(IEnumerable<ProductReviewDto> Items, int TotalCount)>
            GetPendingProductsPagedAsync(int page, int pageSize)
        {
            var query = _context.Products
                .AsNoTracking()
                .Where(p => p.ReviewStatus == 0 && p.IsDeleted != true)
                .OrderByDescending(p => p.CreatedAt);

            int total = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductReviewDto
                {
                    Id           = p.Id,
                    StoreId      = p.StoreId,
                    CategoryName = p.Category != null ? p.Category.Name : "未分類",
                    BrandName    = p.Brand != null ? p.Brand.Name : "未設定",
                    StoreName    = p.Store != null ? p.Store.StoreName : "未知商店",
                    Name         = p.Name,
                    Description  = p.Description,
                    Status       = p.Status ?? 0,
                    ReviewStatus = p.ReviewStatus,
                    ReviewedBy   = p.ReviewedBy,
                    ReviewDate   = p.ReviewDate,
                    CreatedAt    = p.CreatedAt,
                    UpdatedAt    = p.UpdatedAt,
                    MainImageUrl = p.ProductImages
                        .Where(img => img.IsMain == true)
                        .Select(img => img.ImageUrl).FirstOrDefault()
                })
                .ToListAsync();

            return (items, total);
        }

        /// <inheritdoc/>
        public async Task<(IEnumerable<ProductReviewDto> Items, int TotalCount)>
            GetRejectedProductsPagedAsync(int page, int pageSize)
        {
            var query = _context.Products
                .AsNoTracking()
                .Where(p => p.ReviewStatus == 2 && p.IsDeleted != true)
                .OrderByDescending(p => p.ReviewDate ?? p.UpdatedAt);

            int total = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductReviewDto
                {
                    Id           = p.Id,
                    StoreId      = p.StoreId,
                    CategoryName = p.Category != null ? p.Category.Name : "未分類",
                    BrandName    = p.Brand != null ? p.Brand.Name : "未設定",
                    StoreName    = p.Store != null ? p.Store.StoreName : "未知商店",
                    Name         = p.Name,
                    Description  = p.Description,
                    Status       = p.Status ?? 0,
                    ReviewStatus = p.ReviewStatus,
                    ReviewedBy   = p.ReviewedBy,
                    ReviewDate   = p.ReviewDate,
                    RejectReason        = p.RejectReason,
                    ForceOffShelfReason = p.ForceOffShelfReason,
                    CreatedAt    = p.CreatedAt,
                    UpdatedAt    = p.UpdatedAt,
                    MainImageUrl = p.ProductImages
                        .Where(img => img.IsMain == true)
                        .Select(img => img.ImageUrl).FirstOrDefault()
                })
                .ToListAsync();

            return (items, total);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ProductReviewDto>> GetRecentRejectedProductsAsync(int top)
        {
            return await _context.Products
                .AsNoTracking()
                .Where(p => p.ReviewStatus == 2 && p.IsDeleted != true)
                .OrderByDescending(p => p.UpdatedAt)
                .Take(top)
                .Select(p => new ProductReviewDto
                {
                    Id           = p.Id,
                    StoreId      = p.StoreId,
                    CategoryName = p.Category != null ? p.Category.Name : "未分類",
                    BrandName    = p.Brand != null ? p.Brand.Name : "未設定",
                    StoreName    = p.Store != null ? p.Store.StoreName : "未知商店",
                    Name         = p.Name,
                    Description  = p.Description,
                    Status       = p.Status ?? 0,
                    ReviewStatus = p.ReviewStatus,
                    ReviewedBy   = p.ReviewedBy,
                    ReviewDate   = p.ReviewDate,
                    RejectReason        = p.RejectReason,
                    ForceOffShelfReason = p.ForceOffShelfReason,
                    CreatedAt    = p.CreatedAt,
                    UpdatedAt    = p.UpdatedAt,
                    MainImageUrl = p.ProductImages
                        .Where(img => img.IsMain == true)
                        .Select(img => img.ImageUrl).FirstOrDefault()
                })
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task ApproveProductAsync(int id, string adminId)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return;

            product.Status       = 1;
            product.ReviewStatus = 1;
            product.ReviewedBy   = adminId;
            product.RejectReason = null;
            product.ReviewDate   = DateTime.Now;
            product.UpdatedAt    = DateTime.Now;
            if (product.Name != null && product.Name.StartsWith("[待審核]"))
                product.Name = product.Name.Substring("[待審核]".Length).TrimStart();

            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task RejectProductAsync(int id, string adminId, string reason)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return;

            product.Status       = 3;
            product.ReviewStatus = 2;
            product.ReviewedBy   = adminId;
            product.RejectReason = reason;
            product.ReviewDate   = DateTime.Now;
            product.UpdatedAt    = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task SubmitProductForReviewAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return;

            // 僅將商品設為「待審核」狀態，實際敏感字比對由 Service 層統一執行
            product.Status       = 2;
            product.ReviewStatus = 0;
            product.ReviewedBy   = null;
            product.RejectReason = null;
            product.ReviewDate   = null;
            product.UpdatedAt    = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<int> CleanupExpiredRejectedAsync(int expirationSeconds)
        {
            var cutoff = DateTime.Now.AddSeconds(-expirationSeconds);
            var expired = await _context.Products
                .Where(p => p.ReviewStatus == 2
                         && p.ReviewDate != null
                         && p.ReviewDate <= cutoff
                         && p.IsDeleted != true)
                .ToListAsync();

            foreach (var p in expired)
            {
                p.IsDeleted = true;
                p.UpdatedAt = DateTime.Now;
            }
            await _context.SaveChangesAsync();
            return expired.Count;
        }

        /// <inheritdoc/>
        public async Task ResetToPendingAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return;

            product.Status       = 2;
            product.ReviewStatus = 0;
            product.ReviewedBy   = null;
            product.ReviewDate   = null;
            product.RejectReason = null;
            product.UpdatedAt    = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<(int Total, int Published, int Unpublished, int Pending, int ForcedOffShelf)> GetStatusCountsAsync()
        {
            var q = _context.Products.Where(p => p.IsDeleted != true);
            return (
                await q.CountAsync(),
                await q.CountAsync(p => p.Status == 1),
                await q.CountAsync(p => p.Status == 0),   // 一般下架（不含強制下架）
                await q.CountAsync(p => p.ReviewStatus == 0),
                await q.CountAsync(p => p.Status == 4)    // 強制下架
            );
        }

        /// <inheritdoc/>
        public async Task ForceUnpublishAsync(int id, string? reason, int? adminBy)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return;

            product.Status              = 4;              // 強制下架（區別於一般下架 0）
            product.ForceOffShelfReason = reason;
            product.ForceOffShelfDate   = DateTime.Now;
            product.ForceOffShelfBy     = adminBy;
            product.ReviewStatus        = 2;              // 進入退回紀錄，賣家可從此處模擬送審
            product.UpdatedAt           = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<int> BatchForceOffShelfAsync(List<int> ids, string? reason, int? adminBy)
        {
            var products = await _context.Products
                .Where(p => ids.Contains(p.Id) && p.Status == 1 && p.IsDeleted != true)
                .ToListAsync();

            var now = DateTime.Now;
            foreach (var product in products)
            {
                product.Status              = 4;
                product.ForceOffShelfReason = reason;
                product.ForceOffShelfDate   = now;
                product.ForceOffShelfBy     = adminBy;
                product.ReviewStatus        = 2; // 2 = 已退回/拒絕
                product.UpdatedAt           = now;
            }

            await _context.SaveChangesAsync();
            return products.Count;
        }

        /// <inheritdoc/>
        public async Task<(IEnumerable<ProductReviewDto> Items, int TotalCount)>
            GetReApplyProductsPagedAsync(int page, int pageSize)
        {
            var query = _context.Products
                .AsNoTracking()
                .Where(p => p.ReviewStatus == 3 && p.IsDeleted != true)
                .OrderByDescending(p => p.ReApplyDate ?? p.UpdatedAt);

            int total = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductReviewDto
                {
                    Id                  = p.Id,
                    StoreId             = p.StoreId,
                    CategoryName        = p.Category != null ? p.Category.Name : "未分類",
                    BrandName           = p.Brand    != null ? p.Brand.Name    : "未設定",
                    StoreName           = p.Store    != null ? p.Store.StoreName : "未知商店",
                    Name                = p.Name,
                    Description         = p.Description,
                    Status              = p.Status ?? 0,
                    ReviewStatus        = p.ReviewStatus,
                    ReviewedBy          = p.ReviewedBy,
                    ReviewDate          = p.ReviewDate,
                    RejectReason        = p.RejectReason,
                    ForceOffShelfReason = p.ForceOffShelfReason,
                    ForceOffShelfDate   = p.ForceOffShelfDate,
                    ForceOffShelfBy     = p.ForceOffShelfBy,
                    ReApplyDate         = p.ReApplyDate,
                    CreatedAt           = p.CreatedAt,
                    UpdatedAt           = p.UpdatedAt,
                    MainImageUrl        = p.ProductImages
                        .Where(img => img.IsMain == true)
                        .Select(img => img.ImageUrl).FirstOrDefault()
                })
                .ToListAsync();

            return (items, total);
        }

        /// <inheritdoc/>
        public async Task ReApplyAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null || product.Status != 4) return;

            product.ReviewStatus = 3;
            product.ReApplyDate  = DateTime.Now;
            product.UpdatedAt    = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task SimulateSellerResubmitAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            // 只對 ReviewStatus=2（已退回）的商品有效
            if (product == null || product.ReviewStatus != 2) return;

            product.ReviewStatus = 3;           // 待重新審核
            product.ReApplyDate  = DateTime.Now;
            product.UpdatedAt    = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task ApproveForcedProductAsync(int id, string adminId)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return;

            product.Status       = 1;    // 上架
            product.ReviewStatus = 1;    // 審核通過
            product.ReviewedBy   = adminId;
            product.ReviewDate   = DateTime.Now;
            product.RejectReason = null;
            product.UpdatedAt    = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task RejectForcedProductAsync(int id, string adminId, string reason)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return;

            product.Status       = 4;    // 維持強制下架
            product.ReviewStatus = 2;    // 駁回
            product.ReviewedBy   = adminId;
            product.ReviewDate   = DateTime.Now;
            product.RejectReason = reason;
            product.UpdatedAt    = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<Product?> GetFirstActiveProductAsync()
        {
            return await _context.Products
                .Where(p => p.IsDeleted != true)
                .OrderBy(p => p.Id)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public async Task AddProductsRangeAsync(IEnumerable<Product> products)
        {
            _context.Products.AddRange(products);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ProductReviewDto>> GetRecentlyApprovedProductsAsync(int hours = 24)
        {
            var since = DateTime.Now.AddHours(-hours);

            return await _context.Products
                .AsNoTracking()
                .Where(p => p.ReviewStatus == 1
                         && p.ReviewDate != null
                         && p.ReviewDate >= since
                         && p.IsDeleted != true)
                .OrderByDescending(p => p.ReviewDate)
                .Select(p => new ProductReviewDto
                {
                    Id           = p.Id,
                    StoreId      = p.StoreId,
                    CategoryName = p.Category != null ? p.Category.Name : "未分類",
                    BrandName    = p.Brand    != null ? p.Brand.Name    : "未設定",
                    StoreName    = p.Store    != null ? p.Store.StoreName : "未知商店",
                    Name         = p.Name,
                    Description  = p.Description,
                    Status       = p.Status ?? 0,
                    ReviewStatus = p.ReviewStatus,
                    ReviewedBy   = p.ReviewedBy,
                    ReviewDate   = p.ReviewDate,
                    CreatedAt    = p.CreatedAt,
                    UpdatedAt    = p.UpdatedAt,
                    MainImageUrl = p.ProductImages
                        .Where(img => img.IsMain == true)
                        .Select(img => img.ImageUrl).FirstOrDefault()
                })
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<(IEnumerable<ProductReviewDto> Items, int TotalCount)> GetRecentlyApprovedProductsPagedAsync(int page, int pageSize, int hours = 24)
        {
            var since = DateTime.Now.AddHours(-hours);

            var query = _context.Products
                .AsNoTracking()
                .Where(p => p.ReviewStatus == 1
                         && p.ReviewDate != null
                         && p.ReviewDate >= since
                         && p.IsDeleted != true);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(p => p.ReviewDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductReviewDto
                {
                    Id           = p.Id,
                    StoreId      = p.StoreId,
                    CategoryName = p.Category != null ? p.Category.Name : "未分類",
                    BrandName    = p.Brand    != null ? p.Brand.Name    : "未設定",
                    StoreName    = p.Store    != null ? p.Store.StoreName : "未知商店",
                    Name         = p.Name,
                    Description  = p.Description,
                    Status       = p.Status ?? 0,
                    ReviewStatus = p.ReviewStatus,
                    ReviewedBy   = p.ReviewedBy,
                    ReviewDate   = p.ReviewDate,
                    CreatedAt    = p.CreatedAt,
                    UpdatedAt    = p.UpdatedAt,
                    MainImageUrl = p.ProductImages
                        .Where(img => img.IsMain == true)
                        .Select(img => img.ImageUrl).FirstOrDefault()
                })
                .ToListAsync();

            return (items, total);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ProductReviewDto>> GetProductsByIdsForReviewAsync(IEnumerable<int> ids)
        {
            var idList = ids.ToList();
            return await _context.Products
                .AsNoTracking()
                .Where(p => idList.Contains(p.Id))
                .Select(p => new ProductReviewDto
                {
                    Id           = p.Id,
                    StoreId      = p.StoreId,
                    StoreName    = p.Store != null ? p.Store.StoreName : "未知商店",
                    CategoryName = p.Category != null ? p.Category.Name : "未分類",
                    BrandName    = p.Brand != null ? p.Brand.Name : "未設定",
                    Name         = p.Name,
                    ReviewStatus = p.ReviewStatus,
                    CreatedAt    = p.CreatedAt,
                    MainImageUrl = p.ProductImages
                        .Where(img => img.IsMain == true)
                        .Select(img => img.ImageUrl).FirstOrDefault()
                })
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<Product>> GetRandomProductsWithImagesAsync(int count)
        {
            // 優先取有圖片的商品
            var idsWithImages = await _context.Products
                .AsNoTracking()
                .Where(p => p.IsDeleted != true && p.ProductImages.Any(img => img.VariantId == null))
                .Select(p => p.Id)
                .ToListAsync();

            if (idsWithImages.Count == 0)
            {
                // Fallback：取任意商品
                idsWithImages = await _context.Products
                    .AsNoTracking()
                    .Where(p => p.IsDeleted != true)
                    .Select(p => p.Id)
                    .ToListAsync();
            }

            if (idsWithImages.Count == 0)
                return new List<Product>();

            var rng = new Random();
            var selectedIds = idsWithImages.OrderBy(_ => rng.Next()).Take(count).ToList();

            return await _context.Products
                .AsNoTracking()
                .Where(p => selectedIds.Contains(p.Id))
                .Include(p => p.ProductImages)
                .ToListAsync();
        }

        // ═══════════════════════════════════════════════════════════
        //  前台商品列表（只查 Status==1 上架中）
        // ═══════════════════════════════════════════════════════════

        /// <inheritdoc/>
        public async Task<(IEnumerable<ProductListDto> Items, int TotalCount)> GetFrontActiveProductsAsync(
            int? categoryId, string? keyword, string sortBy, int page, int pageSize,
            int? subCategoryId = null, int[]? brandIds = null,
            decimal? minPrice = null, decimal? maxPrice = null)
        {
            var query = _context.Products
                .AsNoTracking()
                .Where(p => p.IsDeleted != true && p.Status == 1)
                .AsQueryable();

            // ── 分類篩選 ───────────────────────────────────────────
            // subCategoryId 優先（直接篩，不做展開）
            if (subCategoryId.HasValue)
            {
                _logger.LogInformation(
                    "[FrontProducts] 收到 subCategoryId={SubCategoryId}（直接篩）", subCategoryId.Value);
                query = query.Where(p => p.CategoryId == subCategoryId.Value);
            }
            else if (categoryId.HasValue)
            {
                _logger.LogInformation(
                    "[FrontProducts] 收到 categoryId={CategoryId}", categoryId.Value);

                // SQL-1：一次撈出「該分類本身 + 它的所有直接子分類」
                var categoryRows = await _context.Categories
                    .AsNoTracking()
                    .Where(c => c.Id == categoryId.Value || c.ParentId == categoryId.Value)
                    .Select(c => new { c.Id, c.ParentId })
                    .ToListAsync();

                bool isMainCategory = categoryRows.Any(c => c.Id == categoryId.Value && c.ParentId == null);
                var  subIds         = categoryRows.Where(c => c.ParentId == categoryId.Value)
                                                  .Select(c => c.Id)
                                                  .ToList();

                _logger.LogInformation(
                    "[FrontProducts] categoryId={CategoryId} → {Type}，subIds=[{SubIds}]",
                    categoryId.Value,
                    isMainCategory ? "主分類" : "子分類",
                    string.Join(", ", subIds));

                if (isMainCategory && subIds.Count > 0)
                    query = query.Where(p => subIds.Contains(p.CategoryId));
                else
                    query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // ── 品牌篩選 ───────────────────────────────────────────
            if (brandIds != null && brandIds.Length > 0)
                query = query.Where(p => p.BrandId != null && brandIds.Contains(p.BrandId!.Value));

            // ── 價格區間（以 MinPrice 比較）────────────────────────
            if (minPrice.HasValue)
                query = query.Where(p => p.MinPrice >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(p => p.MinPrice <= maxPrice.Value);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(kw));
            }

            query = sortBy switch
            {
                "priceAsc"  => query.OrderBy(p => p.MinPrice),
                "priceDesc" => query.OrderByDescending(p => p.MinPrice),
                "soldCount" => query.OrderByDescending(p => p.TotalSales ?? 0),
                _           => query.OrderByDescending(p => p.CreatedAt)
            };

            // SQL-2：COUNT
            int totalCount = await query.CountAsync();

            _logger.LogInformation(
                "[FrontProducts] categoryId={CategoryId} 最終 total={Total}",
                categoryId, totalCount);

            // SQL-3：分頁取資料
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductListDto
                {
                    Id           = p.Id,
                    CategoryId   = p.CategoryId,
                    TotalSales   = p.TotalSales,
                    TotalStock   = p.ProductVariants.Where(v => v.IsDeleted != true).Sum(v => v.Stock ?? 0),
                    Name         = p.Name,
                    CategoryName = p.Category != null ? p.Category.Name : "未分類",
                    StoreName    = p.Store != null ? p.Store.StoreName : string.Empty,
                    MinPrice     = p.MinPrice,
                    MaxPrice     = p.MaxPrice,
                    Status       = p.Status,
                    CreatedAt    = p.CreatedAt,
                    ReviewStatus = p.ReviewStatus,
                    ReviewedBy   = p.ReviewedBy,
                    ReviewDate   = p.ReviewDate,
                    RejectReason = p.RejectReason,
                    MainImageUrl =
                        p.ProductImages.Where(img => img.IsMain == true)
                                       .Select(img => img.ImageUrl).FirstOrDefault()
                        ?? p.ProductImages.Select(img => img.ImageUrl).FirstOrDefault()
                        ?? string.Empty
                })
                .ToListAsync();

            return (items, totalCount);
        }

        // ═══════════════════════════════════════════════════════════
        //  前台商品詳情頁
        // ═══════════════════════════════════════════════════════════

        /// <inheritdoc/>
        public async Task<Product?> GetProductDetailAsync(int id)
        {
            return await _context.Products
                .AsNoTracking()
                .Include(p => p.Brand)
                .Include(p => p.Store)
                .Include(p => p.Category)
                    .ThenInclude(c => c.Parent)
                        .ThenInclude(c2 => c2!.Parent)
                .Include(p => p.ProductImages.OrderBy(img => img.SortOrder))
                .Include(p => p.ProductVariants.Where(v => v.IsDeleted != true))
                    .ThenInclude(v => v.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted != true);
        }

        /// <inheritdoc/>
        public async Task<(decimal? Rating, int ReviewCount)> GetProductRatingAsync(int productId)
        {
            // OrderReview → Order → OrderDetail → ProductId
            var ratings = await _context.OrderReviews
                .AsNoTracking()
                .Where(r => r.IsHidden != true
                         && r.Order.OrderDetails.Any(d => d.ProductId == productId))
                .Select(r => (decimal)r.Rating)
                .ToListAsync();

            if (!ratings.Any())
                return (null, 0);

            return (Math.Round((decimal)ratings.Average(), 1), ratings.Count);
        }

        /// <inheritdoc/>
        public async Task<int> GetStoreActiveProductCountAsync(int storeId)
        {
            return await _context.Products
                .AsNoTracking()
                .CountAsync(p => p.StoreId == storeId && p.Status == 1 && p.IsDeleted != true);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ProductListDto>> GetRelatedProductsAsync(
            int productId, int categoryId, int limit)
        {
            return await _context.Products
                .AsNoTracking()
                .Where(p => p.IsDeleted != true
                         && p.Status == 1
                         && p.Id != productId
                         && p.CategoryId == categoryId)
                .OrderByDescending(p => p.TotalSales ?? 0)
                .Take(limit)
                .Select(p => new ProductListDto
                {
                    Id           = p.Id,
                    CategoryId   = p.CategoryId,
                    TotalSales   = p.TotalSales,
                    TotalStock   = p.ProductVariants.Where(v => v.IsDeleted != true).Sum(v => v.Stock ?? 0),
                    Name         = p.Name,
                    CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                    StoreName    = p.Store    != null ? p.Store.StoreName : string.Empty,
                    MinPrice     = p.MinPrice,
                    MaxPrice     = p.MaxPrice,
                    Status       = p.Status,
                    CreatedAt    = p.CreatedAt,
                    ReviewStatus = p.ReviewStatus,
                    ReviewedBy   = p.ReviewedBy,
                    ReviewDate   = p.ReviewDate,
                    RejectReason = p.RejectReason,
                    MainImageUrl =
                        p.ProductImages.Where(img => img.IsMain == true)
                                       .Select(img => img.ImageUrl).FirstOrDefault()
                        ?? p.ProductImages.Select(img => img.ImageUrl).FirstOrDefault()
                        ?? string.Empty
                })
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<string>> GetHotKeywordsAsync(int limit)
        {
            return await _context.Products
                .Where(p => p.Status == 1 && p.IsDeleted == false)
                .OrderByDescending(p => p.ViewCount)
                .Take(limit)
                .Select(p => p.Name.Length > 10 ? p.Name.Substring(0, 10) : p.Name)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public void AddProductImages(int productId, IEnumerable<ProductImage> images)
        {
            foreach (var img in images)
            {
                img.ProductId = productId;
                _context.ProductImages.Add(img);
            }
            _context.SaveChanges();
        }

        /// <inheritdoc/>
        public void DeleteProductImages(int productId, string webRootPath)
        {
            var images = _context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .ToList();

            foreach (var img in images)
            {
                // 刪除實體檔案
                if (!string.IsNullOrEmpty(img.ImageUrl))
                {
                    var filePath = Path.Combine(webRootPath, img.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(filePath))
                    {
                        try
                        {
                            System.IO.File.Delete(filePath);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "無法刪除圖片檔案：{FilePath}", filePath);
                        }
                    }
                }
            }

            // 刪除資料庫記錄
            _context.ProductImages.RemoveRange(images);
            _context.SaveChanges();
        }

        /// <inheritdoc/>
        public void DeleteProductImagesExcept(int productId, List<string> keepImageUrls, string webRootPath)
        {
            var images = _context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .ToList();

            // 刪除不在 keepImageUrls 中的圖片
            foreach (var img in images)
            {
                if (!keepImageUrls.Contains(img.ImageUrl))
                {
                    // 刪除實體檔案
                    if (!string.IsNullOrEmpty(img.ImageUrl))
                    {
                        var filePath = Path.Combine(webRootPath, img.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (System.IO.File.Exists(filePath))
                        {
                            try
                            {
                                System.IO.File.Delete(filePath);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "無法刪除圖片檔案：{FilePath}", filePath);
                            }
                        }
                    }

                    // 刪除資料庫記錄
                    _context.ProductImages.Remove(img);
                }
            }

            _context.SaveChanges();
        }

        /// <inheritdoc/>
        public void UpdateMainImage(int productId, int mainImageIndex)
        {
            var images = _context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .OrderBy(pi => pi.SortOrder)
                .ToList();

            for (int i = 0; i < images.Count; i++)
            {
                images[i].IsMain = (i == mainImageIndex);
            }

            _context.SaveChanges();
        }
    }
}
