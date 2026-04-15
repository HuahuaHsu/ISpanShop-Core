using System.Collections.Generic;
using System.Threading.Tasks;
using ISpanShop.Models.DTOs.Categories;
using ISpanShop.Models.DTOs.Common;
using ISpanShop.Repositories.Products;
using ISpanShop.Repositories.Categories;
using ISpanShop.Repositories.Inventories;

namespace ISpanShop.Services.Categories
{
    public class CategoryAttributeService
    {
        private readonly ICategoryAttributeRepository _categoryAttributeRepository;

        public CategoryAttributeService(ICategoryAttributeRepository categoryAttributeRepository)
        {
            _categoryAttributeRepository = categoryAttributeRepository;
        }

        public IEnumerable<CategorySpecDto> GetAll()
        {
            return _categoryAttributeRepository.GetAll();
        }

        public CategorySpecDto? GetById(int id)
        {
            return _categoryAttributeRepository.GetById(id);
        }

        public void Create(string name, string inputType, bool isRequired, bool allowCustomInput, int sortOrder, List<string> options)
        {
            var cleanOptions = NeedsOptions(inputType) ? options : new List<string>();
            _categoryAttributeRepository.Create(name, inputType, isRequired, allowCustomInput, sortOrder, cleanOptions);
        }

        public void Update(int id, string name, string inputType, bool isRequired, bool allowCustomInput, int sortOrder, List<string> options)
        {
            var cleanOptions = NeedsOptions(inputType) ? options : new List<string>();
            _categoryAttributeRepository.Update(id, name, inputType, isRequired, allowCustomInput, sortOrder, cleanOptions);
        }

        public void Delete(int id)
        {
            _categoryAttributeRepository.Delete(id);
        }

        public void UpdateIsActive(int id, bool isActive)
        {
            _categoryAttributeRepository.UpdateIsActive(id, isActive);
        }

        // ── 分類綁定相關 ──────────────────────────────

        public IEnumerable<CategoryDto> GetAllCategories()
        {
            return _categoryAttributeRepository.GetAllCategories();
        }

        public List<int> GetBoundAttributeIds(int categoryId)
        {
            return _categoryAttributeRepository.GetBoundAttributeIds(categoryId);
        }

        public List<BoundSpecItem> GetBoundSpecItems(int categoryId)
        {
            return _categoryAttributeRepository.GetBoundSpecItems(categoryId);
        }

        public void ToggleBinding(int categoryId, int specId, bool isBound)
        {
            _categoryAttributeRepository.ToggleBinding(categoryId, specId, isBound);
        }

        public void ToggleFilterable(int categoryId, int specId, bool isFilterable)
        {
            _categoryAttributeRepository.ToggleFilterable(categoryId, specId, isFilterable);
        }

        public List<object> GetAllBindingPairs()
        {
            return _categoryAttributeRepository.GetAllBindingPairs();
        }

        // ── 新版綁定管理 ─────────────────────────────────────────

        public List<BoundSpecDetailDto> GetBoundSpecsWithDetails(int categoryId)
            => _categoryAttributeRepository.GetBoundSpecsWithDetails(categoryId);

        public void BindSpec(int categoryId, int specId)
            => _categoryAttributeRepository.BindSpec(categoryId, specId);

        public void UnbindSpec(int categoryId, int specId)
            => _categoryAttributeRepository.UnbindSpec(categoryId, specId);

        public void UpdateBindingSort(int categoryId, List<int> orderedSpecIds)
            => _categoryAttributeRepository.UpdateBindingSort(categoryId, orderedSpecIds);

        public bool HasBindings(int specId)
            => _categoryAttributeRepository.HasBindings(specId);

        public async Task<PagedResult<CategorySpecDto>> GetPagedAsync(int pageNumber, int pageSize)
            => await _categoryAttributeRepository.GetPagedAsync(pageNumber, pageSize);

        private static bool NeedsOptions(string inputType)
        {
            return inputType == "select"
                || inputType == "checkbox"
                || inputType == "radio";
        }
    }
}
