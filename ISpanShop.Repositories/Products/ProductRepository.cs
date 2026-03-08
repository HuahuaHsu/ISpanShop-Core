using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ISpanShop.Models.DTOs.Products;
using ISpanShop.Models.DTOs.Common;
using ISpanShop.Models.EfModels;
using ISpanShop.Repositories.Products;
using ISpanShop.Repositories.Categories;
using ISpanShop.Repositories.Inventories;

namespace ISpanShop.Repositories.Products
{
    /// <summary>
    /// 商品 Repository 實作 - 處理商品的 CRUD 操作
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly ISpanShopDBContext _context;

        /// <summary>
        /// 建構子 - 注入 DbContext
        /// </summary>
        /// <param name="context">ISpanShop 資料庫上下文</param>
        public ProductRepository(ISpanShopDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 新增商品 - 依賴 EF Core 的關聯追蹤來一併儲存 Variant 和 Image
        /// </summary>
        /// <param name="product">商品實體</param>
        public void AddProduct(Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
        }

        /// <summary>
        /// 檢查 SKU 代碼是否已存在
        /// </summary>
        /// <param name="skuCode">SKU 代碼</param>
        /// <returns>true 表示已存在，false 表示不存在</returns>
        public bool IsSkuExists(string skuCode)
        {
            return _context.ProductVariants.Any(pv => pv.SkuCode == skuCode);
        }

        /// <summary>
        /// 取得待審核商品列表 (ReviewStatus == 0)
        /// </summary>
        /// <returns>待審核商品集合</returns>
        public IEnumerable<Product> GetPendingProducts()
        {
            return _context.Products
                .AsNoTracking()
                .Include(p => p.Store)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.ReviewStatus == 0 && p.IsDeleted != true)
                .ToList();
        }

        /// <summary>
        /// 更新商品狀態
        /// </summary>
        /// <param name="id">商品 ID</param>
        /// <param name="status">新的狀態值</param>
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
        /// 取得所有商品列表 - 包含關聯資料
        /// </summary>
        /// <returns>所有商品集合</returns>
        public IEnumerable<Product> GetAllProducts()
        {
            return _context.Products
                .AsNoTracking()
                .Include(p => p.Store)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .Where(p => p.IsDeleted != true)
                .ToList();
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
        /// 分頁取得商品列表，支援分類篩選與多維度搜尋
        /// </summary>
        /// <param name="criteria">搜尋條件（分類、關鍵字、商家、狀態篩選 + 分頁）</param>
        /// <returns>商品集合與總筆數</returns>
        public (IEnumerable<Product> Items, int TotalCount) GetProductsPaged(ProductSearchCriteria criteria)
        {
            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.Store)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .Where(p => p.IsDeleted != true)
                .AsQueryable();

            // 分類篩選：優先以子分類篩選，否則以主分類篩選所有子分類商品
            if (criteria.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == criteria.CategoryId.Value);
            }
            else if (criteria.ParentCategoryId.HasValue)
            {
                query = query.Where(p => p.Category.ParentId == criteria.ParentCategoryId.Value);
            }

