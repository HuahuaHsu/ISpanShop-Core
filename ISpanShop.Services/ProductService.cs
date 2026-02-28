using System;
using System.Collections.Generic;
using System.Linq;
using ISpanShop.Models.DTOs;
using ISpanShop.Models.EfModels;
using ISpanShop.Repositories;
using ISpanShop.Repositories.Interfaces;
using ISpanShop.Services.Interfaces;

namespace ISpanShop.Services
{
    /// <summary>
    /// 商品 Service 實作 - 處理商品相關的商業邏輯
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        /// <summary>
        /// 建構子 - 注入 ProductRepository
        /// </summary>
        /// <param name="productRepository">商品 Repository</param>
        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        /// <summary>
        /// 建立新商品 - 包含商業邏輯轉換與驗證
        /// </summary>
        /// <param name="dto">商品建立 DTO</param>
        public void CreateProduct(ProductCreateDto dto)
        {
            // DTO 轉換為 Product Entity
            var product = new Product
            {
                StoreId = dto.StoreId,
                CategoryId = dto.CategoryId,
                BrandId = dto.BrandId,
                Name = dto.Name,
                Description = dto.Description,
                VideoUrl = dto.VideoUrl,
                SpecDefinitionJson = dto.SpecDefinitionJson,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Status = AutoReviewProduct(dto.Name, dto.Description)
            };

            // 商業邏輯一：計算價格區間 (最低價與最高價)
            if (dto.Variants != null && dto.Variants.Count > 0)
            {
                product.MinPrice = dto.Variants.Min(v => v.Price);
                product.MaxPrice = dto.Variants.Max(v => v.Price);

                // 商業邏輯二：SKU 生成與 Variant 轉換
                product.ProductVariants = dto.Variants.Select(variantDto =>
                {
                    // 如果 SkuCode 為 null 或空字串，自動產生唯一代碼
                    string skuCode = string.IsNullOrWhiteSpace(variantDto.SkuCode)
                        ? GenerateUniqueSku()
                        : variantDto.SkuCode;

                    return new ProductVariant
                    {
                        SkuCode = skuCode,
                        VariantName = variantDto.VariantName,
                        SpecValueJson = variantDto.SpecValueJson,
                        Price = variantDto.Price,
                        Stock = variantDto.Stock,
                        SafetyStock = variantDto.SafetyStock,
                    };
                }).ToList();
            }

            // TODO: 處理實體檔案上傳與 ProductImages 資料表寫入，我們稍後再獨立處理。

            // 將轉換好的 Product Entity 交給 Repository 儲存
            _productRepository.AddProduct(product);
        }

        /// <summary>
        /// 生成唯一的 SKU 代碼
        /// </summary>
        /// <returns>唯一的 SKU 代碼</returns>
        private string GenerateUniqueSku()
        {
            // 使用 Guid 的前 8 個字元 + 時間戳記
            string guidPart = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            string timePart = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            return $"{guidPart}-{timePart}";
        }

        /// <summary>
        /// 自動化商品審核機制 - 檢查是否包含違禁詞
        /// </summary>
        /// <param name="name">商品名稱</param>
        /// <param name="description">商品描述</param>
        /// <returns>審核狀態：1 = 自動通過允許上架，2 = 需要人工審核</returns>
        private byte AutoReviewProduct(string name, string description)
        {
            // 定義電商違禁詞
            var bannedKeywords = new[]
            {
                "高仿", "原單", "槍械", "毒品", "贗品", "假貨", "冒牌", "盜版",
                "走私", "非法", "詐騙", "傳銷", "洗錢", "賭博", "色情",
                "暴力", "恐怖", "炸藥", "刀具", "槍", "彈藥"
            };

            // 檢查商品名稱與描述是否包含違禁詞
            string combined = (name + " " + description).ToLower();

            foreach (var keyword in bannedKeywords)
            {
                if (combined.Contains(keyword.ToLower()))
                {
                    // 包含違禁詞，回傳 2 表示需要人工審核
                    return 2;
                }
            }

            // 不包含違禁詞，回傳 1 表示自動審核通過，允許上架
            return 1;
        }
    }
}
