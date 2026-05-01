using System.Collections.Generic;
using System.Threading.Tasks;
using ISpanShop.Models.DTOs.Products;
using ISpanShop.Models.DTOs.Common;
using ISpanShop.Models.EfModels;

namespace ISpanShop.Repositories.Products
{
    /// <summary>
    /// 商品 Repository 介面 - 定義商品相關的資料存取操作
    /// </summary>
    public interface IProductRepository
    {
        /// <summary>
        /// 根據 ID 取得商品詳情（包含圖片與規格）
        /// </summary>
        Product? GetProductById(int id);

        /// <summary>
        /// 新增商品
        /// </summary>
        void AddProduct(Product product);

        /// <summary>
        /// 檢查 SKU 代碼是否已存在
        /// </summary>
        bool IsSkuExists(string skuCode);

        /// <summary>
        /// 更新商品狀態
        /// </summary>
        void UpdateProductStatus(int id, byte status);

        /// <summary>
        /// 分頁取得商品列表，支援分類篩選與多維度搜尋
        /// </summary>
        (IEnumerable<Product> Items, int TotalCount) GetProductsPaged(ProductSearchCriteria criteria);

        /// <summary>
        /// 更新商品基本資料
        /// </summary>
        void UpdateProduct(ProductUpdateDto dto);

        /// <summary>
        /// 軟刪除商品（設 IsDeleted = true）
        /// </summary>
        void SoftDeleteProduct(int id);

        /// <summary>
        /// 根據 ID 取得規格（含所屬商品）
        /// </summary>
        ProductVariant? GetVariantById(int id);

        /// <summary>
        /// 新增規格並重算商品價格區間
        /// </summary>
        void AddVariant(ProductVariant variant);

        /// <summary>
        /// 更新規格（SKU/售價/庫存/安全庫存）並重算商品價格區間
        /// </summary>
        void UpdateVariant(ProductVariantUpdateDto dto);

        /// <summary>
        /// 軟刪除規格並重算商品價格區間
        /// </summary>
        void SoftDeleteVariant(int id);

        /// <summary>
        /// 取得所有分類（含父子關聯）
        /// </summary>
        IEnumerable<Category> GetAllCategories();

        /// <summary>
        /// 取得所有商家清單 (Id, StoreName)
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

        /// <summary>[Async] 分頁取得商品列表，直接投影至 ProductListDto，SQL 端分頁</summary>
        Task<(IEnumerable<ProductListDto> Items, int TotalCount)> GetProductsPagedAsync(ProductSearchCriteria criteria);

        /// <summary>[Async] 取得待審核商品列表，直接投影至 ProductReviewDto</summary>
        Task<IEnumerable<ProductReviewDto>> GetPendingProductsAsync();

        /// <summary>[Async] 分頁取得待審核商品（ReviewStatus == 0），直接投影至 ProductReviewDto</summary>
        Task<(IEnumerable<ProductReviewDto> Items, int TotalCount)> GetPendingProductsPagedAsync(int page, int pageSize);

        /// <summary>[Async] 分頁取得已退回商品（ReviewStatus == 2），直接投影至 ProductReviewDto</summary>
        Task<(IEnumerable<ProductReviewDto> Items, int TotalCount)> GetRejectedProductsPagedAsync(int page, int pageSize);

        /// <summary>[Async] 取得最近退回商品清單，直接投影至 ProductReviewDto</summary>
        Task<IEnumerable<ProductReviewDto>> GetRecentRejectedProductsAsync(int top);

        /// <summary>[Async] 核准商品審核（Status → 1 上架，記錄審核人）</summary>
        Task ApproveProductAsync(int id, string adminId);

        /// <summary>[Async] 退回商品審核（Status → 3，記錄退回原因、審核人與時間）</summary>
        Task RejectProductAsync(int id, string adminId, string reason);

        /// <summary>[Async] 提交商品審核：自動違禁詞攔截或轉為待審核</summary>
        Task SubmitProductForReviewAsync(int productId);

        /// <summary>[Async] 軟刪除所有過期退回商品，回傳清理筆數</summary>
        Task<int> CleanupExpiredRejectedAsync(int expirationSeconds);

        /// <summary>[Async] 重設為待審核：清空審核結果欄位，商品回到 Status=2 / ReviewStatus=0</summary>
        Task ResetToPendingAsync(int productId);

        /// <summary>[Async] 取得全站商品各狀態筆數（Unpublished 不含強制下架）</summary>
        Task<(int Total, int Published, int Unpublished, int Pending, int ForcedOffShelf)> GetStatusCountsAsync();

        /// <summary>[Async] 管理員強制下架商品（Status→4），儲存下架原因與操作人</summary>
        Task ForceUnpublishAsync(int id, string? reason, int? adminBy);

        /// <summary>[Async] 批次強制下架</summary>
        Task<int> BatchForceOffShelfAsync(List<int> ids, string? reason, int? adminBy);

        /// <summary>[Async] 分頁取得重新申請審核商品（ReviewStatus==3）</summary>
        Task<(IEnumerable<ProductReviewDto> Items, int TotalCount)> GetReApplyProductsPagedAsync(int page, int pageSize);

        /// <summary>[Async] 賣家申請重新上架：設 ReviewStatus=3，記錄申請時間</summary>
        Task ReApplyAsync(int id);

