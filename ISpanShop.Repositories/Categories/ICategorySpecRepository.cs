using System.Collections.Generic;
using System.Threading.Tasks;
using ISpanShop.Models.DTOs.Categories;
using ISpanShop.Models.DTOs.Common;

namespace ISpanShop.Repositories.Categories
{
    public interface ICategorySpecRepository
    {
        IEnumerable<CategorySpecDto> GetAll();
        CategorySpecDto? GetById(int id);
        void Create(string name, string inputType, bool isRequired, bool allowCustomInput, int sortOrder, List<string> options);
        void Update(int id, string name, string inputType, bool isRequired, bool allowCustomInput, int sortOrder, List<string> options);
        void Delete(int id);
        void UpdateIsActive(int id, bool isActive);
        List<int> GetBoundAttributeIds(int categoryId);
        List<BoundSpecItem> GetBoundSpecItems(int categoryId);
        void ToggleBinding(int categoryId, int specId, bool isBound);
        void ToggleFilterable(int categoryId, int specId, bool isFilterable);
        List<object> GetAllBindingPairs();
        IEnumerable<CategoryDto> GetAllCategories();

        // ── 新版綁定管理 ─────────────────────────────────────────
        List<BoundSpecDetailDto> GetBoundSpecsWithDetails(int categoryId);
        void BindSpec(int categoryId, int specId);
        void UnbindSpec(int categoryId, int specId);
        void UpdateBindingSort(int categoryId, List<int> orderedSpecIds);
        bool HasBindings(int specId);

        // ── 屬性庫分頁查詢 ───────────────────────────────────────
        Task<PagedResult<CategorySpecDto>> GetPagedAsync(int pageNumber, int pageSize);
    }
}
