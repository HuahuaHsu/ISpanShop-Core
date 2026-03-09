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
        /// 變更商品狀態
        /// </summary>
        void ChangeProductStatus(int id, byte newStatus);

        /// <summary>
        /// 取得所有分類清單（含主分類與子分類）
        /// </summary>
        IEnumerable<CategoryDto> GetAllCategories();

        /// <summary>
        /// 根據 ID 取得商品詳情（包含圖片與規格）
        /// </summary>
        ProductDetailDto? GetProductDetail(int id);

        /// <summary>
        /// 建立新商品
        /// </summary>
        void CreateProduct(ProductCreateDto dto);

        /// <summary>
        /// 分頁取得商品列表，支援分類篩選
        /// </summary>
        PagedResult<ProductListDto> GetProductsPaged(ProductSearchCriteria criteria);

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
        /// 為商品新增規格（自動產生 SKU、重算價格區間）。回傳 null 表示成功；回傳錯誤訊息字串表示失敗。
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

        /// <summary>
        /// 取得所有商家清單 (Id, Name)
        /// </summary>
        IEnumerable<(int Id, string Name)> GetStoreOptions();

        /// <summary>
        /// 取得所有品牌清單 (Id, Name)
        /// </summary>
        IEnumerable<(int Id, string Name)> GetBrandOptions();

        /// <summary>
        /// 根據子分類取得該分類下商品涵蓋的品牌清單
        /// </summary>
        IEnumerable<(int Id, string Name)> GetBrandsByCategory(int? categoryId);

        /// <summary>
        /// 批次更新商品上下架狀態
        /// </summary>
        Task<int> UpdateBatchStatusAsync(List<int> productIds, byte targetStatus);

        // ═══════════════════════════════════════════════════════════
        //  非同步版本（效能最佳化：async/await + 投影 + 真分頁）
        // ═══════════════════════════════════════════════════════════

        /// <summary>[Async] 取得待審核商品列表</summary>
        Task<IEnumerable<ProductReviewDto>> GetPendingProductsAsync();

        /// <summary>[Async] 分頁取得商品列表，支援分類篩選</summary>
        Task<PagedResult<ProductListDto>> GetProductsPagedAsync(ProductSearchCriteria criteria);

        /// <summary>[Async] 核准商品（待審核 → 上架，記錄審核人）</summary>
        Task ApproveProductAsync(int id, string adminId);

        /// <summary>[Async] 退回商品（待審核 → 審核退回，記錄原因、審核人與退回時間）</summary>
        Task RejectProductAsync(int id, string adminId, string reason);

        /// <summary>[Async] 提交商品審核：自動違禁詞攔截或轉為待審核佇列</summary>
        Task SubmitProductForReviewAsync(int productId);

        /// <summary>[Async] 清理過期退回商品（IsDeleted=true），回傳清理筆數</summary>
        Task<int> CleanupExpiredRejectedProductsAsync(int expirationSeconds = 60);

        /// <summary>[Async] 取得最近退回的商品清單（ReviewStatus == 2），依 UpdatedAt 降冪排序</summary>
        Task<IEnumerable<ProductReviewDto>> GetRecentRejectedProductsAsync(int top = 10);

        /// <summary>[Async] 分頁取得待審核商品（ReviewStatus == 0）</summary>
        Task<PagedResult<ProductReviewDto>> GetPendingProductsPagedAsync(int page, int pageSize);

        /// <summary>[Async] 分頁取得已退回商品（ReviewStatus == 2）</summary>
        Task<PagedResult<ProductReviewDto>> GetRejectedProductsPagedAsync(int page, int pageSize);

        /// <summary>[Async] 重設為待審核：清空審核結果欄位，商品回到 Status=2 / ReviewStatus=0</summary>
        Task ResetToPendingAsync(int productId);

        /// <summary>[Async] 取得全站商品各狀態統計數字</summary>
        Task<(int Total, int Published, int Unpublished, int Pending)> GetProductStatusCountsAsync();

        /// <summary>[Async] 管理員強制下架（儲存下架原因）</summary>
        Task ForceUnpublishAsync(int id, string? reason);

        /// <summary>[Async] 模擬系統自動審核：新增測試商品並執行違禁詞批次審核，回傳攔截與放行筆數</summary>
        Task<SimulateAutoReviewResult> SimulateAutoReviewAsync();
    }
}
