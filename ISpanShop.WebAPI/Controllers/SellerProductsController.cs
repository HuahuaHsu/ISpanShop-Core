using ISpanShop.Models.DTOs;
using ISpanShop.Services.Interfaces;
using ISpanShop.WebAPI.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ISpanShop.WebAPI.Controllers
{
    /// <summary>賣家商品管理 API</summary>
    [ApiController]
    [Route("api/seller/products")]
    [Produces("application/json")]
    public class SellerProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public SellerProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // ──────────────────────────────────────────────────────────
        // GET api/seller/products
        // 分頁取得商品列表（暫不過濾賣家，之後加身份驗證再限制）
        // ──────────────────────────────────────────────────────────
        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<ProductListItemResponse>), StatusCodes.Status200OK)]
        public ActionResult<PagedResultDto<ProductListItemResponse>> GetProducts(
            [FromQuery] string? keyword      = null,
            [FromQuery] int?    categoryId   = null,
            [FromQuery] int?    parentCatId  = null,
            [FromQuery] int?    brandId      = null,
            [FromQuery] int?    storeId      = null,
            [FromQuery] int?    status       = null,
            [FromQuery] string? sortBy       = null,
            [FromQuery] int     page         = 1,
            [FromQuery] int     pageSize     = 20)
        {
            var criteria = new ProductSearchCriteria
            {
                Keyword          = keyword,
                CategoryId       = categoryId,
                ParentCategoryId = parentCatId,
                BrandId          = brandId,
                StoreId          = storeId,
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
        // ──────────────────────────────────────────────────────────
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProductDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ProductDetailResponse> GetProduct(int id)
        {
            var dto = _productService.GetProductDetail(id);
            if (dto == null) return NotFound(new { message = "商品不存在" });

            return Ok(MapToDetail(dto));
        }

        // ──────────────────────────────────────────────────────────
        // POST api/seller/products
        // 新增商品（multipart/form-data，支援圖片上傳）
        // 新商品 Status 由 Service 自動決定（關鍵字審查 → 待審核 or 上架）
        // ──────────────────────────────────────────────────────────
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CreateProduct([FromForm] CreateProductRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dto = new ProductCreateDto
            {
                StoreId            = request.StoreId,
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
        // ──────────────────────────────────────────────────────────
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateProduct(int id, [FromBody] UpdateProductRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = _productService.GetProductDetail(id);
            if (existing == null)
                return NotFound(new { message = "商品不存在" });

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
        // ──────────────────────────────────────────────────────────
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteProduct(int id)
        {
            var existing = _productService.GetProductDetail(id);
            if (existing == null)
                return NotFound(new { message = "商品不存在" });

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
            CreatedAt    = dto.CreatedAt
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
