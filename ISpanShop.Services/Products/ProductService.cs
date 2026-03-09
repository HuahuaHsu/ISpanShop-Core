using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISpanShop.Models.DTOs.Products;
using ISpanShop.Models.DTOs.Categories;
using ISpanShop.Models.DTOs.Common;
using ISpanShop.Repositories.Products;
using ISpanShop.Services.Products;

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
        /// 建立新商品
        /// </summary>
        public void CreateProduct(ProductCreateDto dto)
        {
            var product = new ISpanShop.Models.EfModels.Product
            {
                StoreId            = dto.StoreId,
                CategoryId         = dto.CategoryId,
                BrandId            = dto.BrandId,
                Name               = dto.Name,
                Description        = dto.Description,
                VideoUrl           = dto.VideoUrl,
                SpecDefinitionJson = dto.SpecDefinitionJson,
                CreatedAt          = DateTime.Now,
                UpdatedAt          = DateTime.Now,
                Status             = 2, // 待審核
                ReviewStatus       = 0
            };

            if (dto.Variants != null && dto.Variants.Count > 0)
            {
                product.MinPrice = dto.Variants.Min(v => v.Price);
                product.MaxPrice = dto.Variants.Max(v => v.Price);
                product.ProductVariants = dto.Variants.Select(variantDto =>
                    new ISpanShop.Models.EfModels.ProductVariant
                    {
                        SkuCode       = string.IsNullOrWhiteSpace(variantDto.SkuCode)
                                        ? $"{Guid.NewGuid().ToString("N")[..8].ToUpper()}"
                                        : variantDto.SkuCode,
                        VariantName   = variantDto.VariantName,
                        SpecValueJson = variantDto.SpecValueJson,
                        Price         = variantDto.Price,
                        Stock         = variantDto.Stock,
                        SafetyStock   = variantDto.SafetyStock,
                    }).ToList();
            }

            _productRepository.AddProduct(product);
        }

        /// <summary>
        /// 分頁取得商品列表
        /// </summary>
        public PagedResult<ProductListDto> GetProductsPaged(ProductSearchCriteria criteria)
        {
            var (items, totalCount) = _productRepository.GetProductsPaged(criteria);
            var dtos = items.Select(p => new ProductListDto
            {
                Id           = p.Id,
                StoreName    = p.Store?.StoreName ?? "未知商店",
                CategoryName = p.Category?.Name ?? "未分類",
                BrandName    = p.Brand?.Name ?? "未設定",
                Name         = p.Name,
                MinPrice     = p.MinPrice,
                MaxPrice     = p.MaxPrice,
                Status       = p.Status,
                CreatedAt    = p.CreatedAt,
                ReviewStatus = p.ReviewStatus,
                ReviewedBy   = p.ReviewedBy,
                ReviewDate   = p.ReviewDate,
                RejectReason = p.RejectReason,
                MainImageUrl = p.ProductImages
                    ?.FirstOrDefault(img => img.IsMain == true)?.ImageUrl
                    ?? p.ProductImages?.FirstOrDefault()?.ImageUrl
                    ?? "https://via.placeholder.com/400x400?text=No+Image"
            }).ToList();
            return PagedResult<ProductListDto>.Create(dtos, totalCount, criteria.PageNumber, criteria.PageSize);
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
                int attempts = 0;
                do { skuCode = Guid.NewGuid().ToString("N")[..8].ToUpper(); attempts++; }
                while (_productRepository.IsSkuExists(skuCode) && attempts < 5);
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

            try { _productRepository.AddVariant(variant); }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
                when (ex.InnerException?.Message.Contains("UNIQUE") == true ||
                      ex.InnerException?.Message.Contains("unique") == true ||
                      ex.InnerException?.Message.Contains("duplicate") == true)
            { return "此 SKU 代碼已被使用，請更換。"; }

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

        /// <summary>
        /// 變更商品狀態
        /// </summary>
        public void ChangeProductStatus(int id, byte newStatus)
        {
            _productRepository.UpdateProductStatus(id, newStatus);
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

        /// <inheritdoc/>
        public async Task<SimulateAutoReviewResult> SimulateAutoReviewAsync()
            => await _productRepository.SimulateAutoReviewAsync();
    }
}
