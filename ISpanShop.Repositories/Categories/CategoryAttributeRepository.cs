using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISpanShop.Models.DTOs.Categories;
using ISpanShop.Models.DTOs.Common;
using ISpanShop.Models.EfModels;
using ISpanShop.Repositories.Products;
using ISpanShop.Repositories.Categories;
using ISpanShop.Repositories.Inventories;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.Repositories.Categories
{
    public class CategoryAttributeRepository : ICategoryAttributeRepository
    {
        private readonly ISpanShopDBContext _db;

        public CategoryAttributeRepository(ISpanShopDBContext db)
        {
            _db = db;
        }

        // ── 取得所有規格（含選項、已綁定分類名稱）──
        public IEnumerable<CategorySpecDto> GetAll()
        {
			var specs = _db.CategoryAttributes.ToList();
			var allOptions = _db.CategoryAttributeOptions.ToList();
            var allMappings = _db.CategoryAttributeMappings
                .Join(_db.Categories, m => m.CategoryId, c => c.Id,
                      (m, c) => new { m.CategoryAttributeId, c.Name })
                .ToList();

            return specs.Select(s => new CategorySpecDto
            {
                Id               = s.Id,
                Name             = s.Name,
                InputType        = s.InputType,
                IsRequired       = s.IsRequired,
                AllowCustomInput = s.AllowCustomInput,
                IsActive         = s.IsActive,
                Options          = allOptions
                    .Where(o => o.CategoryAttributeId == s.Id)
                    .OrderBy(o => o.SortOrder)
                    .Select(o => o.OptionName)
                    .ToList(),
                BoundCategoryNames = allMappings
                    .Where(m => m.CategoryAttributeId == s.Id)
                    .Select(m => m.Name)
                    .ToList()
            }).ToList();
        }

        // ── 取得單一規格 ──
        public CategorySpecDto? GetById(int id)
        {
            var s = _db.CategoryAttributes.FirstOrDefault(x => x.Id == id);
            if (s == null) return null;

            return new CategorySpecDto
            {
                Id               = s.Id,
                Name             = s.Name,
                InputType        = s.InputType,
                IsRequired       = s.IsRequired,
                AllowCustomInput = s.AllowCustomInput,
                IsActive         = s.IsActive,
                Options          = _db.CategoryAttributeOptions
                    .Where(o => o.CategoryAttributeId == id)
                    .OrderBy(o => o.SortOrder)
                    .Select(o => o.OptionName)
                    .ToList()
            };
        }

        // ── 新增 ──
        public void Create(string name, string inputType, bool isRequired,
                           bool allowCustomInput, int sortOrder, List<string> options)
        {
            var spec = new CategoryAttribute
            {
                Name             = name,
                InputType        = inputType,
                IsRequired       = isRequired,
                AllowCustomInput = allowCustomInput,
                IsActive         = true
            };
            _db.CategoryAttributes.Add(spec);
            _db.SaveChanges();
            WriteOptions(spec.Id, options);
            _db.SaveChanges();
        }

        // ── 更新 ──
        public void Update(int id, string name, string inputType,
                           bool isRequired, bool allowCustomInput, int sortOrder, List<string> options)
        {
            var spec = _db.CategoryAttributes.FirstOrDefault(s => s.Id == id);
            if (spec == null) return;

            spec.Name             = name;
            spec.InputType        = inputType;
            spec.IsRequired       = isRequired;
            spec.AllowCustomInput = allowCustomInput;

            var old = _db.CategoryAttributeOptions.Where(o => o.CategoryAttributeId == id).ToList();
            _db.CategoryAttributeOptions.RemoveRange(old);
            WriteOptions(id, options);
            _db.SaveChanges();
        }

        // ── 刪除 ──
        public void Delete(int id)
        {
            var mappings = _db.CategoryAttributeMappings.Where(m => m.CategoryAttributeId == id).ToList();
            _db.CategoryAttributeMappings.RemoveRange(mappings);

            var opts = _db.CategoryAttributeOptions.Where(o => o.CategoryAttributeId == id).ToList();
            _db.CategoryAttributeOptions.RemoveRange(opts);

            var spec = _db.CategoryAttributes.FirstOrDefault(s => s.Id == id);
            if (spec != null) _db.CategoryAttributes.Remove(spec);

            _db.SaveChanges();
        }

        // ── 啟用/停用 ──
        public void UpdateIsActive(int id, bool isActive)
        {
            var spec = _db.CategoryAttributes.FirstOrDefault(s => s.Id == id);
            if (spec == null) return;
            spec.IsActive = isActive;
            _db.SaveChanges();
        }

        // ── 分類綁定：取得某分類已綁定的規格 ID 列表（含 IsFilterable）──
        public List<BoundSpecItem> GetBoundSpecItems(int categoryId)
        {
            return _db.CategoryAttributeMappings
                .Where(m => m.CategoryId == categoryId)
                .Select(m => new BoundSpecItem
                {
                    AttributeId  = m.CategoryAttributeId,
                    IsFilterable = m.IsFilterable
                })
                .ToList();
        }

        // ── 分類綁定：取得已綁定的規格 ID（供矩陣用）──
        public List<int> GetBoundAttributeIds(int categoryId)
        {
            return _db.CategoryAttributeMappings
                .Where(m => m.CategoryId == categoryId)
                .Select(m => m.CategoryAttributeId)
                .ToList();
        }

        // ── 即時切換單一綁定 ──
        public void ToggleBinding(int categoryId, int specId, bool isBound)
        {
            var existing = _db.CategoryAttributeMappings
                .FirstOrDefault(m => m.CategoryId == categoryId && m.CategoryAttributeId == specId);

            if (isBound && existing == null)
            {
                _db.CategoryAttributeMappings.Add(new CategoryAttributeMapping
                {
                    CategoryId          = categoryId,
                    CategoryAttributeId = specId,
                    IsFilterable        = false
                });
            }
            else if (!isBound && existing != null)
            {
                _db.CategoryAttributeMappings.Remove(existing);
            }
            _db.SaveChanges();
        }

        // ── 即時切換 IsFilterable ──
        public void ToggleFilterable(int categoryId, int specId, bool isFilterable)
        {
            var mapping = _db.CategoryAttributeMappings
                .FirstOrDefault(m => m.CategoryId == categoryId && m.CategoryAttributeId == specId);
            if (mapping == null) return;
            mapping.IsFilterable = isFilterable;
            _db.SaveChanges();
        }

        // ── 取得矩陣資料用 ──
        public List<object> GetAllBindingPairs()
        {
            return _db.CategoryAttributeMappings
                .Select(m => new {
                    categoryId   = m.CategoryId,
                    attributeId  = m.CategoryAttributeId,
                    isFilterable = m.IsFilterable
                })
                .Cast<object>()
                .ToList();
        }

        // ── 取得所有分類 ──
        public IEnumerable<CategoryDto> GetAllCategories()
        {
            return _db.Categories
                .OrderBy(c => c.Sort)
                .ThenBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id       = c.Id,
                    Name     = c.Name,
                    ParentId = c.ParentId
                })
                .ToList();
        }

        // ── 新版綁定管理 ──────────────────────────────────────────

        public List<BoundSpecDetailDto> GetBoundSpecsWithDetails(int categoryId)
        {
            var mappings = _db.CategoryAttributeMappings
                .Where(m => m.CategoryId == categoryId)
                .OrderBy(m => m.Sort)
                .ThenBy(m => m.CategoryAttributeId)
                .ToList();

            if (!mappings.Any()) return new List<BoundSpecDetailDto>();

            var specIds  = mappings.Select(m => m.CategoryAttributeId).ToList();
            var specs    = _db.CategoryAttributes.Where(s => specIds.Contains(s.Id)).ToList();
            var options  = _db.CategoryAttributeOptions
                .Where(o => specIds.Contains(o.CategoryAttributeId))
                .OrderBy(o => o.SortOrder)
                .ToList();

            return mappings.Select(m =>
            {
                var spec = specs.First(s => s.Id == m.CategoryAttributeId);
                return new BoundSpecDetailDto
                {
                    SpecId           = m.CategoryAttributeId,
                    Name             = spec.Name,
                    InputType        = spec.InputType,
                    IsRequired       = spec.IsRequired,
                    AllowCustomInput = spec.AllowCustomInput,
                    IsFilterable     = m.IsFilterable,
                    Sort             = m.Sort,
                    Options          = options
                        .Where(o => o.CategoryAttributeId == m.CategoryAttributeId)
                        .Select(o => o.OptionName)
                        .ToList()
                };
            }).ToList();
        }

        public void BindSpec(int categoryId, int specId)
        {
            if (_db.CategoryAttributeMappings.Any(m => m.CategoryId == categoryId && m.CategoryAttributeId == specId))
                return;

            var maxSort = _db.CategoryAttributeMappings
                .Where(m => m.CategoryId == categoryId)
                .Select(m => (int?)m.Sort)
                .Max() ?? 0;

            _db.CategoryAttributeMappings.Add(new CategoryAttributeMapping
            {
                CategoryId          = categoryId,
                CategoryAttributeId = specId,
                IsFilterable        = false,
                Sort                = maxSort + 1
            });
            _db.SaveChanges();
        }

        public void UnbindSpec(int categoryId, int specId)
        {
            var mapping = _db.CategoryAttributeMappings
                .FirstOrDefault(m => m.CategoryId == categoryId && m.CategoryAttributeId == specId);
            if (mapping == null) return;
            _db.CategoryAttributeMappings.Remove(mapping);
            _db.SaveChanges();
        }

        public void UpdateBindingSort(int categoryId, List<int> orderedSpecIds)
        {
            var mappings = _db.CategoryAttributeMappings
                .Where(m => m.CategoryId == categoryId)
                .ToList();

            for (int i = 0; i < orderedSpecIds.Count; i++)
            {
                var m = mappings.FirstOrDefault(x => x.CategoryAttributeId == orderedSpecIds[i]);
                if (m != null) m.Sort = i + 1;
            }
            _db.SaveChanges();
        }

        public bool HasBindings(int specId)
            => _db.CategoryAttributeMappings.Any(m => m.CategoryAttributeId == specId);

        // ── 屬性庫分頁查詢 ────────────────────────────────────────
        public async Task<PagedResult<CategorySpecDto>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = _db.CategoryAttributes.OrderBy(s => s.Id);

            var totalCount = await query.CountAsync();

            var specs = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var specIds = specs.Select(s => s.Id).ToList();

            var options = await _db.CategoryAttributeOptions
                .Where(o => specIds.Contains(o.CategoryAttributeId))
                .OrderBy(o => o.SortOrder)
                .ToListAsync();

            var mappings = await _db.CategoryAttributeMappings
                .Where(m => specIds.Contains(m.CategoryAttributeId))
                .Join(_db.Categories, m => m.CategoryId, c => c.Id,
                      (m, c) => new { m.CategoryAttributeId, c.Name })
                .ToListAsync();

            var data = specs.Select(s => new CategorySpecDto
            {
                Id               = s.Id,
                Name             = s.Name,
                InputType        = s.InputType,
                IsRequired       = s.IsRequired,
                AllowCustomInput = s.AllowCustomInput,
                IsActive         = s.IsActive,
                Options          = options
                    .Where(o => o.CategoryAttributeId == s.Id)
                    .Select(o => o.OptionName)
                    .ToList(),
                BoundCategoryNames = mappings
                    .Where(m => m.CategoryAttributeId == s.Id)
                    .Select(m => m.Name)
                    .ToList()
            }).ToList();

            return PagedResult<CategorySpecDto>.Create(data, totalCount, pageNumber, pageSize);
        }

        // ── 私有：寫入選項 ──
        private void WriteOptions(int specId, List<string> options)
        {
            if (options == null || !options.Any()) return;
            _db.CategoryAttributeOptions.AddRange(
                options.Where(o => !string.IsNullOrWhiteSpace(o))
                       .Select((o, i) => new CategoryAttributeOption
                       {
                           CategoryAttributeId = specId,
                           OptionName          = o.Trim(),
                           SortOrder           = i + 1
                       })
            );
        }
    }
}
