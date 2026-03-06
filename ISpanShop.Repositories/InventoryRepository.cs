using System.Collections.Generic;
using System.Linq;
using ISpanShop.Models.DTOs;
using ISpanShop.Models.EfModels;
using ISpanShop.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.Repositories
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
                var kw = criteria.Keyword.Trim();
                baseQuery = baseQuery.Where(v =>
                    v.Product.Name.Contains(kw) ||
                    v.VariantName.Contains(kw) ||
                    v.SkuCode.Contains(kw));
            }

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
    }
}