        /// <summary>[Async] 管理員核准強制下架商品重新上架（Status→1, ReviewStatus→1）</summary>
        Task ApproveForcedProductAsync(int id, string adminId);

        /// <summary>[Async] 管理員駁回重新申請（保持 Status=4, ReviewStatus→2，記錄原因）</summary>
        Task RejectForcedProductAsync(int id, string adminId, string reason);

        /// <summary>[Async] 取得第一筆未刪除商品作為測試商品的範本（複製 StoreId/CategoryId/BrandId）</summary>
        Task<Product?> GetFirstActiveProductAsync();

        /// <summary>[Async] 批次新增商品</summary>
        Task AddProductsRangeAsync(IEnumerable<Product> products);

        /// <summary>[Async] 取得近期審核通過的商品（ReviewStatus=1 且 ReviewDate 在 hours 小時內）</summary>
        Task<IEnumerable<ProductReviewDto>> GetRecentlyApprovedProductsAsync(int hours = 24);

        /// <summary>[Async] 分頁取得近期審核通過的商品（ReviewStatus=1 且 ReviewDate 在 hours 小時內）</summary>
        Task<(IEnumerable<ProductReviewDto> Items, int TotalCount)> GetRecentlyApprovedProductsPagedAsync(int page, int pageSize, int hours = 24);

        /// <summary>[Async] 隨機取得包含圖片的真實商品（用於生成測試商品時複製圖片）</summary>
        Task<List<Product>> GetRandomProductsWithImagesAsync(int count);

        /// <summary>[Async] 依 ID 清單取得商品審核摘要（用於生成測試商品後回傳 DOM 插入所需資料）</summary>
        Task<IEnumerable<ProductReviewDto>> GetProductsByIdsForReviewAsync(IEnumerable<int> ids);

        /// <summary>[Async] 模擬賣家修改後重新送審：將 ReviewStatus=2（已退回）的商品改為 ReviewStatus=3（待重新審核）。</summary>
        Task SimulateSellerResubmitAsync(int id);

        // ═══════════════════════════════════════════════════════════
        //  前台商品列表（AllowAnonymous，只查 Status==1 上架中）
        // ═══════════════════════════════════════════════════════════

        /// <summary>[Async] 前台商品總覽：只查上架中商品，支援分類/關鍵字/品牌/價格區間/排序/分頁。
        /// subCategoryId 優先於 categoryId（直接過濾，不展開）；brandIds 為空陣列時不篩選；price 以 MinPrice 比較。</summary>
        Task<(IEnumerable<ProductListDto> Items, int TotalCount)> GetFrontActiveProductsAsync(
            int? categoryId, string? keyword, string sortBy, int page, int pageSize,
            int? subCategoryId = null, int[]? brandIds = null,
            decimal? minPrice = null, decimal? maxPrice = null);

        // ═══════════════════════════════════════════════════════════
        //  前台商品詳情頁
        // ═══════════════════════════════════════════════════════════

        /// <summary>[Async] 前台商品詳情：一次撈商品 + Brand + Store + Category(含父層) + ProductImages + ProductVariants(含圖)。</summary>
        Task<Product?> GetProductDetailAsync(int id);

        /// <summary>[Async] 計算商品評分與評論數（透過 OrderReview → Order → OrderDetail）。</summary>
        Task<(decimal? Rating, int ReviewCount)> GetProductRatingAsync(int productId);

        /// <summary>[Async] 計算賣場總平均評分。</summary>
        Task<decimal?> GetStoreRatingAsync(int storeId);

        /// <summary>[Async] 計算指定商店上架中商品數。</summary>
        Task<int> GetStoreActiveProductCountAsync(int storeId);

        /// <summary>[Async] 取得同子分類相關商品（排除自身、只取上架中、依銷量排序）。</summary>
        Task<IEnumerable<ProductListDto>> GetRelatedProductsAsync(int productId, int categoryId, int limit);

        /// <summary>[Async] 取得熱搜關鍵字（瀏覽次數最高的前 N 筆上架商品名稱）。</summary>
        Task<List<string>> GetHotKeywordsAsync(int limit);

        /// <summary>[Async] 將商品 ViewCount +1（使用 ExecuteUpdateAsync 直接更新，不載入實體）。</summary>
        Task IncrementViewCountAsync(int productId);

        /// <summary>批次新增商品圖片（前台賣家上傳）。</summary>
        void AddProductImages(int productId, IEnumerable<ProductImage> images);

        /// <summary>刪除商品所有圖片（資料庫記錄 + 實體檔案）。</summary>
        void DeleteProductImages(int productId, string webRootPath);

        /// <summary>刪除商品圖片（排除指定的 URL 列表）。</summary>
        void DeleteProductImagesExcept(int productId, List<string> keepImageUrls, string webRootPath);

        /// <summary>更新商品主圖設定。</summary>
        void UpdateMainImage(int productId, int mainImageIndex);

        /// <summary>
        /// [Async] 更新商品規格庫存與商品總銷量
        /// </summary>
        /// <param name="productId">商品 ID</param>
        /// <param name="variantId">規格 ID (為 null 則不更新規格庫存，僅更新商品總銷量)</param>
        /// <param name="quantityChange">變動數量 (負數為扣除，正數為歸還)</param>
        /// <returns>更新是否成功</returns>
        Task<bool> UpdateStockAsync(int productId, int? variantId, int quantityChange);
    }
}
