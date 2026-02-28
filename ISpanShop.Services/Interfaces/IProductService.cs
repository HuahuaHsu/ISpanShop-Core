using ISpanShop.Models.DTOs;
using ISpanShop.Models.EfModels;

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
    }
}
