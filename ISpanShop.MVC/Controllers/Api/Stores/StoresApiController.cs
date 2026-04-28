using ISpanShop.Models.DTOs.Products;
using ISpanShop.Models.DTOs.Stores;
using ISpanShop.MVC.Models.Dto;
using ISpanShop.Services.Products;
using ISpanShop.Services.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISpanShop.MVC.Controllers.Api.Stores
{
    /// <summary>前台賣場公開 API（不需登入）</summary>
    [ApiController]
    [Route("api/stores")]
    [Produces("application/json")]
    [AllowAnonymous]
    public class StoresApiController : ControllerBase
    {
        private readonly IFrontStoreService _frontStoreService;
        private readonly IProductService    _productService;

        public StoresApiController(
            IFrontStoreService frontStoreService,
            IProductService    productService)
        {
            _frontStoreService = frontStoreService;
            _productService    = productService;
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/stores/{storeId}
        // 取得賣場公開資訊
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// 取得賣場基本資訊（名稱、描述、Logo、上架商品數等）
        /// </summary>
        [HttpGet("{storeId:int}")]
        [ProducesResponseType(typeof(StorePublicProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStoreProfile(int storeId)
        {
            var profile = await _frontStoreService.GetPublicStoreProfileAsync(storeId);
            if (profile == null)
                return NotFound(new { success = false, message = "找不到此賣場" });

            return Ok(new { success = true, data = profile, message = "" });
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/stores/{storeId}/products
        // 取得賣場已上架商品（分頁）
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// 取得賣場的已上架商品列表（分頁）
        /// </summary>
        [HttpGet("{storeId:int}/products")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStoreProducts(
            int storeId,
            [FromQuery] int     page     = 1,
            [FromQuery] int     pageSize = 20,
            [FromQuery] string? sortBy   = null)
        {
            // 停權賣場 / 黑名單店主 → GetPublicStoreProfileAsync 回傳 null
            var profile = await _frontStoreService.GetPublicStoreProfileAsync(storeId);
            if (profile == null)
                return NotFound(new { success = false, message = "找不到此賣場" });

            page     = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var criteria = new ProductSearchCriteria
            {
                StoreId    = storeId,
                Status     = 1,          // 只顯示已上架
                SortOrder  = sortBy ?? "date_desc",
                PageNumber = page,
                PageSize   = pageSize
            };

            var result = _productService.GetProductsPaged(criteria);

            var items = result.Data.Select(p => new ProductListItemDto
            {
                Id            = p.Id,
                Name          = p.Name,
                Price         = p.MinPrice ?? 0m,
                OriginalPrice = null,
                ImageUrl      = p.MainImageUrl ?? string.Empty,
                SoldCount     = p.TotalSales ?? 0,
                TotalStock    = p.TotalStock,
                Location      = string.Empty,
                CategoryId    = p.CategoryId,
                Rating        = null
            }).ToList();

            return Ok(new
            {
                success = true,
                data = new ProductListResponseDto
                {
                    Items    = items,
                    Total    = result.TotalCount,
                    Page     = result.CurrentPage,
                    PageSize = result.PageSize
                },
                message = ""
            });
        }
    }
}
