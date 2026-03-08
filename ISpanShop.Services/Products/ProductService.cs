using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISpanShop.Models.DTOs.Products;
using ISpanShop.Models.DTOs.Categories;
using ISpanShop.Models.DTOs.Common;
using ISpanShop.Models.EfModels;
using ISpanShop.Repositories.Products;
using ISpanShop.Repositories.Categories;
using ISpanShop.Repositories.Inventories;
using ISpanShop.Services.Products;
using ISpanShop.Services.Inventories;

namespace ISpanShop.Services.Products
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
                ReviewStatus = p.ReviewStatus,
                ReviewedBy  = p.ReviewedBy,
                ReviewDate  = p.ReviewDate,
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
                ReviewStatus = p.ReviewStatus,
                ReviewedBy   = p.ReviewedBy,
                ReviewDate   = p.ReviewDate,
                RejectReason = p.RejectReason,
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
                CreatedAt = p.CreatedAt,
                ReviewStatus = p.ReviewStatus,
                ReviewedBy   = p.ReviewedBy,
                ReviewDate   = p.ReviewDate,
                RejectReason = p.RejectReason
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
                Id                 = product.Id,
                Name               = product.Name,
                StoreId            = product.StoreId,
                StoreName          = product.Store?.StoreName ?? "未知商店",
                CategoryId         = product.CategoryId,
                CategoryName       = product.Category?.Name ?? "未分類",
                BrandId            = product.BrandId,
                BrandName          = product.Brand?.Name ?? "未設定",
                Description        = product.Description,
                Status             = product.Status,
                MinPrice           = product.MinPrice,
                MaxPrice           = product.MaxPrice,
                TotalSales         = product.TotalSales,
                ViewCount          = product.ViewCount,
                RejectReason       = product.RejectReason,
                SpecDefinitionJson = product.SpecDefinitionJson,
                CreatedAt          = product.CreatedAt,
                UpdatedAt          = product.UpdatedAt,
                ReviewStatus       = product.ReviewStatus,
                ReviewedBy         = product.ReviewedBy,
                ReviewDate         = product.ReviewDate,
                Images = product.ProductImages?
                    .OrderBy(img => img.SortOrder)
                    .Select(img => img.ImageUrl)
                    .ToList() ?? new List<string>(),
                Variants = product.ProductVariants?
                    .Where(v => v.IsDeleted != true)
                    .Select(v => new ProductVariantDetailDto
                    {
                        Id            = v.Id,
                        ProductId     = v.ProductId,
                        SkuCode       = v.SkuCode,
                        VariantName   = v.VariantName,
                        Price         = v.Price,
                        Stock         = v.Stock,
                        SafetyStock   = v.SafetyStock,
                        SpecValueJson = v.SpecValueJson,
                        IsDeleted     = v.IsDeleted
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
        /// 核准商品審核 - Status=1(上架)，記錄審核人
        /// </summary>
        public void ApproveProduct(int id, string adminId)
        {
            _productRepository.ApproveProduct(id, adminId);
        }

        /// <summary>
        /// 退回商品審核 - Status=3(退回)，記錄原因、審核人與退回時間
        /// </summary>
        public void RejectProduct(int id, string adminId, string reason)
        {
            _productRepository.RejectProduct(id, adminId, reason);
        }

        /// <summary>
        /// 提交商品審核：自動違禁詞攔截（立即退回）或轉為待審核佇列
        /// </summary>
        public void SubmitProductForReview(int productId)
        {
            _productRepository.SubmitProductForReview(productId);
        }

        /// <summary>
        /// [Demo 專用] 清理過期退回商品（IsDeleted=true），回傳清理筆數
        /// </summary>
        public int CleanupExpiredRejectedProducts(int expirationSeconds = 60)
        {
            return _productRepository.CleanupExpiredRejected(expirationSeconds);
        }


        /// <summary>
        /// 管理員後台新增商品 - 略過審核直接上架，自動建立預設規格
        /// </summary>
        public void CreateProductAdmin(ProductAdminCreateDto dto)
        {
            var skuCode = $"{Guid.NewGuid().ToString()[..8].ToUpper()}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

            var product = new Product
            {
                StoreId            = dto.StoreId,
                CategoryId         = dto.CategoryId,
                BrandId            = dto.BrandId,
                Name               = dto.Name,
                Description        = dto.Description,
                SpecDefinitionJson = "[]",
                MinPrice           = dto.Price,
                MaxPrice           = dto.Price,
                Status             = 1,        // 管理員建立直接上架
                IsDeleted          = false,
                CreatedAt          = DateTime.Now,
                UpdatedAt          = DateTime.Now,
                ProductVariants    = new List<ProductVariant>
                {
                    new ProductVariant
                    {
                        SkuCode       = skuCode,
                        VariantName   = "標準版",
                        SpecValueJson = "{}",
                        Price         = dto.Price,
                        Stock         = 0,
                        SafetyStock   = 5,
                        IsDeleted     = false
                    }
                }
            };

            if (!string.IsNullOrWhiteSpace(dto.MainImageUrl))
            {
                product.ProductImages = new List<ProductImage>
                {
                    new ProductImage { ImageUrl = dto.MainImageUrl, IsMain = true, SortOrder = 0 }
                };
            }

            _productRepository.AddProduct(product);
        }

        /// <summary>
        /// 更新商品基本資料
        /// </summary>
        public void UpdateProduct(ProductUpdateDto dto)
            => _productRepository.UpdateProduct(dto);

        /// <summary>
        /// 軟刪除商品（IsDeleted = true）
        /// </summary>
        public void SoftDeleteProduct(int id)
            => _productRepository.SoftDeleteProduct(id);

        /// <summary>
        /// 根據 ID 取得規格詳情
        /// </summary>
        public ProductVariantDetailDto? GetVariantById(int id)
        {
            var v = _productRepository.GetVariantById(id);
            if (v == null) return null;
            return new ProductVariantDetailDto
            {
                Id            = v.Id,
                ProductId     = v.ProductId,
                SkuCode       = v.SkuCode,
                VariantName   = v.VariantName,
                Price         = v.Price,
                Stock         = v.Stock,
                SafetyStock   = v.SafetyStock,
                SpecValueJson = v.SpecValueJson,
                IsDeleted     = v.IsDeleted
            };
        }

        /// <summary>
        /// 為商品新增規格（自動產生 SKU 若未提供）
        /// </summary>
        public string? AddVariant(int productId, ProductVariantCreateDto dto)
        {
            string skuCode;
            if (string.IsNullOrWhiteSpace(dto.SkuCode))
            {
                // 自動產生唯一 SKU（最多嘗試 5 次）
                int attempts = 0;
                do
                {
                    skuCode = Guid.NewGuid().ToString("N")[..8].ToUpper();
                    attempts++;
                } while (_productRepository.IsSkuExists(skuCode) && attempts < 5);
            }
            else
            {
                skuCode = dto.SkuCode.Trim();
                if (_productRepository.IsSkuExists(skuCode))
                    return "此 SKU 代碼已被使用，請更換。";
            }

            var variant = new ISpanShop.Models.EfModels.ProductVariant
            {
                ProductId     = productId,
                SkuCode       = skuCode,
                VariantName   = dto.VariantName,
                SpecValueJson = dto.SpecValueJson,
                Price         = dto.Price,
                Stock         = dto.Stock,
                SafetyStock   = dto.SafetyStock,
                IsDeleted     = false
            };

            try
            {
                _productRepository.AddVariant(variant);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
                when (ex.InnerException?.Message.Contains("UNIQUE") == true ||
                      ex.InnerException?.Message.Contains("unique") == true ||
                      ex.InnerException?.Message.Contains("duplicate") == true)
            {
                return "此 SKU 代碼已被使用，請更換。";
            }

            return null;
        }

        /// <summary>
        /// 更新規格
        /// </summary>
        public void UpdateVariant(ProductVariantUpdateDto dto)
            => _productRepository.UpdateVariant(dto);

        /// <summary>
        /// 軟刪除規格
        /// </summary>
        public void SoftDeleteVariant(int id)
            => _productRepository.SoftDeleteVariant(id);

        public (int Total, int Published, int Unpublished, int Pending) GetProductStatusCounts()
            => _productRepository.GetStatusCounts();

        public void ForceUnpublish(int id, string? reason)
            => _productRepository.ForceUnpublish(id, reason);

        /// <summary>
        /// 取得最近退回的商品清單（Status == 3）
        /// </summary>
        public IEnumerable<ProductReviewDto> GetRecentRejectedProducts(int top = 10)
        {
            return _productRepository.GetRecentRejectedProducts(top)
                .Select(p => ToReviewDto(p)).ToList();
        }

        /// <summary>分頁取得待審核商品（ReviewStatus == 0）</summary>
        public PagedResult<ProductReviewDto> GetPendingProductsPaged(int page, int pageSize)
        {
            var (items, total) = _productRepository.GetPendingProductsPaged(page, pageSize);
            return PagedResult<ProductReviewDto>.Create(
                items.Select(p => ToReviewDto(p)).ToList(), total, page, pageSize);
        }

        /// <summary>分頁取得已退回商品（ReviewStatus == 2）</summary>
        public PagedResult<ProductReviewDto> GetRejectedProductsPaged(int page, int pageSize)
        {
            var (items, total) = _productRepository.GetRejectedProductsPaged(page, pageSize);
            return PagedResult<ProductReviewDto>.Create(
                items.Select(p => ToReviewDto(p)).ToList(), total, page, pageSize);
        }

        /// <summary>重設為待審核：清空審核結果欄位</summary>
        public void ResetToPending(int productId)
            => _productRepository.ResetToPending(productId);

        private static ProductReviewDto ToReviewDto(ISpanShop.Models.EfModels.Product p) =>
            new ProductReviewDto
            {
                Id           = p.Id,
                StoreId      = p.StoreId,
                CategoryName = p.Category?.Name ?? "未分類",
                BrandName    = p.Brand?.Name ?? "未設定",
                StoreName    = p.Store?.StoreName ?? "未知商店",
                Name         = p.Name,
                Description  = p.Description,
                Status       = p.Status ?? 0,
                ReviewStatus = p.ReviewStatus,
                ReviewedBy   = p.ReviewedBy,
                ReviewDate   = p.ReviewDate,
                RejectReason = p.RejectReason,
                CreatedAt    = p.CreatedAt,
                UpdatedAt    = p.UpdatedAt,
                MainImageUrl = p.ProductImages?.FirstOrDefault(img => img.IsMain == true)?.ImageUrl
            };

        // ═══════════════════════════════════════════════════════════
        //  非同步實作（async/await + 投影 + 真分頁）
        // ═══════════════════════════════════════════════════════════

        /// <inheritdoc/>
        public async Task<IEnumerable<ProductReviewDto>> GetPendingProductsAsync()
            => await _productRepository.GetPendingProductsAsync();

        /// <inheritdoc/>
        public async Task<PagedResult<ProductListDto>> GetProductsPagedAsync(ProductSearchCriteria criteria)
        {
            var (items, totalCount) = await _productRepository.GetProductsPagedAsync(criteria);
            return PagedResult<ProductListDto>.Create(items.ToList(), totalCount, criteria.PageNumber, criteria.PageSize);
        }

        /// <inheritdoc/>
        public async Task ApproveProductAsync(int id, string adminId)
            => await _productRepository.ApproveProductAsync(id, adminId);

        /// <inheritdoc/>
        public async Task RejectProductAsync(int id, string adminId, string reason)
            => await _productRepository.RejectProductAsync(id, adminId, reason);

        /// <inheritdoc/>
        public async Task SubmitProductForReviewAsync(int productId)
            => await _productRepository.SubmitProductForReviewAsync(productId);

        /// <inheritdoc/>
        public async Task<int> CleanupExpiredRejectedProductsAsync(int expirationSeconds = 60)
            => await _productRepository.CleanupExpiredRejectedAsync(expirationSeconds);

        /// <inheritdoc/>
        public async Task<IEnumerable<ProductReviewDto>> GetRecentRejectedProductsAsync(int top = 10)
            => await _productRepository.GetRecentRejectedProductsAsync(top);

        /// <inheritdoc/>
        public async Task<PagedResult<ProductReviewDto>> GetPendingProductsPagedAsync(int page, int pageSize)
        {
            var (items, total) = await _productRepository.GetPendingProductsPagedAsync(page, pageSize);
            return PagedResult<ProductReviewDto>.Create(items.ToList(), total, page, pageSize);
        }

        /// <inheritdoc/>
        public async Task<PagedResult<ProductReviewDto>> GetRejectedProductsPagedAsync(int page, int pageSize)
        {
            var (items, total) = await _productRepository.GetRejectedProductsPagedAsync(page, pageSize);
            return PagedResult<ProductReviewDto>.Create(items.ToList(), total, page, pageSize);
        }

        /// <inheritdoc/>
        public async Task ResetToPendingAsync(int productId)
            => await _productRepository.ResetToPendingAsync(productId);

        /// <inheritdoc/>
        public async Task<(int Total, int Published, int Unpublished, int Pending)> GetProductStatusCountsAsync()
            => await _productRepository.GetStatusCountsAsync();

        /// <inheritdoc/>
        public async Task ForceUnpublishAsync(int id, string? reason)
            => await _productRepository.ForceUnpublishAsync(id, reason);
    }
}
