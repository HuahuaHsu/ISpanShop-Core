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
    }
}
