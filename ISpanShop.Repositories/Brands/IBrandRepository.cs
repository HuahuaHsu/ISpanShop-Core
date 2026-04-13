using System.Collections.Generic;
using System.Threading.Tasks;
using ISpanShop.Models.DTOs.Brands;

namespace ISpanShop.Repositories.Brands
{
    public interface IBrandRepository
    {
        /// <summary>
        /// [Async] 取得有上架商品的品牌列表，含商品數。
        /// subCategoryId 優先（直接篩）；categoryId 主分類自動展開子分類；keyword 篩選品牌名稱。
        /// </summary>
        Task<IEnumerable<BrandWithCountDto>> GetBrandsAsync(
            int? categoryId, string? keyword, int? subCategoryId = null);
    }
}
