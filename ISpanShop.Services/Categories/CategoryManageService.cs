using System.Collections.Generic;
using System.Threading.Tasks;
using ISpanShop.Models.DTOs.Categories;
using ISpanShop.Repositories.Products;
using ISpanShop.Repositories.Categories;
using ISpanShop.Repositories.Inventories;

namespace ISpanShop.Services.Categories
{
    public class CategoryManageService
    {
        private readonly ICategoryManageRepository _repo;
        public CategoryManageService(ICategoryManageRepository repo) => _repo = repo;

        public IEnumerable<CategoryManageDto> GetTree() => _repo.GetTree();
        public CategoryManageDto? GetById(int id) => _repo.GetById(id);
        public int GetProductCount(int categoryId) => _repo.GetProductCount(categoryId);

        public void Create(string name, string? nameEn, int? parentId, int sortOrder, string? imageUrl)
            => _repo.Create(name, nameEn, parentId, sortOrder, imageUrl);

        public void Update(int id, string name, string? nameEn, int? parentId, int sortOrder, string? imageUrl)
            => _repo.Update(id, name, nameEn, parentId, sortOrder, imageUrl);

        public async Task DeleteAsync(int id) => await _repo.DeleteAsync(id);

        public void UpdateIsActive(int id, bool isActive) => _repo.UpdateIsActive(id, isActive);
        public void UpdateSortOrder(int id, int sortOrder) => _repo.UpdateSortOrder(id, sortOrder);
    }
}
