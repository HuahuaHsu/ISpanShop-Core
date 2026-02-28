using ISpanShop.Models.EfModels;
using ISpanShop.Repositories.Interfaces;

namespace ISpanShop.Repositories
{
    /// <summary>
    /// 商品 Repository 實作 - 處理商品的 CRUD 操作
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly ISpanShopDBContext _context;

        /// <summary>
        /// 建構子 - 注入 DbContext
        /// </summary>
        /// <param name="context">ISpanShop 資料庫上下文</param>
        public ProductRepository(ISpanShopDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 新增商品 - 依賴 EF Core 的關聯追蹤來一併儲存 Variant 和 Image
        /// </summary>
        /// <param name="product">商品實體</param>
        public void AddProduct(Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
        }

        /// <summary>
        /// 檢查 SKU 代碼是否已存在
        /// </summary>
        /// <param name="skuCode">SKU 代碼</param>
        /// <returns>true 表示已存在，false 表示不存在</returns>
        public bool IsSkuExists(string skuCode)
        {
            return _context.ProductVariants.Any(pv => pv.SkuCode == skuCode);
        }
    }
}
