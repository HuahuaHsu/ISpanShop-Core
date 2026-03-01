using System.Collections.Generic;
using System.Threading.Tasks;
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

        /// <summary>
        /// 取得所有商家清單 (Id, Name)
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
        /// 核准商品（待審核 → 上架）
        /// </summary>
        /// <param name="id">商品 ID</param>
        void ApproveProduct(int id);

        /// <summary>
        /// 退回商品（待審核 → 審核退回）
        /// </summary>
        /// <param name="id">商品 ID</param>
        /// <param name="reason">退回原因（記錄於回應訊息，不持久化）</param>
        void RejectProduct(int id, string? reason);

        /// <summary>
        /// 取得最近退回的商品清單（Status == 3），依 UpdatedAt 降冪排序
        /// </summary>
        /// <param name="top">最多取幾筆，預設 10</param>
        IEnumerable<ProductReviewDto> GetRecentRejectedProducts(int top = 10);
    }
}
