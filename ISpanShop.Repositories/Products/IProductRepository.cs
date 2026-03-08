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
        /// 核准商品審核（Status → 1 上架，記錄審核人）
        /// </summary>
        void ApproveProduct(int id, string adminId);

        /// <summary>
        /// 退回商品審核（Status → 3 審核退回，記錄退回原因、審核人與時間）
        /// </summary>
        void RejectProduct(int id, string adminId, string reason);

        /// <summary>
        /// 提交商品審核：自動違禁詞攔截（ReviewStatus=2）或轉為待審核（ReviewStatus=0）
        /// </summary>
        void SubmitProductForReview(int productId);

        /// <summary>
        /// 軟刪除所有過期退回商品（ReviewStatus==2 且距 RejectDate 已達 expirationDays 天）
        /// 回傳清理筆數。
        /// </summary>
        int CleanupExpiredRejected(int expirationSeconds);

        /// <summary>
        /// 取得最近退回的商品清單（Status == 3），依 UpdatedAt 降冪排序
        /// </summary>
        /// <param name="top">最多取幾筆</param>
        IEnumerable<Product> GetRecentRejectedProducts(int top);

        /// <summary>
        /// 分頁取得待審核商品（ReviewStatus == 0）
        /// </summary>
        (IEnumerable<Product> Items, int TotalCount) GetPendingProductsPaged(int page, int pageSize);

        /// <summary>
        /// 分頁取得已退回商品（ReviewStatus == 2）
        /// </summary>
        (IEnumerable<Product> Items, int TotalCount) GetRejectedProductsPaged(int page, int pageSize);

        /// <summary>
        /// 重設為待審核：清空審核結果欄位，商品回到 Status=2 / ReviewStatus=0
        /// </summary>
        void ResetToPending(int productId);

        /// <summary>
        /// 更新商品基本資料（名稱、描述、分類、品牌、規格定義、主圖）
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
        /// 取得全站商品各狀態筆數（Total、已上架、未上架、待審核）
        /// </summary>
        (int Total, int Published, int Unpublished, int Pending) GetStatusCounts();

        /// <summary>
        /// 管理員強制下架商品，儲存下架原因
        /// </summary>
        void ForceUnpublish(int id, string? reason);

        // ═══════════════════════════════════════════════════════════
        //  非同步版本（效能最佳化：async/await + 投影 + 真分頁）
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// [Async] 分頁取得商品列表，直接投影至 <see cref="ProductListDto"/>，SQL 端分頁
        /// </summary>
        Task<(IEnumerable<ProductListDto> Items, int TotalCount)> GetProductsPagedAsync(ProductSearchCriteria criteria);

        /// <summary>
        /// [Async] 取得待審核商品列表，直接投影至 <see cref="ProductReviewDto"/>
        /// </summary>
        Task<IEnumerable<ProductReviewDto>> GetPendingProductsAsync();

        /// <summary>
        /// [Async] 分頁取得待審核商品（ReviewStatus == 0），直接投影至 <see cref="ProductReviewDto"/>
        /// </summary>
        Task<(IEnumerable<ProductReviewDto> Items, int TotalCount)> GetPendingProductsPagedAsync(int page, int pageSize);

        /// <summary>
        /// [Async] 分頁取得已退回商品（ReviewStatus == 2），直接投影至 <see cref="ProductReviewDto"/>
        /// </summary>
        Task<(IEnumerable<ProductReviewDto> Items, int TotalCount)> GetRejectedProductsPagedAsync(int page, int pageSize);

        /// <summary>
        /// [Async] 取得最近退回商品清單，直接投影至 <see cref="ProductReviewDto"/>
        /// </summary>
        Task<IEnumerable<ProductReviewDto>> GetRecentRejectedProductsAsync(int top);

        /// <summary>
        /// [Async] 核准商品審核（Status → 1 上架，記錄審核人）
        /// </summary>
        Task ApproveProductAsync(int id, string adminId);

        /// <summary>
        /// [Async] 退回商品審核（Status → 3，記錄退回原因、審核人與時間）
        /// </summary>
        Task RejectProductAsync(int id, string adminId, string reason);

        /// <summary>
        /// [Async] 提交商品審核：自動違禁詞攔截或轉為待審核
        /// </summary>
        Task SubmitProductForReviewAsync(int productId);

        /// <summary>
        /// [Async] 軟刪除所有過期退回商品，回傳清理筆數
        /// </summary>
        Task<int> CleanupExpiredRejectedAsync(int expirationSeconds);

        /// <summary>
        /// [Async] 重設為待審核：清空審核結果欄位，商品回到 Status=2 / ReviewStatus=0
        /// </summary>
        Task ResetToPendingAsync(int productId);

        /// <summary>
        /// [Async] 取得全站商品各狀態筆數
        /// </summary>
        Task<(int Total, int Published, int Unpublished, int Pending)> GetStatusCountsAsync();

        /// <summary>
        /// [Async] 管理員強制下架商品，儲存下架原因
        /// </summary>
        Task ForceUnpublishAsync(int id, string? reason);
    }
}
