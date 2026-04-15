using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISpanShop.Models.DTOs.Categories;
using ISpanShop.Models.EfModels;
using ISpanShop.Repositories.Products;
using ISpanShop.Repositories.Categories;
using ISpanShop.Repositories.Inventories;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.Repositories.Categories
{
    public class CategoryManageRepository : ICategoryManageRepository
    {
        private readonly ISpanShopDBContext _db;

        public CategoryManageRepository(ISpanShopDBContext db) => _db = db;

        // ── 取得完整樹狀結構 ──
        public IEnumerable<CategoryManageDto> GetTree()
        {
            var all = _db.Categories.ToList();

            // 預先計算各分類商品數（非刪除的商品）
            var productCounts = _db.Products
                .Where(p => p.IsDeleted != true)
                .GroupBy(p => p.CategoryId)
                .ToDictionary(g => g.Key, g => g.Count());

            CategoryManageDto ToDto(Category c) => new CategoryManageDto
            {
                Id           = c.Id,
                Name         = c.Name,
                NameEn       = c.NameEn,
                ParentId     = c.ParentId,
                ParentName   = c.ParentId.HasValue
                    ? all.FirstOrDefault(x => x.Id == c.ParentId)?.Name
                    : null,
                SortOrder    = c.Sort ?? 0,
                IsActive     = c.IsVisible ?? true,
                ImageUrl     = c.IconUrl,
                ProductCount = productCounts.TryGetValue(c.Id, out var cnt) ? cnt : 0,
                ChildCount   = all.Count(x => x.ParentId == c.Id),
                Children     = all
                    .Where(x => x.ParentId == c.Id)
                    .OrderBy(x => x.Sort ?? 0)
                    .Select(x => ToDto(x))
                    .ToList()
            };

            return all
                .Where(c => c.ParentId == null)
                .OrderBy(c => c.Sort ?? 0)
                .ThenBy(c => c.Name)
                .Select(c => ToDto(c))
                .ToList();
        }

        public CategoryManageDto? GetById(int id)
        {
            var c = _db.Categories.FirstOrDefault(x => x.Id == id);
            if (c == null) return null;
            return new CategoryManageDto
            {
                Id           = c.Id,
                Name         = c.Name,
                NameEn       = c.NameEn,
                ParentId     = c.ParentId,
                SortOrder    = c.Sort ?? 0,
                IsActive     = c.IsVisible ?? true,
                ImageUrl     = c.IconUrl,
                ProductCount = _db.Products.Count(p => p.CategoryId == id && p.IsDeleted != true)
            };
        }

        public int GetProductCount(int categoryId)
        {
            return _db.Products.Count(p => p.CategoryId == categoryId && p.IsDeleted != true);
        }

        public void Create(string name, string? nameEn, int? parentId, int sortOrder, string? imageUrl)
        {
            _db.Categories.Add(new Category
            {
                Name      = name,
                NameEn    = nameEn,
                ParentId  = parentId,
                Sort      = sortOrder,
                IsVisible = true,
                IconUrl   = imageUrl
            });
            _db.SaveChanges();
        }

        public void Update(int id, string name, string? nameEn, int? parentId, int sortOrder, string? imageUrl)
        {
            var c = _db.Categories.FirstOrDefault(x => x.Id == id);
            if (c == null) return;
            c.Name     = name;
            c.NameEn   = nameEn;
            c.ParentId = parentId;
            c.Sort     = sortOrder;
            c.IconUrl  = imageUrl;
            _db.SaveChanges();
        }

        public async Task DeleteAsync(int id)
        {
            bool hasProducts = await _db.Products
                .AnyAsync(p => p.CategoryId == id && p.IsDeleted != true);
            if (hasProducts)
                throw new InvalidOperationException("此分類底下尚有商品，請先移除商品後再刪除！");

            bool hasChildren = await _db.Categories
                .AnyAsync(c => c.ParentId == id);
            if (hasChildren)
                throw new InvalidOperationException("此分類底下尚有子分類，無法刪除！");

            var c = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (c == null)
                throw new InvalidOperationException("找不到指定的分類！");

            _db.Categories.Remove(c);
            await _db.SaveChangesAsync();
        }

        public void UpdateIsActive(int id, bool isActive)
        {
            var c = _db.Categories.FirstOrDefault(x => x.Id == id);
            if (c == null) return;
            c.IsVisible = isActive;
            _db.SaveChanges();
        }

        public void UpdateSortOrder(int id, int sortOrder)
        {
            var c = _db.Categories.FirstOrDefault(x => x.Id == id);
            if (c == null) return;
            c.Sort = sortOrder;
            _db.SaveChanges();
        }

        // ─── 前台 API ───────────────────────────────────────────────

        /// <inheritdoc/>
        public async Task<IEnumerable<CategoryManageDto>> GetMainCategoriesAsync()
        {
            // 1. 取啟用中的主分類
            var mains = await _db.Categories
                .AsNoTracking()
                .Where(c => c.ParentId == null && (c.IsVisible ?? true))
                .OrderBy(c => c.Sort ?? 0)
                .ThenBy(c => c.Id)
                .ToListAsync();

            var mainIds = mains.Select(c => c.Id).ToList();

            // 2. 取第一層子分類，建立 childId → parentId 映射
            var childToParent = await _db.Categories
                .AsNoTracking()
                .Where(c => c.ParentId != null && mainIds.Contains(c.ParentId!.Value))
                .Select(c => new { c.Id, c.ParentId })
                .ToListAsync();

            // 3. 統計子分類下的上架商品數，按父分類彙總
            var childIds = childToParent.Select(c => c.Id).ToList();
            var productGroups = await _db.Products
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.Status == 1 && childIds.Contains(p.CategoryId))
                .GroupBy(p => p.CategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToListAsync();

            var countByParent = new Dictionary<int, int>();
            foreach (var pg in productGroups)
            {
                var parentId = childToParent.FirstOrDefault(c => c.Id == pg.CategoryId)?.ParentId;
                if (parentId.HasValue)
                {
                    countByParent.TryGetValue(parentId.Value, out int cur);
                    countByParent[parentId.Value] = cur + pg.Count;
                }
            }

            var childCount = childToParent
                .GroupBy(c => c.ParentId!.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            return mains.Select(c => new CategoryManageDto
            {
                Id           = c.Id,
                Name         = c.Name,
                NameEn       = c.NameEn,
                ParentId     = c.ParentId,
                SortOrder    = c.Sort ?? 0,
                IsActive     = c.IsVisible ?? true,
                ImageUrl     = c.IconUrl,
                ProductCount = countByParent.TryGetValue(c.Id, out var cnt) ? cnt : 0,
                ChildCount   = childCount.TryGetValue(c.Id, out var cc) ? cc : 0,
                Children     = new System.Collections.Generic.List<CategoryManageDto>()
            });
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CategoryManageDto>> GetChildCategoriesAsync(int parentId)
        {
            // 1. 取啟用中的子分類
            var children = await _db.Categories
                .AsNoTracking()
                .Where(c => c.ParentId == parentId && (c.IsVisible ?? true))
                .OrderBy(c => c.Sort ?? 0)
                .ThenBy(c => c.Id)
                .ToListAsync();

            // 2. 統計各子分類底下的上架商品數
            var childIds = children.Select(c => c.Id).ToList();
            var productCounts = await _db.Products
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.Status == 1 && childIds.Contains(p.CategoryId))
                .GroupBy(p => p.CategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CategoryId, x => x.Count);

            return children.Select(c => new CategoryManageDto
            {
                Id           = c.Id,
                Name         = c.Name,
                NameEn       = c.NameEn,
                ParentId     = c.ParentId,
                SortOrder    = c.Sort ?? 0,
                IsActive     = c.IsVisible ?? true,
                ImageUrl     = c.IconUrl,
                ProductCount = productCounts.TryGetValue(c.Id, out var cnt) ? cnt : 0,
                Children     = new System.Collections.Generic.List<CategoryManageDto>()
            });
        }
    }
}
