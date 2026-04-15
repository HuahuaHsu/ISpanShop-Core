using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISpanShop.Models.DTOs.Brands;
using ISpanShop.Models.EfModels;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.Repositories.Brands
{
    public class BrandRepository : IBrandRepository
    {
        private readonly ISpanShopDBContext _db;

        public BrandRepository(ISpanShopDBContext db) => _db = db;

        /// <inheritdoc/>
        public async Task<IEnumerable<BrandWithCountDto>> GetBrandsAsync(
            int? categoryId, string? keyword, int? subCategoryId = null)
        {
            // ── 分類展開（主分類 → 子分類 IDs）─────────────────────────
            List<int>? filterCategoryIds = null;

            if (subCategoryId.HasValue)
            {
                // 直接用子分類，不展開
                filterCategoryIds = new List<int> { subCategoryId.Value };
            }
            else if (categoryId.HasValue)
            {
                // SQL-1：取得目標分類本身 + 直接子分類
                var categoryRows = await _db.Categories
                    .AsNoTracking()
                    .Where(c => c.Id == categoryId.Value || c.ParentId == categoryId.Value)
                    .Select(c => new { c.Id, c.ParentId })
                    .ToListAsync();

                bool isMain = categoryRows.Any(c => c.Id == categoryId.Value && c.ParentId == null);
                var subIds  = categoryRows.Where(c => c.ParentId == categoryId.Value)
                                          .Select(c => c.Id)
                                          .ToList();

                filterCategoryIds = isMain && subIds.Count > 0
                    ? subIds
                    : new List<int> { categoryId.Value };
            }

            // ── SQL-2：Products JOIN Brands，篩選後 GROUP BY 品牌取商品數 ─
            var productQuery = _db.Products
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.Status == 1 && p.BrandId != null);

            if (filterCategoryIds != null)
                productQuery = productQuery.Where(p => filterCategoryIds.Contains(p.CategoryId));

            var brandQuery = productQuery
                .Join(_db.Brands.Where(b => b.IsDeleted != true),
                      p => p.BrandId,
                      b => b.Id,
                      (p, b) => new { b.Id, b.Name })
                .GroupBy(x => new { x.Id, x.Name })
                .Select(g => new BrandWithCountDto
                {
                    Id           = g.Key.Id,
                    Name         = g.Key.Name,
                    ProductCount = g.Count()
                });

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                brandQuery = brandQuery.Where(b => b.Name.Contains(kw));
            }

            return await brandQuery
                .OrderBy(b => b.Name)
                .ToListAsync();
        }
    }
}
