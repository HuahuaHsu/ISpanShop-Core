using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ISpanShop.Models.DTOs;
using ISpanShop.Models.EfModels;
using ISpanShop.Repositories.Interfaces;

namespace ISpanShop.Repositories
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
        /// 取得待審核商品列表 (Status == 2)
        /// </summary>
        /// <returns>待審核商品集合</returns>
        public IEnumerable<Product> GetPendingProducts()
        {
            return _context.Products
                .Include(p => p.Store)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.Status == 2)
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
                .Include(p => p.Store)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
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
                .Include(p => p.Store)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
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

            // 商品狀態篩選：精準匹配狀態
            if (criteria.Status.HasValue)
            {
                query = query.Where(p => p.Status == criteria.Status.Value);
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
        /// 核准商品審核 - 將 Status 設為 1 (上架)
        /// </summary>
        public void ApproveProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                product.Status = 1;
                product.UpdatedAt = DateTime.Now;
                _context.SaveChanges();
            }
        }

        /// <summary>
        /// 退回商品審核 - 將 Status 設為 3 (審核退回)，並儲存退回原因
        /// </summary>
        public void RejectProduct(int id, string? reason)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                product.Status = 3;
                product.RejectReason = reason;
                product.UpdatedAt = DateTime.Now;
                _context.SaveChanges();
            }
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
                .Where(p => p.Status == 3)
                .OrderByDescending(p => p.UpdatedAt)
                .Take(top)
                .ToList();
        }
    }
}
