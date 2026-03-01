using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        /// 自動化商品審核機制
        /// 規則：含違禁詞 → 3(退回)；描述過短 → 2(待手動審核)；正常 → 1(自動通過上架)
        /// </summary>
        private byte AutoReviewProduct(string name, string description)
        {
            var badWords = new[]
            {
                "高仿", "原單", "槍械", "毒品", "贗品", "假貨", "冒牌", "盜版",
                "走私", "非法", "詐騙", "傳銷", "洗錢", "賭博", "色情",
                "暴力", "恐怖", "炸藥", "大麻", "槍", "彈藥", "仿冒"
            };

            string combined = (name + " " + (description ?? "")).ToLower();

            if (badWords.Any(w => combined.Contains(w.ToLower())))
                return 3; // 自動退回

            if (string.IsNullOrWhiteSpace(description) || description.Trim().Length < 10)
                return 2; // 描述過短，待手動審核

            return 1; // 自動通過，直接上架
        }

        /// <summary>
        /// 取得待審核商品列表 - 轉換為 DTO
        /// </summary>
        /// <returns>待審核商品 DTO 集合</returns>
        public IEnumerable<ProductReviewDto> GetPendingProducts()
        {
            var pendingProducts = _productRepository.GetPendingProducts();

            return pendingProducts.Select(p => new ProductReviewDto
            {
                Id          = p.Id,
                StoreId     = p.StoreId,
                CategoryName = p.Category?.Name ?? "未分類",
                BrandName   = p.Brand?.Name ?? "未設定",
                StoreName   = p.Store?.StoreName ?? "未知商店",
                Name        = p.Name,
                Description = p.Description,
                Status      = p.Status ?? 0,
                CreatedAt   = p.CreatedAt,
                UpdatedAt   = p.UpdatedAt,
                MainImageUrl = p.ProductImages
                    ?.FirstOrDefault(img => img.IsMain == true)?.ImageUrl
            }).ToList();
        }

        /// <summary>
        /// 變更商品狀態
        /// </summary>
        /// <param name="id">商品 ID</param>
        /// <param name="newStatus">新的狀態值</param>
        public void ChangeProductStatus(int id, byte newStatus)
        {
            _productRepository.UpdateProductStatus(id, newStatus);
        }

        /// <summary>
        /// 取得所有商品列表 - 轉換為 DTO
        /// </summary>
        /// <returns>商品列表 DTO 集合</returns>
        public IEnumerable<ProductListDto> GetAllProducts()
        {
            var allProducts = _productRepository.GetAllProducts();

            return allProducts.Select(p => new ProductListDto
            {
                Id = p.Id,
                StoreName = p.Store?.StoreName ?? "未知商店",
                CategoryName = p.Category?.Name ?? "未分類",
                BrandName = p.Brand?.Name ?? "未設定",
                Name = p.Name,
                MinPrice = p.MinPrice,
                MaxPrice = p.MaxPrice,
                Status = p.Status,
                MainImageUrl = p.ProductImages
                    ?.FirstOrDefault(img => img.IsMain==true)?.ImageUrl 
                    ?? p.ProductImages?.FirstOrDefault()?.ImageUrl 
                    ?? "https://via.placeholder.com/400x400?text=No+Image"
            }).ToList();
        }

        /// <summary>
        /// 分頁取得商品列表，支援分類篩選
        /// </summary>
        /// <param name="criteria">搜尋條件</param>
        /// <returns>分頁商品列表 DTO</returns>
        public PagedResult<ProductListDto> GetProductsPaged(ProductSearchCriteria criteria)
        {
            var (items, totalCount) = _productRepository.GetProductsPaged(criteria);

            var dtos = items.Select(p => new ProductListDto
            {
                Id = p.Id,
                StoreName = p.Store?.StoreName ?? "未知商店",
                CategoryName = p.Category?.Name ?? "未分類",
                BrandName = p.Brand?.Name ?? "未設定",
                Name = p.Name,
                MinPrice = p.MinPrice,
                MaxPrice = p.MaxPrice,
                Status = p.Status,
                MainImageUrl = p.ProductImages
                    ?.FirstOrDefault(img => img.IsMain == true)?.ImageUrl
                    ?? p.ProductImages?.FirstOrDefault()?.ImageUrl
                    ?? "https://via.placeholder.com/400x400?text=No+Image",
                CreatedAt = p.CreatedAt
            }).ToList();

            return PagedResult<ProductListDto>.Create(dtos, totalCount, criteria.PageNumber, criteria.PageSize);
        }

        /// <summary>
        /// 取得所有分類清單（含主分類與子分類）
        /// </summary>
        /// <returns>分類 DTO 集合</returns>
        public IEnumerable<CategoryDto> GetAllCategories()
        {
            return _productRepository.GetAllCategories()
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParentId = c.ParentId
                })
                .ToList();
        }

        /// <summary>
        /// 根據 ID 取得商品詳情 - 轉換為 DTO（包含完整圖片與規格列表）
        /// </summary>
        /// <param name="id">商品 ID</param>
        /// <returns>商品詳情 DTO，若不存在則返回 null</returns>
        public ProductDetailDto? GetProductDetail(int id)
        {
            var product = _productRepository.GetProductById(id);

            if (product == null)
            {
                return null;
            }

            return new ProductDetailDto
            {
                Id = product.Id,
                Name = product.Name,
                StoreName = product.Store?.StoreName ?? "未知商店",
                CategoryName = product.Category?.Name ?? "未分類",
                BrandName = product.Brand?.Name ?? "未設定",
                Description = product.Description,
                Status = product.Status,
                Images = product.ProductImages?
                    .OrderBy(img => img.SortOrder)
                    .Select(img => img.ImageUrl)
                    .ToList() ?? new List<string>(),
                Variants = product.ProductVariants?
                    .Where(v => !v.IsDeleted==false)
                    .Select(v => new ProductVariantDetailDto
                    {
                        SkuCode = v.SkuCode,
                        VariantName = v.VariantName,
                        Price = v.Price,
                        Stock = v.Stock ?? 0,
                        SpecValueJson = v.SpecValueJson
                    })
                    .ToList() ?? new List<ProductVariantDetailDto>()
            };
        }

        /// <summary>
        /// 取得所有商家清單
        /// </summary>
        /// <returns>商家清單 (Id, Name)</returns>
        public IEnumerable<(int Id, string Name)> GetStoreOptions()
        {
            return _productRepository.GetStoreOptions();
        }

        /// <summary>
        /// 取得所有品牌清單
        /// </summary>
        /// <returns>品牌清單 (Id, Name)</returns>
        public IEnumerable<(int Id, string Name)> GetBrandOptions()
        {
            return _productRepository.GetBrandOptions();
        }

        /// <summary>
        /// 根據子分類取得該分類下商品涵蓋的品牌清單
        /// </summary>
        /// <param name="categoryId">子分類 ID；為 null 時回傳全部品牌</param>
        /// <returns>品牌清單 (Id, Name)</returns>
        public IEnumerable<(int Id, string Name)> GetBrandsByCategory(int? categoryId)
        {
            return _productRepository.GetBrandsByCategory(categoryId);
        }

        /// <summary>
        /// 批次更新商品上下架狀態
        /// </summary>
        public async Task<int> UpdateBatchStatusAsync(List<int> productIds, byte targetStatus)
        {
            if (productIds == null || productIds.Count == 0) return 0;
            return await _productRepository.UpdateBatchStatusAsync(productIds, targetStatus);
        }

        /// <summary>
        /// 核准商品審核 - 將狀態設為 1 (上架)
        /// </summary>
        public void ApproveProduct(int id)
        {
            _productRepository.ApproveProduct(id);
        }

        /// <summary>
        /// 退回商品審核 - 將狀態設為 3 (審核退回)
        /// </summary>
        public void RejectProduct(int id, string? reason)
        {
            _productRepository.RejectProduct(id, reason);
        }

        /// <summary>
        /// 取得最近退回的商品清單（Status == 3）
        /// </summary>
        public IEnumerable<ProductReviewDto> GetRecentRejectedProducts(int top = 10)
        {
            return _productRepository.GetRecentRejectedProducts(top)
                .Select(p => new ProductReviewDto
                {
                    Id          = p.Id,
                    StoreId     = p.StoreId,
                    CategoryName = p.Category?.Name ?? "未分類",
                    BrandName   = p.Brand?.Name ?? "未設定",
                    StoreName   = p.Store?.StoreName ?? "未知商店",
                    Name        = p.Name,
                    Description = p.Description,
                    Status      = p.Status ?? 3,
                    RejectReason = p.RejectReason,
                    CreatedAt   = p.CreatedAt,
                    UpdatedAt   = p.UpdatedAt,
                    MainImageUrl = p.ProductImages
                        ?.FirstOrDefault(img => img.IsMain == true)?.ImageUrl
                }).ToList();
        }
    }
}
