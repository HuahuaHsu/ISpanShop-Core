using System.Collections.Generic;
using ISpanShop.Models.EfModels;

namespace ISpanShop.Repositories.Interfaces
{
    /// <summary>
    /// 商品 Repository 介面 - 定義商品相關的資料存取操作
    /// </summary>
    public interface IProductRepository
    {
        /// <summary>
        /// 新增商品 - 包含規格變體與圖片的完整新增
        /// </summary>
        /// <param name="product">商品實體</param>
        void AddProduct(Product product);

        /// <summary>
        /// 檢查 SKU 代碼是否已存在
        /// </summary>
        /// <param name="skuCode">SKU 代碼</param>
        /// <returns>true 表示已存在，false 表示不存在</returns>
        bool IsSkuExists(string skuCode);

        /// <summary>
        /// 取得待審核商品列表 (Status == 2)
        /// </summary>
        /// <returns>待審核商品集合</returns>
        IEnumerable<Product> GetPendingProducts();

        /// <summary>
        /// 更新商品狀態
        /// </summary>
        /// <param name="id">商品 ID</param>
        /// <param name="status">新的狀態值</param>
        void UpdateProductStatus(int id, byte status);

        /// <summary>
        /// 取得所有商品列表
        /// </summary>
        /// <returns>所有商品集合</returns>
        IEnumerable<Product> GetAllProducts();

        /// <summary>
        /// 根據 ID 取得商品詳情（包含圖片與規格）
        /// </summary>
        /// <param name="id">商品 ID</param>
        /// <returns>商品實體，若不存在則返回 null</returns>
        Product? GetProductById(int id);
    }
}
