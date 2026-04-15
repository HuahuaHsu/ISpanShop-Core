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

        /// <summary>[Async] 取得啟用中的主分類（ParentId == null, IsVisible == true），含每個主分類底下的上架商品數</summary>
        Task<IEnumerable<CategoryManageDto>> GetMainCategoriesAsync();

        /// <summary>[Async] 取得指定主分類底下所有啟用中的子分類，含各子分類的上架商品數</summary>
        Task<IEnumerable<CategoryManageDto>> GetChildCategoriesAsync(int parentId);
    }
}
