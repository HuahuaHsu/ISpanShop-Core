using System.Collections.Generic;
using System.Threading.Tasks;
using ISpanShop.Models.DTOs;
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

        /// <summary>
        /// 分頁取得商品列表，支援分類篩選
        /// </summary>
        /// <param name="criteria">搜尋條件（分類篩選 + 分頁）</param>
        /// <returns>商品集合與總筆數</returns>
        (IEnumerable<Product> Items, int TotalCount) GetProductsPaged(ProductSearchCriteria criteria);

        /// <summary>
        /// 取得所有分類（含父子關聯）
        /// </summary>
        /// <returns>所有分類實體集合</returns>
        IEnumerable<Category> GetAllCategories();

        /// <summary>
        /// 取得所有商家清單 (Id, StoreName)
        /// </summary>
        /// <returns>商家清單</returns>
        IEnumerable<(int Id, string Name)> GetStoreOptions();

        /// <summary>
        /// 取得所有品牌清單 (Id, Name)
        /// </summary>
        /// <returns>品牌清單</returns>
        IEnumerable<(int Id, string Name)> GetBrandOptions();

        /// <summary>
        /// 根據子分類取得該分類下商品涵蓋的品牌清單
        /// </summary>
        /// <param name="categoryId">子分類 ID；為 null 時回傳全部品牌</param>
        /// <returns>品牌清單</returns>
        IEnumerable<(int Id, string Name)> GetBrandsByCategory(int? categoryId);

        /// <summary>
        /// 批次更新商品上下架狀態
        /// </summary>
        /// <param name="productIds">要更新的商品 ID 集合</param>
        /// <param name="targetStatus">目標狀態：1 為上架，0 為下架</param>
        /// <returns>實際更新的筆數</returns>
        Task<int> UpdateBatchStatusAsync(List<int> productIds, byte targetStatus);

        /// <summary>
        /// 核准商品審核（Status → 1 上架）
        /// </summary>
        /// <param name="id">商品 ID</param>
        void ApproveProduct(int id);

        /// <summary>
        /// 退回商品審核（Status → 3 審核退回）
        /// </summary>
        /// <param name="id">商品 ID</param>
        void RejectProduct(int id, string? reason);

        /// <summary>
        /// 取得最近退回的商品清單（Status == 3），依 UpdatedAt 降冪排序
        /// </summary>
        /// <param name="top">最多取幾筆</param>
        IEnumerable<Product> GetRecentRejectedProducts(int top);
    }
}
