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

            var mode = request.Mode?.ToLower() ?? "draft";
            var dto = new ProductCreateDto
            {
                StoreId            = storeId,  // 使用 JWT 中的 StoreId，忽略前端傳入的值
                CategoryId         = request.CategoryId,
                BrandId            = request.BrandId,  // null = 未指定品牌，不可用 ?? 0（0 非合法 BrandId）
                Name               = request.Name,
                Description        = request.Description,
                VideoUrl           = request.VideoUrl,
                SpecDefinitionJson = request.SpecDefinitionJson ?? "[]",
                Variants           = new List<ProductVariantCreateDto>(),
                Status             = (byte)(mode == "submit" ? 2 : 0), // submit=2(待審核), draft=0(未上架)
                ReviewStatus       = (mode == "submit") ? 0 : 4, // 0=待審核, 4=草稿
                AttributesJson     = request.AttributesJson
            };

            // 解析 VariantsJson → 有規格時填入 dto.Variants
            if (!string.IsNullOrWhiteSpace(request.VariantsJson) && request.VariantsJson != "[]")
            {
                try
                {
                    var parsed = System.Text.Json.JsonSerializer.Deserialize<List<ProductVariantCreateDto>>(
                        request.VariantsJson,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (parsed != null && parsed.Count > 0)
                        dto.Variants = parsed;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("解析 VariantsJson 失敗: " + ex.Message);
                }
            }

            // 無規格商品：以 Price / Stock 建立預設 variant
            if (dto.Variants.Count == 0)
            {
                dto.Variants.Add(new ProductVariantCreateDto
                {
                    VariantName   = "預設",
                    SpecValueJson = "{}",
                    Price         = request.Price ?? 0,
                    Stock         = request.Stock ?? 0,
                });
            }

            Console.WriteLine($"=== CreateProduct Debug === mode={mode} Status={dto.Status} ReviewStatus={dto.ReviewStatus} Price={dto.Variants.FirstOrDefault()?.Price} Stock={dto.Variants.FirstOrDefault()?.Stock} Variants={dto.Variants.Count}");

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

            var successMsg = (mode == "submit") ? "商品已提交審核，請等待管理員審核" : "商品草稿已儲存";
            return StatusCode(StatusCodes.Status201Created, new { success = true, data = new { productId }, message = successMsg });
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

            // 待審核商品不允許編輯 (ReviewStatus 0=待審核, 3=重新送審)
            if (existing.ReviewStatus == 0 || existing.ReviewStatus == 3 || existing.Status == 2)
                return BadRequest(new { success = false, message = "商品審核中，無法編輯" });

            var mode = request.Mode?.ToLower() ?? "draft";
            var dto = new ProductUpdateDto
            {
                Id                 = id,
                CategoryId         = request.CategoryId,
                BrandId            = request.BrandId,
                Name               = request.Name,
                Description        = request.Description,
                SpecDefinitionJson = request.SpecDefinitionJson,
                MainImageUrl       = request.MainImageUrl,
                Status             = 0, // 絕對規則：編輯商品時保持 Status=0 (未上架)
                ReviewStatus       = (mode == "submit") ? 0 : (existing.ReviewStatus == 2 ? 4 : existing.ReviewStatus),
                VariantsJson       = request.VariantsJson,
                AttributesJson     = request.AttributesJson
            };

            // 如果原本是退回狀態(2)且現在要送審，則改為重新送審(3)
            if (mode == "submit" && (existing.ReviewStatus == 2 || existing.Status == 3))
            {
                dto.ReviewStatus = 3; // 重新送審
            }

            _productService.UpdateProduct(dto);
            
            var message = (mode == "submit") ? "商品已送審" : "商品已儲存為草稿";
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

            // Debug：印出前後端 URL 格式以利排查不一致問題
            Console.WriteLine("=== 圖片更新 Debug ===");
            Console.WriteLine("收到的 existingImages:");
            foreach (var img in existingImages ?? new List<string>())
                Console.WriteLine("  前端送來: " + img);
            Console.WriteLine("資料庫中的圖片:");
            foreach (var img in existing.Images)
                Console.WriteLine("  資料庫: " + img);

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

            // 刪除不在 existingImages 中的舊圖片（內部會做 URL 正規化比對）
            _productService.DeleteProductImagesExcept(id, existingImages ?? new List<string>(), _env.WebRootPath);

            // 上傳新圖片
            var productImages = new List<ProductImage>();
            if (images != null && images.Count > 0)
            {
                var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "products");
                Directory.CreateDirectory(uploadDir);

                // 計算排序起始值（從實際保留的圖片數量開始，repository 已重排 SortOrder 從 0 起）
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
        // PUT api/seller/products/{id}/submit-review
        // 草稿商品送出審核：把 Status=0（草稿）改為 Status=2（待審核）
        // ──────────────────────────────────────────────────────────
        [HttpPut("{id:int}/submit-review")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult SubmitProductForReview(int id)
        {
            var storeIdClaim = User.FindFirst("StoreId")?.Value
                            ?? User.FindFirst(c => c.Type.EndsWith("/StoreId"))?.Value;
            if (string.IsNullOrEmpty(storeIdClaim) || !int.TryParse(storeIdClaim, out var storeId))
                return Unauthorized(new { success = false, message = "無法識別賣家身份" });

            var existing = _productService.GetProductDetail(id);
            if (existing == null)
                return NotFound(new { success = false, message = "商品不存在" });

            if (existing.StoreId != storeId)
                return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "無權操作此商品" });

            if (existing.Status == 1)
                return BadRequest(new { success = false, message = "商品已上架，無需重新送審" });

            if (existing.Status == 2)
                return BadRequest(new { success = false, message = "商品已在審核中" });

            _productService.ChangeProductStatus(id, 2);
            return Ok(new { success = true, message = "商品已送出審核，請等待管理員審核" });
        }

        // ──────────────────────────────────────────────────────────
        // POST api/seller/products/upload-image
        // 上傳商品描述圖片 (供編輯器使用)
        // ──────────────────────────────────────────────────────────
        [HttpPost("upload-image")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UploadDescriptionImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest(new { success = false, message = "請選擇圖片" });

            // 限制 5MB
            if (image.Length > 5 * 1024 * 1024)
                return BadRequest(new { success = false, message = "圖片大小不能超過 5MB" });

            var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!allowedExts.Contains(ext))
                return BadRequest(new { success = false, message = "只支援 JPG、PNG、WEBP 格式" });

            var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "descriptions");
            Directory.CreateDirectory(uploadPath);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            var relativeUrl = $"/uploads/descriptions/{fileName}";
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var fullUrl = $"{baseUrl}{relativeUrl}";

            return Ok(new { 
                success = true, 
                url = fullUrl,          // 完整網址
                imageUrl = relativeUrl  // 相對路徑 (保留相容性)
            });
        }

        // ──────────────────────────────────────────────────────────
        // 內部映射方法
        // ──────────────────────────────────────────────────────────
        private static string ToStatusText(byte? status, int reviewStatus) => status switch
        {
            1 => "已上架",
            2 => "待審核",
            3 => "審核退回",
            4 => "強制下架",
            0 => (reviewStatus == 0 || reviewStatus == 3) ? "待審核" : "未上架",
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
            StatusText   = ToStatusText(dto.Status, dto.ReviewStatus),
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
            StatusText         = ToStatusText(dto.Status, dto.ReviewStatus),
            MinPrice           = dto.MinPrice,
            MaxPrice           = dto.MaxPrice,
            SpecDefinitionJson = dto.SpecDefinitionJson,
            RejectReason       = dto.RejectReason,
            ReviewStatus       = dto.ReviewStatus,
            CreatedAt          = dto.CreatedAt,
            UpdatedAt          = dto.UpdatedAt,
            AttributesJson     = dto.AttributesJson,
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
