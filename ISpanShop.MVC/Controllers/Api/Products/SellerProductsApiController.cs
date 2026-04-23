using ISpanShop.Models.DTOs.Products;
using ISpanShop.Models.DTOs.Inventories;
using ISpanShop.Models.EfModels;
using ISpanShop.Services.Products;
using ISpanShop.Services.Inventories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace ISpanShop.MVC.Controllers.Api.Products
{
    /// <summary>賣家商品管理 API</summary>
    [ApiController]
    [Route("api/seller/products")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = "FrontendJwt")]
    public class SellerProductsApiController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IWebHostEnvironment _env;

        private static readonly string[] _allowedImageExts = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxImageSize = 5 * 1024 * 1024; // 5 MB

        public SellerProductsApiController(IProductService productService, IWebHostEnvironment env)
        {
            _productService = productService;
            _env = env;
        }

        // ──────────────────────────────────────────────────────────
        // GET api/seller/products
        // 取得該賣家的商品列表（從 JWT 的 StoreId 過濾）
        // ──────────────────────────────────────────────────────────
        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<ProductListItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<PagedResultDto<ProductListItemResponse>> GetProducts(
            [FromQuery] string? keyword      = null,
            [FromQuery] int?    categoryId   = null,
            [FromQuery] int?    parentCatId  = null,
            [FromQuery] int?    brandId      = null,
            [FromQuery] int?    status       = null,
            [FromQuery] string? sortBy       = null,
            [FromQuery] int     page         = 1,
            [FromQuery] int     pageSize     = 20)
        {
            // 從 JWT token 取得 StoreId (支援多種 claim type 格式)
            var storeIdClaim = User.FindFirst("StoreId")?.Value 
                            ?? User.FindFirst(c => c.Type.EndsWith("/StoreId"))?.Value
                            ?? User.FindFirst(c => c.Type.EndsWith("StoreId"))?.Value;
            
            if (string.IsNullOrEmpty(storeIdClaim) || !int.TryParse(storeIdClaim, out var storeId))
            {
                return Unauthorized(new { success = false, message = "無法識別賣家身份，請確認您已開通店家" });
            }

            var criteria = new ProductSearchCriteria
            {
                Keyword          = keyword,
                CategoryId       = categoryId,
                ParentCategoryId = parentCatId,
                BrandId          = brandId,
                StoreId          = storeId,  // 強制使用 JWT 中的 StoreId
                Status           = status,
                SortOrder        = sortBy ?? "date_desc",
                PageNumber       = page,
                PageSize         = pageSize,
                IncludeDeleted   = true  // 賣家需要看到已刪除的商品（違規/刪除 tab）
            };

            var result = _productService.GetProductsPaged(criteria);

            var response = new PagedResultDto<ProductListItemResponse>
            {
                Items      = result.Data.Select(MapToListItem).ToList(),
                TotalCount = result.TotalCount,
                Page       = result.CurrentPage,
                PageSize   = result.PageSize,
                TotalPages = result.TotalPages
            };

            return Ok(response);
        }

        // ──────────────────────────────────────────────────────────
        // GET api/seller/products/{id}
        // 取得單一商品詳情（需驗證是否為該賣家的商品）
        // ──────────────────────────────────────────────────────────
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProductDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<ProductDetailResponse> GetProduct(int id)
        {
            // 從 JWT token 取得 StoreId
            var storeIdClaim = User.FindFirst("StoreId")?.Value;
            if (string.IsNullOrEmpty(storeIdClaim) || !int.TryParse(storeIdClaim, out var storeId))
            {
                return Unauthorized(new { success = false, message = "無法識別賣家身份" });
            }

            var dto = _productService.GetProductDetail(id);
            if (dto == null) return NotFound(new { message = "商品不存在" });

            // 驗證商品是否屬於該賣家
            if (dto.StoreId != storeId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "無權存取此商品" });
            }

            return Ok(MapToDetail(dto));
        }

        // ──────────────────────────────────────────────────────────
        // POST api/seller/products
        // 新增商品（multipart/form-data，支援圖片上傳）
        // StoreId 從 JWT token 自動取得，防止前端偽造
        // ──────────────────────────────────────────────────────────
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 從 JWT token 取得 StoreId
            var storeIdClaim = User.FindFirst("StoreId")?.Value;
            if (string.IsNullOrEmpty(storeIdClaim) || !int.TryParse(storeIdClaim, out var storeId))
            {
                return Unauthorized(new { success = false, message = "無法識別賣家身份，請確認您已開通店家" });
            }

            var dto = new ProductCreateDto
            {
                StoreId            = storeId,  // 使用 JWT 中的 StoreId，忽略前端傳入的值
                CategoryId         = request.CategoryId,
                BrandId            = request.BrandId,  // null = 未指定品牌，不可用 ?? 0（0 非合法 BrandId）
                Name               = request.Name,
                Description        = request.Description,
                VideoUrl           = request.VideoUrl,
                SpecDefinitionJson = request.SpecDefinitionJson ?? "[]",
                Variants           = new List<ProductVariantCreateDto>()
            };

            var productId = _productService.CreateProduct(dto);

            // 儲存圖片：存檔到 wwwroot/uploads/products/，再寫入 ProductImages 表
            if (request.Images != null && request.Images.Count > 0)
            {
                var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "products");
                Directory.CreateDirectory(uploadDir);

                var productImages = new List<ProductImage>();
                int sortOrder = 0;

                for (int i = 0; i < request.Images.Count; i++)
                {
                    var file = request.Images[i];
                    if (file == null || file.Length == 0) continue;

                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!_allowedImageExts.Contains(ext)) continue;
                    if (file.Length > MaxImageSize) continue;

                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadDir, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    productImages.Add(new ProductImage
                    {
                        ProductId = productId,
                        ImageUrl  = $"/uploads/products/{fileName}",
                        IsMain    = (i == request.MainImageIndex),
                        SortOrder = sortOrder++
                    });
                }

                if (productImages.Count > 0)
                {
                    // 確保至少有一張主圖
                    if (!productImages.Any(img => img.IsMain == true))
                        productImages[0].IsMain = true;

                    _productService.AddProductImages(productId, productImages);
                }
            }

            return StatusCode(StatusCodes.Status201Created, new { success = true, data = new { productId }, message = "商品建立成功，等待審核" });
        }

        // ──────────────────────────────────────────────────────────
        // PUT api/seller/products/{id}
        // 更新商品（需驗證是否為該賣家的商品）
        // ──────────────────────────────────────────────────────────
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult UpdateProduct(int id, [FromBody] UpdateProductRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 從 JWT token 取得 StoreId
            var storeIdClaim = User.FindFirst("StoreId")?.Value;
            if (string.IsNullOrEmpty(storeIdClaim) || !int.TryParse(storeIdClaim, out var storeId))
            {
                return Unauthorized(new { success = false, message = "無法識別賣家身份" });
            }

            var existing = _productService.GetProductDetail(id);
            if (existing == null)
                return NotFound(new { message = "商品不存在" });

            // 驗證商品是否屬於該賣家
            if (existing.StoreId != storeId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "無權修改此商品" });
            }

            // 待審核商品不允許編輯
            if (existing.Status == 2)
                return BadRequest(new { success = false, message = "商品審核中，無法編輯" });

            var dto = new ProductUpdateDto
            {
                Id                 = id,
                CategoryId         = request.CategoryId,
                BrandId            = request.BrandId,
                Name               = request.Name,
                Description        = request.Description,
                SpecDefinitionJson = request.SpecDefinitionJson,
                MainImageUrl       = request.MainImageUrl
            };

            _productService.UpdateProduct(dto);
            var message = existing.Status == 3 ? "商品更新成功，已重新送審" : "商品更新成功";
            return Ok(new { success = true, message });
        }

        // ──────────────────────────────────────────────────────────
        // DELETE api/seller/products/{id}
        // 刪除商品（需驗證是否為該賣家的商品）
        // ──────────────────────────────────────────────────────────
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult DeleteProduct(int id)
        {
            // 從 JWT token 取得 StoreId
            var storeIdClaim = User.FindFirst("StoreId")?.Value;
            if (string.IsNullOrEmpty(storeIdClaim) || !int.TryParse(storeIdClaim, out var storeId))
            {
                return Unauthorized(new { success = false, message = "無法識別賣家身份" });
            }

            var existing = _productService.GetProductDetail(id);
            if (existing == null)
                return NotFound(new { message = "商品不存在" });

            // 驗證商品是否屬於該賣家
            if (existing.StoreId != storeId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "無權刪除此商品" });
            }

            _productService.SoftDeleteProduct(id);
            return Ok(new { message = "商品已刪除" });
        }

        // ──────────────────────────────────────────────────────────
        // PUT api/seller/products/{id}/images
        // 更新商品圖片（保留舊圖 + 上傳新圖）
        // ──────────────────────────────────────────────────────────
        [HttpPut("{id:int}/images")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateProductImages(
            int id, 
            [FromForm] List<IFormFile>? images, 
            [FromForm] List<string>? existingImages,
            [FromForm] int mainImageIndex = 0)
        {
            // 從 JWT token 取得 StoreId
            var storeIdClaim = User.FindFirst("StoreId")?.Value;
            if (string.IsNullOrEmpty(storeIdClaim) || !int.TryParse(storeIdClaim, out var storeId))
            {
                return Unauthorized(new { success = false, message = "無法識別賣家身份" });
            }

            var existing = _productService.GetProductDetail(id);
            if (existing == null)
                return NotFound(new { success = false, message = "商品不存在" });

            // 驗證商品是否屬於該賣家
            if (existing.StoreId != storeId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "無權修改此商品" });
            }

            // 如果沒有新圖片也沒有 existingImages，代表前端沒有送圖片相關資料，不做任何處理
            if ((images == null || images.Count == 0) && (existingImages == null || existingImages.Count == 0))
            {
                return Ok(new { success = true, message = "未變更圖片" });
            }

            // 驗證新上傳的圖片格式和大小
            if (images != null && images.Count > 0)
            {
                foreach (var file in images)
                {
                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!_allowedImageExts.Contains(ext))
                    {
                        return BadRequest(new { success = false, message = $"不支援的圖片格式：{ext}，只允許 {string.Join(", ", _allowedImageExts)}" });
                    }
                    if (file.Length > MaxImageSize)
                    {
                        return BadRequest(new { success = false, message = $"圖片大小超過限制（最大 {MaxImageSize / 1024 / 1024} MB）" });
                    }
                }
            }

            // 刪除不在 existingImages 中的舊圖片
            _productService.DeleteProductImagesExcept(id, existingImages ?? new List<string>(), _env.WebRootPath);

            // 上傳新圖片
            var productImages = new List<ProductImage>();
            if (images != null && images.Count > 0)
            {
                var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "products");
                Directory.CreateDirectory(uploadDir);

                // 計算排序起始值（從保留的圖片數量開始）
                int sortOrder = existingImages?.Count ?? 0;

                for (int i = 0; i < images.Count; i++)
                {
                    var file = images[i];
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
                    var filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    productImages.Add(new ProductImage
                    {
                        ProductId = id,
                        ImageUrl  = $"/uploads/products/{fileName}",
                        IsMain    = (sortOrder + i == mainImageIndex),
                        SortOrder = sortOrder + i
                    });
                }

                _productService.AddProductImages(id, productImages);
            }

            // 更新主圖設定（如果需要）
            _productService.UpdateMainImage(id, mainImageIndex);

            var totalImages = (existingImages?.Count ?? 0) + (productImages?.Count ?? 0);
            return Ok(new { success = true, message = "圖片更新成功", imageCount = totalImages });
        }

        // ──────────────────────────────────────────────────────────
        // PATCH api/seller/products/{id}/status
        // 變更商品狀態（上架/下架）
        // ──────────────────────────────────────────────────────────
        [HttpPatch("{id:int}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult UpdateProductStatus(int id, [FromBody] UpdateProductStatusRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 從 JWT token 取得 StoreId
            var storeIdClaim = User.FindFirst("StoreId")?.Value;
            if (string.IsNullOrEmpty(storeIdClaim) || !int.TryParse(storeIdClaim, out var storeId))
            {
                return Unauthorized(new { success = false, message = "無法識別賣家身份" });
            }

            var existing = _productService.GetProductDetail(id);
            if (existing == null)
                return NotFound(new { success = false, message = "商品不存在" });

            // 驗證商品是否屬於該賣家
            if (existing.StoreId != storeId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "無權修改此商品" });
            }

            // 只允許改為 0 (下架) 或 1 (上架)
            if (request.Status != 0 && request.Status != 1)
            {
                return BadRequest(new { success = false, message = "狀態值無效，只能設為 0 (下架) 或 1 (上架)" });
            }

            // 狀態轉換驗證：
            // - 只有已上架(1)可以下架(0)
            // - 只有下架(0)可以上架(1)
            // - 待審核(2)和退回(3)不能直接上架
            if (request.Status == 1 && existing.Status != 0)
            {
                return BadRequest(new { success = false, message = "只有已下架的商品才能上架" });
            }
            if (request.Status == 0 && existing.Status != 1)
            {
                return BadRequest(new { success = false, message = "只有已上架的商品才能下架" });
            }

            _productService.ChangeProductStatus(id, request.Status);

            var statusText = request.Status == 1 ? "上架" : "下架";
            return Ok(new { success = true, message = $"商品已{statusText}" });
        }

        // ──────────────────────────────────────────────────────────
        // 內部映射方法
        // ──────────────────────────────────────────────────────────
        private static string ToStatusText(byte? status) => status switch
        {
            1 => "已上架",
            2 => "待審核",
            3 => "審核退回",
            0 => "未上架",
            _ => "未知"
        };

        private static ProductListItemResponse MapToListItem(ProductListDto dto) => new()
        {
            Id           = dto.Id,
            Name         = dto.Name,
            StoreName    = dto.StoreName,
            CategoryName = dto.CategoryName,
            BrandName    = dto.BrandName,
            MinPrice     = dto.MinPrice,
            MaxPrice     = dto.MaxPrice,
            Status       = dto.Status,
            StatusText   = ToStatusText(dto.Status),
            MainImageUrl = dto.MainImageUrl,
            CreatedAt    = dto.CreatedAt,
            TotalStock   = dto.TotalStock,
            TotalSales   = dto.TotalSales,
            ViewCount    = dto.ViewCount,
            RejectReason = dto.RejectReason,
            ReviewStatus = dto.ReviewStatus,
            IsDeleted    = dto.IsDeleted
        };

        private static ProductDetailResponse MapToDetail(ProductDetailDto dto) => new()
        {
            Id                 = dto.Id,
            Name               = dto.Name,
            StoreId            = dto.StoreId,
            StoreName          = dto.StoreName,
            CategoryId         = dto.CategoryId,
            CategoryName       = dto.CategoryName,
            BrandId            = dto.BrandId,
            BrandName          = dto.BrandName,
            Description        = dto.Description,
            Status             = dto.Status,
            StatusText         = ToStatusText(dto.Status),
            MinPrice           = dto.MinPrice,
            MaxPrice           = dto.MaxPrice,
            SpecDefinitionJson = dto.SpecDefinitionJson,
            RejectReason       = dto.RejectReason,
            CreatedAt          = dto.CreatedAt,
            UpdatedAt          = dto.UpdatedAt,
            Images             = dto.Images,
            Variants           = dto.Variants
                .Where(v => v.IsDeleted != true)
                .Select(v => new VariantDetailResponse
                {
                    Id            = v.Id,
                    SkuCode       = v.SkuCode,
                    VariantName   = v.VariantName,
                    Price         = v.Price,
                    Stock         = v.Stock,
                    SafetyStock   = v.SafetyStock,
                    SpecValueJson = v.SpecValueJson
                }).ToList()
        };
    }
}
