using System.Collections.Generic;
using ISpanShop.Models.DTOs;
using ISpanShop.Repositories.Interfaces;

namespace ISpanShop.Services
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

        public bool Delete(int id)
        {
            var cat = _repo.GetById(id);
            if (cat == null) return false;
            if (cat.ChildCount > 0) return false;
            if (cat.ProductCount > 0) return false;
            _repo.Delete(id);
            return true;
        }

        public void UpdateIsActive(int id, bool isActive) => _repo.UpdateIsActive(id, isActive);
        public void UpdateSortOrder(int id, int sortOrder) => _repo.UpdateSortOrder(id, sortOrder);
    }
}
