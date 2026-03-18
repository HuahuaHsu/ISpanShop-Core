using System.Collections.Generic;
using System.Linq;
using ISpanShop.Models.DTOs.Inventories;
using ISpanShop.Models.DTOs.Common;
using ISpanShop.Models.EfModels;
using ISpanShop.Repositories.Products;
using ISpanShop.Repositories.Categories;
using ISpanShop.Repositories.Inventories;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.Repositories.Inventories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly ISpanShopDBContext _context;

        public InventoryRepository(ISpanShopDBContext context)
        {
            _context = context;
        }

        public (IEnumerable<ProductVariant> Items, int TotalCount) GetVariantsPaged(InventorySearchCriteria criteria)
        {
            var baseQuery = _context.ProductVariants
                .Include(v => v.Product)
                    .ThenInclude(p => p.Store)
                .Include(v => v.Product)
                    .ThenInclude(p => p.Category)
                .Where(v => v.IsDeleted != true && v.Product != null && v.Product.IsDeleted != true)
                .AsQueryable();

            if (criteria.LowStockOnly)
                baseQuery = baseQuery.Where(v => (v.Stock ?? 0) <= (v.SafetyStock ?? 0));
            else if (criteria.ZeroStockOnly)
                baseQuery = baseQuery.Where(v => (v.Stock ?? 0) == 0);
            else if (criteria.NormalStockOnly)
                baseQuery = baseQuery.Where(v => (v.Stock ?? 0) > (v.SafetyStock ?? 0));

            if (!string.IsNullOrWhiteSpace(criteria.Keyword))
            {
                var kw = criteria.Keyword.Trim().ToLower();
                baseQuery = baseQuery.Where(v =>
                    v.Product.Name.ToLower().Contains(kw) ||
                    v.VariantName.ToLower().Contains(kw) ||
                    v.SkuCode.ToLower().Contains(kw) ||
                    (v.Product.Store != null && v.Product.Store.StoreName.ToLower().Contains(kw)) ||
                    (v.Product.Brand != null && v.Product.Brand.Name.ToLower().Contains(kw)) ||
                    (v.Product.Category != null && v.Product.Category.Name.ToLower().Contains(kw)));
            }

            if (criteria.ParentCategoryId.HasValue)
                baseQuery = baseQuery.Where(v => v.Product.Category.ParentId == criteria.ParentCategoryId.Value || v.Product.CategoryId == criteria.ParentCategoryId.Value);

            if (criteria.CategoryId.HasValue)
                baseQuery = baseQuery.Where(v => v.Product.CategoryId == criteria.CategoryId.Value);

            if (criteria.StoreId.HasValue)
                baseQuery = baseQuery.Where(v => v.Product.StoreId == criteria.StoreId.Value);

            if (criteria.MinStock.HasValue)
                baseQuery = baseQuery.Where(v => (v.Stock ?? 0) >= criteria.MinStock.Value);

            if (criteria.MaxStock.HasValue)
                baseQuery = baseQuery.Where(v => (v.Stock ?? 0) <= criteria.MaxStock.Value);

            baseQuery = (criteria.SortBy ?? string.Empty) switch
            {
                "stock_asc"  => baseQuery.OrderBy(v => v.Stock ?? 0).ThenBy(v => v.Product.Name),
                "stock_desc" => baseQuery.OrderByDescending(v => v.Stock ?? 0).ThenBy(v => v.Product.Name),
                "safety_asc" => baseQuery.OrderBy(v => v.SafetyStock ?? 0).ThenBy(v => v.Product.Name),
                "name_asc"   => baseQuery.OrderBy(v => v.Product.Name).ThenBy(v => v.VariantName),
                _            => baseQuery
                                    .OrderBy(v => (v.Stock ?? 0) > (v.SafetyStock ?? 0) ? 1 : 0)
                                    .ThenBy(v => v.Product.Name)
                                    .ThenBy(v => v.VariantName)
            };

            var total = baseQuery.Count();
            var items = baseQuery
                .Skip((criteria.PageNumber - 1) * criteria.PageSize)
                .Take(criteria.PageSize)
                .ToList();

            return (items, total);
        }

        public (IEnumerable<ProductVariant> Variants, int TotalProductCount)
            GetVariantsGroupedPaged(InventorySearchCriteria criteria)
        {
            // ── Step 1：建立篩選查詢（不 Include 導覽屬性，只取輕量欄位）──────────
            var baseQuery = _context.ProductVariants
                .Where(v => v.IsDeleted != true && v.Product != null && v.Product.IsDeleted != true)
                .Include(v => v.Product)
                .AsQueryable();

            if (criteria.LowStockOnly)
                baseQuery = baseQuery.Where(v => (v.Stock ?? 0) <= (v.SafetyStock ?? 0));
            else if (criteria.ZeroStockOnly)
                baseQuery = baseQuery.Where(v => (v.Stock ?? 0) == 0);
            else if (criteria.NormalStockOnly)
                baseQuery = baseQuery.Where(v => (v.Stock ?? 0) > (v.SafetyStock ?? 0));

            if (!string.IsNullOrWhiteSpace(criteria.Keyword))
            {
                var kw = criteria.Keyword.Trim().ToLower();
                baseQuery = baseQuery.Where(v =>
                    v.Product.Name.ToLower().Contains(kw) ||
                    v.VariantName.ToLower().Contains(kw) ||
                    v.SkuCode.ToLower().Contains(kw) ||
                    (v.Product.Store != null && v.Product.Store.StoreName.ToLower().Contains(kw)) ||
                    (v.Product.Brand != null && v.Product.Brand.Name.ToLower().Contains(kw)) ||
                    (v.Product.Category != null && v.Product.Category.Name.ToLower().Contains(kw)));
            }

            if (criteria.ParentCategoryId.HasValue)
                baseQuery = baseQuery.Where(v => v.Product.Category.ParentId == criteria.ParentCategoryId.Value || v.Product.CategoryId == criteria.ParentCategoryId.Value);

            if (criteria.CategoryId.HasValue)
                baseQuery = baseQuery.Where(v => v.Product.CategoryId == criteria.CategoryId.Value);

            if (criteria.StoreId.HasValue)
                baseQuery = baseQuery.Where(v => v.Product.StoreId == criteria.StoreId.Value);

            if (criteria.MinStock.HasValue)
                baseQuery = baseQuery.Where(v => (v.Stock ?? 0) >= criteria.MinStock.Value);

            if (criteria.MaxStock.HasValue)
                baseQuery = baseQuery.Where(v => (v.Stock ?? 0) <= criteria.MaxStock.Value);

            // ── Step 2：輕量投影到記憶體，依商品分組計算排序指標 ────────────────
            var flat = baseQuery.Select(v => new
            {
                v.ProductId,
                ProductName = v.Product.Name,
                Stock       = v.Stock       ?? 0,
                SafetyStock = v.SafetyStock ?? 0
            }).ToList();

            var productGroups = flat
                .GroupBy(v => v.ProductId)
                .Select(g => new
                {
                    ProductId   = g.Key,
                    ProductName = g.First().ProductName,
                    MinStock    = g.Min(v => v.Stock),
                    TotalStock  = g.Sum(v => v.Stock),
                    HasIssue    = g.Any(v => v.Stock <= v.SafetyStock)
                })
                .ToList();

            var totalProductCount = productGroups.Count;

            // ── Step 3：商品層級排序 + 分頁，取出分頁後的 ProductId 清單 ──────────
            IEnumerable<int> pagedIds = ((criteria.SortBy ?? "") switch
            {
                "name_asc"   => productGroups.OrderBy(p => p.ProductName),
                "stock_asc"  => productGroups.OrderBy(p => p.MinStock).ThenBy(p => p.ProductName),
                "stock_desc" => productGroups.OrderByDescending(p => p.TotalStock).ThenBy(p => p.ProductName),
                "safety_asc" => productGroups.OrderBy(p => p.MinStock).ThenBy(p => p.ProductName),
                _            => productGroups
                                    .OrderByDescending(p => p.HasIssue ? 1 : 0)
                                    .ThenBy(p => p.ProductName)
            })
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Select(p => p.ProductId)
            .ToList();

            // ── Step 4：以 ProductId 二次查詢完整 Variants（含 Store / Category）─
            var variants = _context.ProductVariants
                .Include(v => v.Product)
                    .ThenInclude(p => p.Store)
                .Include(v => v.Product)
                    .ThenInclude(p => p.Category)
                .Where(v => pagedIds.Contains(v.ProductId)
                         && v.IsDeleted != true
                         && v.Product.IsDeleted != true)
                .OrderBy(v => v.VariantName)
                .ToList();

            return (variants, totalProductCount);
        }

        public int GetLowStockCount()
            => _context.ProductVariants
                .Count(v => v.IsDeleted != true &&
                            v.Product != null &&
                            v.Product.IsDeleted != true &&
                            (v.Stock ?? 0) <= (v.SafetyStock ?? 0));

        public int GetZeroStockCount()
            => _context.ProductVariants
                .Count(v => v.IsDeleted != true &&
                            v.Product != null &&
                            v.Product.IsDeleted != true &&
                            (v.Stock ?? 0) == 0);

        public int GetTotalVariantCount()
            => _context.ProductVariants
                .Count(v => v.IsDeleted != true &&
                            v.Product != null &&
                            v.Product.IsDeleted != true);

        public ProductVariant? GetVariantById(int variantId)
            => _context.ProductVariants
                .Include(v => v.Product)
                    .ThenInclude(p => p.Store)
                .Include(v => v.Product)
                    .ThenInclude(p => p.Category)
                .FirstOrDefault(v => v.Id == variantId &&
                                     v.IsDeleted != true &&
                                     v.Product != null &&
                                     v.Product.IsDeleted != true);

        public void UpdateStock(int variantId, int newStock)
        {
            var variant = _context.ProductVariants.Find(variantId);
            if (variant == null) return;
            variant.Stock = newStock;
            _context.SaveChanges();
        }

        public void UpdateSafetyStock(int variantId, int newSafetyStock)
        {
            var variant = _context.ProductVariants.Find(variantId);
            if (variant == null) return;
            variant.SafetyStock = newSafetyStock;
            _context.SaveChanges();
        }

        public void UpdateStockAndSafetyStock(int variantId, int newStock, int newSafetyStock)
        {
            var variant = _context.ProductVariants.Find(variantId);
            if (variant == null) return;
            variant.Stock       = newStock;
            variant.SafetyStock = newSafetyStock;
            _context.SaveChanges();
        }

        public IEnumerable<(int Id, string Name)> GetStoreOptions()
            => _context.Stores
                .OrderBy(s => s.StoreName)
                .Select(s => new { s.Id, s.StoreName })
                .ToList()
                .Select(s => (s.Id, s.StoreName));

        public IEnumerable<(int Id, string Name)> GetCategoryOptions()
            => _context.Categories
                .Where(c => c.IsVisible != false)
                .OrderBy(c => c.Sort).ThenBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToList()
                .Select(c => (c.Id, c.Name));

        public IEnumerable<(int Id, string Name)> GetMainCategories()
            => _context.Categories
                .Where(c => c.ParentId == null && c.IsVisible != false)
                .OrderBy(c => c.Sort).ThenBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToList()
                .Select(c => (c.Id, c.Name));

        public IEnumerable<(int Id, string Name)> GetSubCategories(int parentId)
            => _context.Categories
                .Where(c => c.ParentId == parentId && c.IsVisible != false)
                .OrderBy(c => c.Sort).ThenBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToList()
                .Select(c => (c.Id, c.Name));
    }
}
