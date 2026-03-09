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

        /// <summary>[Async] 取得全站商品各狀態筆數</summary>
        Task<(int Total, int Published, int Unpublished, int Pending)> GetStatusCountsAsync();

        /// <summary>[Async] 管理員強制下架商品，儲存下架原因</summary>
        Task ForceUnpublishAsync(int id, string? reason);

        /// <summary>[Async] 模擬系統自動審核：新增測試商品，對所有待審核商品執行違禁詞比對，回傳攔截與放行筆數</summary>
        Task<SimulateAutoReviewResult> SimulateAutoReviewAsync();
    }
}
