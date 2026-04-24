using ISpanShop.Models.DTOs.Products;
using ISpanShop.Services.Products;
using ISpanShop.Services.Inventories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISpanShop.MVC.Controllers.Api.Products
{
    /// <summary>賣家規格管理 API（隸屬於特定商品）</summary>
    [ApiController]
    [Route("api/seller/products/{productId:int}/variants")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = "FrontendJwt")]
    public class SellerVariantsApiController : ControllerBase
    {
        private readonly IProductService _productService;

        public SellerVariantsApiController(IProductService productService)
        {
            _productService = productService;
        }

        // ──────────────────────────────────────────────────────────
        // GET api/admin/seller/products/{productId}/variants
        // ──────────────────────────────────────────────────────────
        [HttpGet]
        [ProducesResponseType(typeof(List<VariantDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<VariantDetailResponse>> GetVariants(int productId)
        {
            var product = _productService.GetProductDetail(productId);
            if (product == null)
                return NotFound(new { message = "商品不存在" });

            var variants = product.Variants
                .Where(v => v.IsDeleted != true)
                .Select(MapToVariant)
                .ToList();

            return Ok(variants);
        }

        // ──────────────────────────────────────────────────────────
        // POST api/admin/seller/products/{productId}/variants
        // ──────────────────────────────────────────────────────────
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult AddVariant(int productId, [FromBody] CreateVariantRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = _productService.GetProductDetail(productId);
            if (product == null)
                return NotFound(new { message = "商品不存在" });

            var dto = new ProductVariantCreateDto
            {
                SkuCode       = request.SkuCode,
                VariantName   = request.VariantName,
                SpecValueJson = request.SpecValueJson,
                Price         = request.Price,
                Stock         = request.Stock,
                SafetyStock   = request.SafetyStock
            };

            var error = _productService.AddVariant(productId, dto);
            if (error != null)
                return BadRequest(new { message = error });

            return StatusCode(StatusCodes.Status201Created, new { message = "規格新增成功" });
        }

        // ──────────────────────────────────────────────────────────
        // PUT api/admin/seller/products/{productId}/variants/{variantId}
        // ──────────────────────────────────────────────────────────
        [HttpPut("{variantId:int}")]
        [ProducesResponseType(typeof(VariantDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateVariant(int productId, int variantId, [FromBody] UpdateVariantRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var variant = _productService.GetVariantById(variantId);
            if (variant == null || variant.ProductId != productId)
                return NotFound(new { message = "規格不存在或不屬於此商品" });

            var dto = new ProductVariantUpdateDto
            {
                Id          = variantId,
                SkuCode     = request.SkuCode ?? variant.SkuCode,
                Price       = request.Price,
                Stock       = request.Stock,
                SafetyStock = request.SafetyStock
            };

            _productService.UpdateVariant(dto);

            var updated = _productService.GetVariantById(variantId);
            return Ok(MapToVariant(updated!));
        }

        // ──────────────────────────────────────────────────────────
        // DELETE api/admin/seller/products/{productId}/variants/{variantId}
        // ──────────────────────────────────────────────────────────
        [HttpDelete("{variantId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteVariant(int productId, int variantId)
        {
            var variant = _productService.GetVariantById(variantId);
            if (variant == null || variant.ProductId != productId)
                return NotFound(new { message = "規格不存在或不屬於此商品" });

            _productService.SoftDeleteVariant(variantId);
            return Ok(new { message = "規格已刪除" });
        }

        private static VariantDetailResponse MapToVariant(ProductVariantDetailDto v) => new()
        {
            Id            = v.Id,
            SkuCode       = v.SkuCode,
            VariantName   = v.VariantName,
            Price         = v.Price,
            Stock         = v.Stock,
            SafetyStock   = v.SafetyStock,
            SpecValueJson = v.SpecValueJson
        };
    }
}
