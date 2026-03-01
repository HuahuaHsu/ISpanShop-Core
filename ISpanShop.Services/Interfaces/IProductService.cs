using System.Collections.Generic;
using ISpanShop.Models.DTOs;

namespace ISpanShop.Services.Interfaces
{
    /// <summary>
    /// 商品 Service 介面 - 定義商品相關的商業邏輯操作
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// 建立新商品
        /// </summary>
        /// <param name="dto">商品建立 DTO</param>
        void CreateProduct(ProductCreateDto dto);

        /// <summary>
        /// 取得待審核商品列表
        /// </summary>
        /// <returns>待審核商品 DTO 集合</returns>
        IEnumerable<ProductReviewDto> GetPendingProducts();

        /// <summary>
        /// 變更商品狀態
        /// </summary>
        /// <param name="id">商品 ID</param>
        /// <param name="newStatus">新的狀態值</param>
        void ChangeProductStatus(int id, byte newStatus);

        /// <summary>
        /// 取得所有商品列表
        /// </summary>
        /// <returns>商品列表 DTO 集合</returns>
        IEnumerable<ProductListDto> GetAllProducts();

        /// <summary>
        /// 分頁取得商品列表，支援分類篩選
        /// </summary>
        /// <param name="criteria">搜尋條件（分類篩選 + 分頁）</param>
        /// <returns>分頁商品列表 DTO</returns>
        PagedResult<ProductListDto> GetProductsPaged(ProductSearchCriteria criteria);

        /// <summary>
        /// 取得所有分類清單（含主分類與子分類）
        /// </summary>
        /// <returns>分類 DTO 集合</returns>
        IEnumerable<CategoryDto> GetAllCategories();

        /// <summary>
        /// 根據 ID 取得商品詳情（包含圖片與規格）
        /// </summary>
        /// <param name="id">商品 ID</param>
        /// <returns>商品詳情 DTO，若不存在則返回 null</returns>
        ProductDetailDto? GetProductDetail(int id);
    }
}
