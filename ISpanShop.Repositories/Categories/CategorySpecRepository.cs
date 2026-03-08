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
    public class CategorySpecRepository : ICategorySpecRepository
    {
        private readonly ISpanShopDBContext _db;

        public CategorySpecRepository(ISpanShopDBContext db)
        {
            _db = db;
        }

        // ── 取得所有規格（含選項、已綁定分類名稱）──
        public IEnumerable<CategorySpecDto> GetAll()
        {
            var specs = _db.CategorySpecs.OrderBy(s => s.SortOrder).ToList();
            var allOptions = _db.CategorySpecOptions.ToList();
            var allMappings = _db.CategorySpecMappings
                .Join(_db.Categories, m => m.CategoryId, c => c.Id,
                      (m, c) => new { m.CategorySpecId, c.Name })
                .ToList();

            return specs.Select(s => new CategorySpecDto
            {
                Id               = s.Id,
                Name             = s.Name,
                InputType        = s.InputType,
                IsRequired       = s.IsRequired,
                AllowCustomInput = s.AllowCustomInput,
                SortOrder        = s.SortOrder,
                IsActive         = s.IsActive,
                Options          = allOptions
                    .Where(o => o.CategorySpecId == s.Id)
                    .OrderBy(o => o.SortOrder)
                    .Select(o => o.OptionName)
                    .ToList(),
                BoundCategoryNames = allMappings
                    .Where(m => m.CategorySpecId == s.Id)
                    .Select(m => m.Name)
                    .ToList()
            }).ToList();
        }

        // ── 取得單一規格 ──
        public CategorySpecDto? GetById(int id)
        {
            var s = _db.CategorySpecs.FirstOrDefault(x => x.Id == id);
            if (s == null) return null;

            return new CategorySpecDto
            {
                Id               = s.Id,
                Name             = s.Name,
                InputType        = s.InputType,
                IsRequired       = s.IsRequired,
                AllowCustomInput = s.AllowCustomInput,
                SortOrder        = s.SortOrder,
                IsActive         = s.IsActive,
                Options          = _db.CategorySpecOptions
                    .Where(o => o.CategorySpecId == id)
                    .OrderBy(o => o.SortOrder)
                    .Select(o => o.OptionName)
                    .ToList()
            };
        }

        // ── 新增 ──
        public void Create(string name, string inputType, bool isRequired,
                           bool allowCustomInput, int sortOrder, List<string> options)
        {
            var spec = new CategorySpec
            {
                Name             = name,
                InputType        = inputType,
                IsRequired       = isRequired,
                AllowCustomInput = allowCustomInput,
                SortOrder        = sortOrder,
                IsActive         = true
            };
            _db.CategorySpecs.Add(spec);
            _db.SaveChanges();
            WriteOptions(spec.Id, options);
            _db.SaveChanges();
        }

        // ── 更新 ──
        public void Update(int id, string name, string inputType,
                           bool isRequired, bool allowCustomInput, int sortOrder, List<string> options)
        {
            var spec = _db.CategorySpecs.FirstOrDefault(s => s.Id == id);
            if (spec == null) return;

            spec.Name             = name;
            spec.InputType        = inputType;
            spec.IsRequired       = isRequired;
            spec.AllowCustomInput = allowCustomInput;
            spec.SortOrder        = sortOrder;

            var old = _db.CategorySpecOptions.Where(o => o.CategorySpecId == id).ToList();
            _db.CategorySpecOptions.RemoveRange(old);
            WriteOptions(id, options);
            _db.SaveChanges();
        }

        // ── 刪除 ──
        public void Delete(int id)
        {
            var mappings = _db.CategorySpecMappings.Where(m => m.CategorySpecId == id).ToList();
            _db.CategorySpecMappings.RemoveRange(mappings);

            var opts = _db.CategorySpecOptions.Where(o => o.CategorySpecId == id).ToList();
            _db.CategorySpecOptions.RemoveRange(opts);

            var spec = _db.CategorySpecs.FirstOrDefault(s => s.Id == id);
            if (spec != null) _db.CategorySpecs.Remove(spec);

            _db.SaveChanges();
        }

        // ── 啟用/停用 ──
        public void UpdateIsActive(int id, bool isActive)
        {
            var spec = _db.CategorySpecs.FirstOrDefault(s => s.Id == id);
            if (spec == null) return;
            spec.IsActive = isActive;
            _db.SaveChanges();
        }

        // ── 分類綁定：取得某分類已綁定的規格 ID 列表（含 IsFilterable）──
        public List<BoundSpecItem> GetBoundSpecItems(int categoryId)
        {
            return _db.CategorySpecMappings
                .Where(m => m.CategoryId == categoryId)
                .Select(m => new BoundSpecItem
                {
                    AttributeId  = m.CategorySpecId,
                    IsFilterable = m.IsFilterable
                })
                .ToList();
        }

        // ── 分類綁定：取得已綁定的規格 ID（供矩陣用）──
        public List<int> GetBoundAttributeIds(int categoryId)
        {
            return _db.CategorySpecMappings
                .Where(m => m.CategoryId == categoryId)
                .Select(m => m.CategorySpecId)
                .ToList();
        }

        // ── 即時切換單一綁定 ──
        public void ToggleBinding(int categoryId, int specId, bool isBound)
        {
            var existing = _db.CategorySpecMappings
                .FirstOrDefault(m => m.CategoryId == categoryId && m.CategorySpecId == specId);

            if (isBound && existing == null)
            {
                _db.CategorySpecMappings.Add(new CategorySpecMapping
                {
                    CategoryId     = categoryId,
                    CategorySpecId = specId,
                    IsFilterable   = false
                });
            }
            else if (!isBound && existing != null)
            {
                _db.CategorySpecMappings.Remove(existing);
            }
            _db.SaveChanges();
        }

        // ── 即時切換 IsFilterable ──
        public void ToggleFilterable(int categoryId, int specId, bool isFilterable)
        {
            var mapping = _db.CategorySpecMappings
                .FirstOrDefault(m => m.CategoryId == categoryId && m.CategorySpecId == specId);
            if (mapping == null) return;
            mapping.IsFilterable = isFilterable;
            _db.SaveChanges();
        }

        // ── 取得矩陣資料用 ──
        public List<object> GetAllBindingPairs()
        {
            return _db.CategorySpecMappings
                .Select(m => new {
                    categoryId   = m.CategoryId,
                    attributeId  = m.CategorySpecId,
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
            var mappings = _db.CategorySpecMappings
                .Where(m => m.CategoryId == categoryId)
                .OrderBy(m => m.Sort)
                .ThenBy(m => m.CategorySpecId)
                .ToList();

            if (!mappings.Any()) return new List<BoundSpecDetailDto>();

            var specIds  = mappings.Select(m => m.CategorySpecId).ToList();
            var specs    = _db.CategorySpecs.Where(s => specIds.Contains(s.Id)).ToList();
            var options  = _db.CategorySpecOptions
                .Where(o => specIds.Contains(o.CategorySpecId))
                .OrderBy(o => o.SortOrder)
                .ToList();

            return mappings.Select(m =>
            {
                var spec = specs.First(s => s.Id == m.CategorySpecId);
                return new BoundSpecDetailDto
                {
                    SpecId           = m.CategorySpecId,
                    Name             = spec.Name,
                    InputType        = spec.InputType,
                    IsRequired       = spec.IsRequired,
                    AllowCustomInput = spec.AllowCustomInput,
                    IsFilterable     = m.IsFilterable,
                    Sort             = m.Sort,
                    Options          = options
                        .Where(o => o.CategorySpecId == m.CategorySpecId)
                        .Select(o => o.OptionName)
                        .ToList()
                };
            }).ToList();
        }

        public void BindSpec(int categoryId, int specId)
        {
            if (_db.CategorySpecMappings.Any(m => m.CategoryId == categoryId && m.CategorySpecId == specId))
                return;

            var maxSort = _db.CategorySpecMappings
                .Where(m => m.CategoryId == categoryId)
                .Select(m => (int?)m.Sort)
                .Max() ?? 0;

            _db.CategorySpecMappings.Add(new CategorySpecMapping
            {
                CategoryId     = categoryId,
                CategorySpecId = specId,
                IsFilterable   = false,
                Sort           = maxSort + 1
            });
            _db.SaveChanges();
        }

        public void UnbindSpec(int categoryId, int specId)
        {
            var mapping = _db.CategorySpecMappings
                .FirstOrDefault(m => m.CategoryId == categoryId && m.CategorySpecId == specId);
            if (mapping == null) return;
            _db.CategorySpecMappings.Remove(mapping);
            _db.SaveChanges();
        }

        public void UpdateBindingSort(int categoryId, List<int> orderedSpecIds)
        {
            var mappings = _db.CategorySpecMappings
                .Where(m => m.CategoryId == categoryId)
                .ToList();

            for (int i = 0; i < orderedSpecIds.Count; i++)
            {
                var m = mappings.FirstOrDefault(x => x.CategorySpecId == orderedSpecIds[i]);
                if (m != null) m.Sort = i + 1;
            }
            _db.SaveChanges();
        }

        public bool HasBindings(int specId)
            => _db.CategorySpecMappings.Any(m => m.CategorySpecId == specId);

        // ── 屬性庫分頁查詢 ────────────────────────────────────────
        public async Task<PagedResult<CategorySpecDto>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = _db.CategorySpecs.OrderBy(s => s.SortOrder).ThenBy(s => s.Id);

            var totalCount = await query.CountAsync();

            var specs = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var specIds = specs.Select(s => s.Id).ToList();

            var options = await _db.CategorySpecOptions
                .Where(o => specIds.Contains(o.CategorySpecId))
                .OrderBy(o => o.SortOrder)
                .ToListAsync();

            var mappings = await _db.CategorySpecMappings
                .Where(m => specIds.Contains(m.CategorySpecId))
                .Join(_db.Categories, m => m.CategoryId, c => c.Id,
                      (m, c) => new { m.CategorySpecId, c.Name })
                .ToListAsync();

            var data = specs.Select(s => new CategorySpecDto
            {
                Id               = s.Id,
                Name             = s.Name,
                InputType        = s.InputType,
                IsRequired       = s.IsRequired,
                AllowCustomInput = s.AllowCustomInput,
                SortOrder        = s.SortOrder,
                IsActive         = s.IsActive,
                Options          = options
                    .Where(o => o.CategorySpecId == s.Id)
                    .Select(o => o.OptionName)
                    .ToList(),
                BoundCategoryNames = mappings
                    .Where(m => m.CategorySpecId == s.Id)
                    .Select(m => m.Name)
                    .ToList()
            }).ToList();

            return PagedResult<CategorySpecDto>.Create(data, totalCount, pageNumber, pageSize);
        }

        // ── 私有：寫入選項 ──
        private void WriteOptions(int specId, List<string> options)
        {
            if (options == null || !options.Any()) return;
            _db.CategorySpecOptions.AddRange(
                options.Where(o => !string.IsNullOrWhiteSpace(o))
                       .Select((o, i) => new CategorySpecOption
                       {
                           CategorySpecId = specId,
                           OptionName     = o.Trim(),
                           SortOrder      = i + 1
                       })
            );
        }
    }
}
