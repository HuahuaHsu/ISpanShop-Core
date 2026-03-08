using System.Collections.Generic;
using System.Threading.Tasks;
using ISpanShop.Models.DTOs.Categories;
using ISpanShop.Models.DTOs.Common;
using ISpanShop.Repositories.Products;
using ISpanShop.Repositories.Categories;
using ISpanShop.Repositories.Inventories;

namespace ISpanShop.Services.Categories
{
    public class CategorySpecService
    {
        private readonly ICategorySpecRepository _categorySpecRepository;

        public CategorySpecService(ICategorySpecRepository categorySpecRepository)
        {
            _categorySpecRepository = categorySpecRepository;
        }

        public IEnumerable<CategorySpecDto> GetAll()
        {
            return _categorySpecRepository.GetAll();
        }

        public CategorySpecDto? GetById(int id)
        {
            return _categorySpecRepository.GetById(id);
        }

        public void Create(string name, string inputType, bool isRequired, bool allowCustomInput, int sortOrder, List<string> options)
        {
            var cleanOptions = NeedsOptions(inputType) ? options : new List<string>();
            _categorySpecRepository.Create(name, inputType, isRequired, allowCustomInput, sortOrder, cleanOptions);
        }

        public void Update(int id, string name, string inputType, bool isRequired, bool allowCustomInput, int sortOrder, List<string> options)
        {
            var cleanOptions = NeedsOptions(inputType) ? options : new List<string>();
            _categorySpecRepository.Update(id, name, inputType, isRequired, allowCustomInput, sortOrder, cleanOptions);
        }

        public void Delete(int id)
        {
            _categorySpecRepository.Delete(id);
        }

        public void UpdateIsActive(int id, bool isActive)
        {
            _categorySpecRepository.UpdateIsActive(id, isActive);
        }

        // ── 分類綁定相關 ──────────────────────────────

        public IEnumerable<CategoryDto> GetAllCategories()
        {
            return _categorySpecRepository.GetAllCategories();
        }

        public List<int> GetBoundAttributeIds(int categoryId)
        {
            return _categorySpecRepository.GetBoundAttributeIds(categoryId);
        }

        public List<BoundSpecItem> GetBoundSpecItems(int categoryId)
        {
            return _categorySpecRepository.GetBoundSpecItems(categoryId);
        }

        public void ToggleBinding(int categoryId, int specId, bool isBound)
        {
            _categorySpecRepository.ToggleBinding(categoryId, specId, isBound);
        }

        public void ToggleFilterable(int categoryId, int specId, bool isFilterable)
        {
            _categorySpecRepository.ToggleFilterable(categoryId, specId, isFilterable);
        }

        public List<object> GetAllBindingPairs()
        {
            return _categorySpecRepository.GetAllBindingPairs();
        }

        // ── 新版綁定管理 ─────────────────────────────────────────

        public List<BoundSpecDetailDto> GetBoundSpecsWithDetails(int categoryId)
            => _categorySpecRepository.GetBoundSpecsWithDetails(categoryId);

        public void BindSpec(int categoryId, int specId)
            => _categorySpecRepository.BindSpec(categoryId, specId);

        public void UnbindSpec(int categoryId, int specId)
            => _categorySpecRepository.UnbindSpec(categoryId, specId);

        public void UpdateBindingSort(int categoryId, List<int> orderedSpecIds)
            => _categorySpecRepository.UpdateBindingSort(categoryId, orderedSpecIds);

        public bool HasBindings(int specId)
            => _categorySpecRepository.HasBindings(specId);

        public async Task<PagedResult<CategorySpecDto>> GetPagedAsync(int pageNumber, int pageSize)
            => await _categorySpecRepository.GetPagedAsync(pageNumber, pageSize);

        private static bool NeedsOptions(string inputType)
        {
            return inputType == "select"
                || inputType == "checkbox"
                || inputType == "radio";
        }
    }
}
