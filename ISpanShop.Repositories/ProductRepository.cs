using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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

        /// <summary>
        /// 取得待審核商品列表 (Status == 2)
        /// </summary>
        /// <returns>待審核商品集合</returns>
        public IEnumerable<Product> GetPendingProducts()
        {
            return _context.Products.Where(p => p.Status == 2).ToList();
        }

        /// <summary>
        /// 更新商品狀態
        /// </summary>
        /// <param name="id">商品 ID</param>
        /// <param name="status">新的狀態值</param>
        public void UpdateProductStatus(int id, byte status)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                product.Status = status;
                product.UpdatedAt = DateTime.Now;
                _context.SaveChanges();
            }
        }

        /// <summary>
        /// 取得所有商品列表 - 包含關聯資料
        /// </summary>
        /// <returns>所有商品集合</returns>
        public IEnumerable<Product> GetAllProducts()
        {
            return _context.Products
                .Include(p => p.Store)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .ToList();
        }

        /// <summary>
        /// 根據 ID 取得商品詳情（包含圖片與規格）
        /// </summary>
        /// <param name="id">商品 ID</param>
        /// <returns>商品實體，若不存在則返回 null</returns>
        public Product? GetProductById(int id)
        {
            return _context.Products
                .Include(p => p.Store)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .FirstOrDefault(p => p.Id == id);
        }
    }
}
