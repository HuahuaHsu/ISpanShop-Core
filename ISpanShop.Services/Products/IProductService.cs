using System.Collections.Generic;
using System.Threading.Tasks;
using ISpanShop.Models.DTOs.Products;
using ISpanShop.Models.DTOs.Categories;
using ISpanShop.Models.DTOs.Common;

namespace ISpanShop.Services.Products
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
        /// 核准商品（待審核 → 上架，記錄審核人）
        /// </summary>
        void ApproveProduct(int id, string adminId);

        /// <summary>
        /// 退回商品（待審核 → 審核退回，記錄原因、審核人與退回時間）
        /// </summary>
        void RejectProduct(int id, string adminId, string reason);

        /// <summary>
        /// 提交商品審核：自動違禁詞攔截（立即退回）或轉為待審核佇列
        /// </summary>
        void SubmitProductForReview(int productId);

        /// <summary>
        /// [Demo 專用] 清理過期退回商品（IsDeleted=true），回傳清理筆數
        /// </summary>
        int CleanupExpiredRejectedProducts(int expirationSeconds = 60);

        /// <summary>
        /// 取得最近退回的商品清單（Status == 3），依 UpdatedAt 降冪排序
        /// </summary>
        /// <param name="top">最多取幾筆，預設 10</param>
        IEnumerable<ProductReviewDto> GetRecentRejectedProducts(int top = 10);

        /// <summary>
        /// 分頁取得待審核商品（ReviewStatus == 0）
        /// </summary>
        PagedResult<ProductReviewDto> GetPendingProductsPaged(int page, int pageSize);

        /// <summary>
        /// 分頁取得已退回商品（ReviewStatus == 2）
        /// </summary>
        PagedResult<ProductReviewDto> GetRejectedProductsPaged(int page, int pageSize);

        /// <summary>
        /// 重設為待審核：清空審核結果欄位，商品回到 Status=2 / ReviewStatus=0
        /// </summary>
        void ResetToPending(int productId);

        /// <summary>
        /// 管理員後台新增商品（略過審核，直接設為上架）
        /// </summary>
        void CreateProductAdmin(ProductAdminCreateDto dto);

        /// <summary>
        /// 更新商品基本資料
        /// </summary>
        void UpdateProduct(ProductUpdateDto dto);

        /// <summary>
        /// 軟刪除商品（IsDeleted = true）
        /// </summary>
        void SoftDeleteProduct(int id);

        /// <summary>
        /// 根據 ID 取得規格詳情 DTO
        /// </summary>
        ProductVariantDetailDto? GetVariantById(int id);

        /// <summary>
        /// 為商品新增規格（自動產生 SKU、重算價格區間）。
        /// 回傳 null 表示成功；回傳錯誤訊息字串表示失敗。
        /// </summary>
        string? AddVariant(int productId, ProductVariantCreateDto dto);

        /// <summary>
        /// 更新規格（SKU/售價/庫存/安全庫存）並重算商品價格區間
        /// </summary>
        void UpdateVariant(ProductVariantUpdateDto dto);

        /// <summary>
        /// 軟刪除規格（IsDeleted = true）並重算商品價格區間
        /// </summary>
        void SoftDeleteVariant(int id);

        /// <summary>取得全站商品各狀態統計數字</summary>
        (int Total, int Published, int Unpublished, int Pending) GetProductStatusCounts();

        /// <summary>管理員強制下架（儲存下架原因）</summary>
        void ForceUnpublish(int id, string? reason);

        // ═══════════════════════════════════════════════════════════
        //  非同步版本（效能最佳化：async/await + 投影 + 真分頁）
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// [Async] 取得待審核商品列表
        /// </summary>
        Task<IEnumerable<ProductReviewDto>> GetPendingProductsAsync();

        /// <summary>
        /// [Async] 分頁取得商品列表，支援分類篩選
        /// </summary>
        Task<PagedResult<ProductListDto>> GetProductsPagedAsync(ProductSearchCriteria criteria);

        /// <summary>
        /// [Async] 核准商品（待審核 → 上架，記錄審核人）
        /// </summary>
        Task ApproveProductAsync(int id, string adminId);

        /// <summary>
        /// [Async] 退回商品（待審核 → 審核退回，記錄原因、審核人與退回時間）
        /// </summary>
        Task RejectProductAsync(int id, string adminId, string reason);

        /// <summary>
        /// [Async] 提交商品審核：自動違禁詞攔截或轉為待審核佇列
        /// </summary>
        Task SubmitProductForReviewAsync(int productId);

        /// <summary>
        /// [Async] 清理過期退回商品（IsDeleted=true），回傳清理筆數
        /// </summary>
        Task<int> CleanupExpiredRejectedProductsAsync(int expirationSeconds = 60);

        /// <summary>
        /// [Async] 取得最近退回的商品清單（ReviewStatus == 2），依 UpdatedAt 降冪排序
        /// </summary>
        Task<IEnumerable<ProductReviewDto>> GetRecentRejectedProductsAsync(int top = 10);

        /// <summary>
        /// [Async] 分頁取得待審核商品（ReviewStatus == 0）
        /// </summary>
        Task<PagedResult<ProductReviewDto>> GetPendingProductsPagedAsync(int page, int pageSize);

        /// <summary>
        /// [Async] 分頁取得已退回商品（ReviewStatus == 2）
        /// </summary>
        Task<PagedResult<ProductReviewDto>> GetRejectedProductsPagedAsync(int page, int pageSize);

        /// <summary>
        /// [Async] 重設為待審核：清空審核結果欄位，商品回到 Status=2 / ReviewStatus=0
        /// </summary>
        Task ResetToPendingAsync(int productId);

        /// <summary>[Async] 取得全站商品各狀態統計數字</summary>
        Task<(int Total, int Published, int Unpublished, int Pending)> GetProductStatusCountsAsync();

        /// <summary>[Async] 管理員強制下架（儲存下架原因）</summary>
        Task ForceUnpublishAsync(int id, string? reason);
    }
}
