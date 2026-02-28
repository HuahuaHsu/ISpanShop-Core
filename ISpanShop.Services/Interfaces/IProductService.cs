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
    }
}
