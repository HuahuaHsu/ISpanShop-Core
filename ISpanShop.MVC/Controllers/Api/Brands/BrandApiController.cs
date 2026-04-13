using ISpanShop.MVC.Models.Dto;
using ISpanShop.Services.Brands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISpanShop.MVC.Controllers.Api.Brands
{
    /// <summary>前台品牌 API（唯讀，不需登入）</summary>
    [ApiController]
    [Route("api/brands")]
    [Produces("application/json")]
    public class BrandApiController : ControllerBase
    {
        private readonly BrandService _brandSvc;

        public BrandApiController(BrandService brandSvc)
        {
            _brandSvc = brandSvc;
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/brands
        // 回傳有上架商品的品牌清單（含商品數）
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// 取得品牌列表，含各品牌的上架商品數。
        /// subCategoryId 優先（直接篩）；categoryId 主分類自動展開子分類；keyword 篩選品牌名稱。
        /// </summary>
        /// <param name="categoryId">主分類篩選（可選，自動展開子分類）</param>
        /// <param name="subCategoryId">子分類篩選（優先於 categoryId，直接篩）</param>
        /// <param name="keyword">品牌名稱關鍵字（可選）</param>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBrands(
            [FromQuery] int?    categoryId    = null,
            [FromQuery] int?    subCategoryId = null,
            [FromQuery] string? keyword       = null)
        {
            var brands = await _brandSvc.GetBrandsAsync(categoryId, keyword, subCategoryId);

            var items = brands.Select(b => new BrandListItemDto
            {
                Id           = b.Id,
                Name         = b.Name,
                ProductCount = b.ProductCount
            }).ToList();

            return Ok(new
            {
                success = true,
                data    = items,
                message = ""
            });
        }
    }
}
