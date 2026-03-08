using System.Collections.Generic;
using System.Threading.Tasks;
using ISpanShop.Models.DTOs.Categories;

namespace ISpanShop.Repositories.Categories
{
    public interface ICategoryManageRepository
    {
        IEnumerable<CategoryManageDto> GetTree();
        CategoryManageDto? GetById(int id);
        int GetProductCount(int categoryId);
        void Create(string name, string? nameEn, int? parentId, int sortOrder, string? imageUrl);
        void Update(int id, string name, string? nameEn, int? parentId, int sortOrder, string? imageUrl);
        Task DeleteAsync(int id);
        void UpdateIsActive(int id, bool isActive);
        void UpdateSortOrder(int id, int sortOrder);
    }
}
