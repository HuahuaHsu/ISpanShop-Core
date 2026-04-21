using ISpanShop.Models.DTOs.Products;
using ISpanShop.Models.DTOs.Inventories;
using ISpanShop.Services.Products;
using ISpanShop.Services.Inventories;
using Microsoft.AspNetCore.Authorization;
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

        public SellerProductsApiController(IProductService productService)
        {
            _productService = productService;
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
                PageSize         = pageSize
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
        public IActionResult CreateProduct([FromForm] CreateProductRequest request)
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
                BrandId            = request.BrandId ?? 0,
                Name               = request.Name,
                Description        = request.Description,
                VideoUrl           = request.VideoUrl,
                SpecDefinitionJson = request.SpecDefinitionJson ?? "[]",
                UploadImages       = request.Images ?? new List<Microsoft.AspNetCore.Http.IFormFile>(),
                MainImageIndex     = request.MainImageIndex,
                Variants           = new List<ProductVariantCreateDto>()
            };

            _productService.CreateProduct(dto);
            return StatusCode(StatusCodes.Status201Created, new { message = "商品建立成功，等待審核" });
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
            return Ok(new { message = "商品更新成功" });
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
            ViewCount    = dto.ViewCount
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