            // 關鍵字搜尋：在名稱或描述中包含關鍵字（Description 可能為 null，需防呆）
            if (!string.IsNullOrWhiteSpace(criteria.Keyword))
            {
                var keyword = criteria.Keyword.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(keyword) ||
                    (p.Description != null && p.Description.ToLower().Contains(keyword))
                );
            }

            // 商家篩選：精準匹配商家 ID
            if (criteria.StoreId.HasValue)
            {
                query = query.Where(p => p.StoreId == criteria.StoreId.Value);
            }

            // 品牌篩選：精準匹配品牌 ID
            if (criteria.BrandId.HasValue)
            {
                query = query.Where(p => p.BrandId == criteria.BrandId.Value);
            }

            // 商品狀態篩選：指定狀態時精準匹配；未指定時預設排除待審核（Status==2）
            if (criteria.Status.HasValue)
            {
                query = query.Where(p => p.Status == criteria.Status.Value);
            }
            else
            {
                query = query.Where(p => p.Status != 2);
            }

            // 建檔日期起：>= StartDate
            if (criteria.StartDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt >= criteria.StartDate.Value);
            }

            // 建檔日期迄：<= EndDate 當天最後一刻（23:59:59.9999999）
            if (criteria.EndDate.HasValue)
            {
                var endOfDay = criteria.EndDate.Value.AddDays(1).AddTicks(-1);
                query = query.Where(p => p.CreatedAt <= endOfDay);
            }

            // 排序
            query = criteria.SortOrder switch
            {
                "name_asc"    => query.OrderBy(p => p.Name),
                "name_desc"   => query.OrderByDescending(p => p.Name),
                "price_asc"   => query.OrderBy(p => p.MinPrice),
                "price_desc"  => query.OrderByDescending(p => p.MinPrice),
                "status_asc"  => query.OrderBy(p => p.Status),
                "status_desc" => query.OrderByDescending(p => p.Status),
                "date_asc"    => query.OrderBy(p => p.CreatedAt),
                _             => query.OrderByDescending(p => p.CreatedAt) // date_desc（預設：最新優先）
            };

            int totalCount = query.Count();

            var items = query
                .Skip((criteria.PageNumber - 1) * criteria.PageSize)
                .Take(criteria.PageSize)
                .ToList();

            return (items, totalCount);
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
                product.Status = targetStatus;
                product.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return products.Count;
        }

        /// <summary>
        /// 核准商品審核 - Status=1(上架), ReviewStatus=1, 記錄審核人
        /// </summary>
        public void ApproveProduct(int id, string adminId)
        {
            var product = _context.Products.Find(id);
            if (product == null) return;

            product.Status       = 1;
            product.ReviewStatus = 1;
            product.ReviewedBy   = adminId;
            product.RejectReason = null;
            product.ReviewDate   = null;
            product.UpdatedAt    = DateTime.Now;
            if (product.Name != null && product.Name.StartsWith("[待審核]"))
                product.Name = product.Name.Substring("[待審核]".Length).TrimStart();
            _context.SaveChanges();
        }

        /// <summary>
        /// 退回商品審核 - Status=3(退回), ReviewStatus=2, 記錄原因、審核人與退回時間
        /// </summary>
        public void RejectProduct(int id, string adminId, string reason)
        {
            var product = _context.Products.Find(id);
            if (product == null) return;

            product.Status       = 3;
            product.ReviewStatus = 2;
            product.ReviewedBy   = adminId;
            product.RejectReason = reason;
            product.ReviewDate   = DateTime.Now;
            product.UpdatedAt    = DateTime.Now;
            _context.SaveChanges();
        }

        /// <summary>
        /// 提交商品審核：自動違禁詞攔截（ReviewStatus=2）或進入待審核佇列（ReviewStatus=0）
        /// </summary>
        public void SubmitProductForReview(int productId)
        {
            var product = _context.Products.Find(productId);
            if (product == null) return;

            var badWords = new[]
            {
                "高仿", "原單", "槍械", "毒品", "贗品", "假貨", "冒牌", "盜版",
                "走私", "非法", "詐騙", "傳銷", "洗錢", "賭博", "色情",
                "暴力", "恐怖", "炸藥", "大麻", "槍", "彈藥", "仿冒", "武器"
            };
            string combined = (product.Name + " " + (product.Description ?? "")).ToLower();
            string? hit = badWords.FirstOrDefault(w => combined.Contains(w.ToLower()));

            if (hit != null)
            {
                // 自動攔截
                product.Status       = 3;
                product.ReviewStatus = 2;
                product.ReviewedBy   = "System";
                product.RejectReason = $"自動攔截：內容含違禁詞「{hit}」";
                product.ReviewDate   = DateTime.Now;
            }
            else
            {
                // 通過初審，進入人工審核佇列
                product.Status       = 2;
                product.ReviewStatus = 0;
                product.ReviewedBy   = null;
                product.RejectReason = null;
                product.ReviewDate   = null;
            }
            product.UpdatedAt = DateTime.Now;
            _context.SaveChanges();
        }

        /// <summary>
        /// 軟刪除所有過期退回商品（ReviewStatus==2 且距 ReviewDate 已達 expirationSeconds 秒）
        /// 回傳清理筆數。
        /// </summary>
        public int CleanupExpiredRejected(int expirationSeconds)
        {
            var cutoff = DateTime.Now.AddSeconds(-expirationSeconds);
            var expired = _context.Products
                .Where(p => p.ReviewStatus == 2
                         && p.ReviewDate != null
                         && p.ReviewDate <= cutoff
                         && p.IsDeleted != true)
                .ToList();

            foreach (var p in expired)
            {
                p.IsDeleted  = true;
                p.UpdatedAt  = DateTime.Now;
            }
            _context.SaveChanges();
            return expired.Count;
        }


        /// <summary>
        /// 取得最近退回的商品清單（Status == 3），依 UpdatedAt 降冪排序
        /// </summary>
        public IEnumerable<Product> GetRecentRejectedProducts(int top)
        {
            return _context.Products
                .Include(p => p.Store)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .Where(p => p.ReviewStatus == 2 && p.IsDeleted != true)
                .OrderByDescending(p => p.UpdatedAt)
                .Take(top)
                .ToList();
        }

        /// <summary>
        /// 分頁取得待審核商品（ReviewStatus == 0）
        /// </summary>
        public (IEnumerable<Product> Items, int TotalCount) GetPendingProductsPaged(int page, int pageSize)
        {
            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.Store)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .Where(p => p.ReviewStatus == 0 && p.IsDeleted != true)
                .OrderByDescending(p => p.CreatedAt);

            int total = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return (items, total);
        }

        /// <summary>
        /// 分頁取得已退回商品（ReviewStatus == 2）
        /// </summary>
        public (IEnumerable<Product> Items, int TotalCount) GetRejectedProductsPaged(int page, int pageSize)
        {
            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.Store)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .Where(p => p.ReviewStatus == 2 && p.IsDeleted != true)
                .OrderByDescending(p => p.ReviewDate ?? p.UpdatedAt);

            int total = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return (items, total);
        }

        /// <summary>
        /// 重設為待審核：清空審核結果欄位，商品回到 Status=2 / ReviewStatus=0
        /// </summary>
        public void ResetToPending(int productId)
        {
            var product = _context.Products.Find(productId);
            if (product == null) return;

            product.Status       = 2;
            product.ReviewStatus = 0;
            product.ReviewedBy   = null;
            product.ReviewDate   = null;
            product.RejectReason = null;
            product.UpdatedAt    = DateTime.Now;
            _context.SaveChanges();
        }

        public void UpdateProduct(ProductUpdateDto dto)
        {
            var product = _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefault(p => p.Id == dto.Id);
            if (product == null) return;

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.CategoryId = dto.CategoryId;
            product.BrandId = dto.BrandId;
            product.SpecDefinitionJson = dto.SpecDefinitionJson;
            product.UpdatedAt = DateTime.Now;

            if (!string.IsNullOrWhiteSpace(dto.MainImageUrl))
            {
                var mainImg = product.ProductImages.FirstOrDefault(i => i.IsMain == true);
                if (mainImg != null)
                    mainImg.ImageUrl = dto.MainImageUrl;
                else
                    product.ProductImages.Add(new ProductImage
                    {
                        ImageUrl = dto.MainImageUrl,
                        IsMain = true,
                        SortOrder = 0
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
        {
            return _context.ProductVariants
                .Include(v => v.Product)
                .FirstOrDefault(v => v.Id == id);
        }

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
            variant.SkuCode = dto.SkuCode;
            variant.Price = dto.Price;
            variant.Stock = dto.Stock;
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

        public (int Total, int Published, int Unpublished, int Pending) GetStatusCounts()
        {
            var q = _context.Products.Where(p => p.IsDeleted != true);
            return (
                q.Count(),
                q.Count(p => p.Status == 1),          // 已上架
                q.Count(p => p.Status == 0),          // 已下架
                q.Count(p => p.ReviewStatus == 0)     // 待審核（與 PendingReview 列表邏輯一致）
            );
        }

        public void ForceUnpublish(int id, string? reason)
        {
            var product = _context.Products.Find(id);
            if (product == null) return;
            product.Status       = 0;
            product.RejectReason = reason;
            product.UpdatedAt    = DateTime.Now;
            _context.SaveChanges();
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
                    (p.Description != null && p.Description.ToLower().Contains(keyword)));
            }

            if (criteria.StoreId.HasValue)
                query = query.Where(p => p.StoreId == criteria.StoreId.Value);

            if (criteria.BrandId.HasValue)
                query = query.Where(p => p.BrandId == criteria.BrandId.Value);

            if (criteria.Status.HasValue)
                query = query.Where(p => p.Status == criteria.Status.Value);
            else
                query = query.Where(p => p.Status != 2);

            if (criteria.StartDate.HasValue)
                query = query.Where(p => p.CreatedAt >= criteria.StartDate.Value);

            if (criteria.EndDate.HasValue)
            {
                var endOfDay = criteria.EndDate.Value.AddDays(1).AddTicks(-1);
                query = query.Where(p => p.CreatedAt <= endOfDay);
            }

            query = criteria.SortOrder switch
            {
                "name_asc"    => query.OrderBy(p => p.Name),
                "name_desc"   => query.OrderByDescending(p => p.Name),
                "price_asc"   => query.OrderBy(p => p.MinPrice),
                "price_desc"  => query.OrderByDescending(p => p.MinPrice),
                "status_asc"  => query.OrderBy(p => p.Status),
                "status_desc" => query.OrderByDescending(p => p.Status),
                "date_asc"    => query.OrderBy(p => p.CreatedAt),
                _             => query.OrderByDescending(p => p.CreatedAt)
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
                    Name         = p.Name,
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
                    RejectReason = p.RejectReason,
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
                    RejectReason = p.RejectReason,
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
            product.ReviewDate   = null;
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

            var badWords = new[]
            {
                "高仿", "原單", "槍械", "毒品", "贗品", "假貨", "冒牌", "盜版",
                "走私", "非法", "詐騙", "傳銷", "洗錢", "賭博", "色情",
                "暴力", "恐怖", "炸藥", "大麻", "槍", "彈藥", "仿冒", "武器"
            };
            string combined = (product.Name + " " + (product.Description ?? "")).ToLower();
            string? hit = badWords.FirstOrDefault(w => combined.Contains(w.ToLower()));

            if (hit != null)
            {
                product.Status       = 3;
                product.ReviewStatus = 2;
                product.ReviewedBy   = "System";
                product.RejectReason = $"自動攔截：內容含違禁詞「{hit}」";
                product.ReviewDate   = DateTime.Now;
            }
            else
            {
                product.Status       = 2;
                product.ReviewStatus = 0;
                product.ReviewedBy   = null;
                product.RejectReason = null;
                product.ReviewDate   = null;
            }
            product.UpdatedAt = DateTime.Now;

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
        public async Task<(int Total, int Published, int Unpublished, int Pending)> GetStatusCountsAsync()
        {
            var q = _context.Products.Where(p => p.IsDeleted != true);
            return (
                await q.CountAsync(),
                await q.CountAsync(p => p.Status == 1),
                await q.CountAsync(p => p.Status == 0),
                await q.CountAsync(p => p.ReviewStatus == 0)
            );
        }

        /// <inheritdoc/>
        public async Task ForceUnpublishAsync(int id, string? reason)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return;

            product.Status       = 0;
            product.RejectReason = reason;
            product.UpdatedAt    = DateTime.Now;

            await _context.SaveChangesAsync();
        }
    }
}
