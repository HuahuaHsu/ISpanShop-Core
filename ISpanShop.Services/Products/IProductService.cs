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
        int CreateProduct(ProductCreateDto dto);

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

        /// <summary>
        /// 批次更新商品審核狀態
        /// </summary>
        Task<int> UpdateBatchReviewStatusAsync(List<int> productIds, int targetReviewStatus, string adminId);

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

        /// <summary>[Async] 取得全站商品各狀態統計數字（Unpublished 不含強制下架）</summary>
        Task<(int Total, int Published, int Unpublished, int Pending, int ForcedOffShelf)> GetProductStatusCountsAsync();

        /// <summary>[Async] 管理員強制下架（Status→4，儲存下架原因與操作人）</summary>
        Task ForceUnpublishAsync(int id, string? reason, int? adminBy);

        /// <summary>[Async] 批次強制下架</summary>
        Task<int> BatchForceOffShelfAsync(List<int> ids, string? reason, int? adminBy);

        /// <summary>[Async] 分頁取得重新申請審核商品（ReviewStatus==3）</summary>
        Task<PagedResult<ProductReviewDto>> GetReApplyProductsPagedAsync(int page, int pageSize);

        /// <summary>[Async] 賣家申請重新上架</summary>
        Task ReApplyAsync(int id);

        /// <summary>[Async] 管理員核准強制下架商品重新上架</summary>
        Task ApproveForcedProductAsync(int id, string adminId);

        /// <summary>[Async] 管理員駁回重新申請</summary>
        Task RejectForcedProductAsync(int id, string adminId, string reason);

        /// <summary>[Async] 模擬系統自動審核：對所有待審核商品執行敏感字比對，回傳詳細結果</summary>
        Task<SimulateAutoReviewResult> SimulateAutoReviewAsync();

        /// <summary>[Async] 生成測試商品（乾淨 5 筆 / 高風險 5 筆 / 邊緣 5 筆，共 15 筆，全為待審核）</summary>
        Task<GenerateTestProductsResult> GenerateTestProductsAsync();

        /// <summary>[Async] 取得近期審核通過的商品（最近 N 小時內 ReviewStatus=1）</summary>
        Task<IEnumerable<ProductReviewDto>> GetRecentlyApprovedAsync(int hours = 24);

        /// <summary>[Async] 分頁取得近期審核通過的商品（最近 N 小時內 ReviewStatus=1）</summary>
        Task<PagedResult<ProductReviewDto>> GetRecentlyApprovedPagedAsync(int page, int pageSize, int hours = 24);

        /// <summary>[Async] 模擬賣家修改後重新送審：將 ReviewStatus=2（已退回）的商品改為 ReviewStatus=3（待重新審核），出現在重新申請審核列表。</summary>
        Task SimulateSellerResubmitAsync(int id);

        // ═══════════════════════════════════════════════════════════
        //  前台商品列表
        // ═══════════════════════════════════════════════════════════

        /// <summary>[Async] 前台商品總覽：只查上架中商品，支援分類/關鍵字/品牌/價格/排序/分頁。pageSize 上限 50。</summary>
        Task<PagedResult<ProductListDto>> GetFrontActiveProductsAsync(
            int? categoryId, string? keyword, string sortBy, int page, int pageSize,
            int? subCategoryId = null, int[]? brandIds = null,
            decimal? minPrice = null, decimal? maxPrice = null);

        // ═══════════════════════════════════════════════════════════
        //  前台商品詳情頁
        // ═══════════════════════════════════════════════════════════

        /// <summary>[Async] 前台商品詳情：載入所有關聯資料，連同評分統計與商店商品數一併回傳。
        /// 回傳 null 代表找不到或已下架。</summary>
        Task<(ISpanShop.Models.EfModels.Product? Product, decimal? Rating, int ReviewCount, int StoreProductCount)>
            GetProductDetailAsync(int id);

        /// <summary>[Async] 取得同子分類相關商品（排除自身、只取上架中、依銷量排序）。</summary>
        Task<IEnumerable<ProductListDto>> GetRelatedProductsAsync(int productId, int categoryId, int limit);

        /// <summary>[Async] 取得熱搜關鍵字（瀏覽次數最高的前 N 筆上架商品名稱，超過 10 字截斷）。</summary>
        Task<List<string>> GetHotKeywordsAsync(int limit = 8);

        /// <summary>批次新增商品圖片（前台賣家上傳，URLs 已儲存至磁碟）。</summary>
        void AddProductImages(int productId, IEnumerable<ISpanShop.Models.EfModels.ProductImage> images);

        /// <summary>刪除商品所有圖片（資料庫記錄 + 實體檔案）。</summary>
        void DeleteProductImages(int productId, string webRootPath);

        /// <summary>刪除商品圖片（排除指定的 URL 列表）。</summary>
        void DeleteProductImagesExcept(int productId, List<string> keepImageUrls, string webRootPath);

        /// <summary>更新商品主圖設定。</summary>
        void UpdateMainImage(int productId, int mainImageIndex);
    }
}
