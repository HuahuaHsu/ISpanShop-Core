using ISpanShop.Models.EfModels;
using ISpanShop.MVC.Models.Dto;
using ISpanShop.Services.Promotions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.MVC.Controllers.Api.Promotions
{
    /// <summary>前台活動 API（唯讀，不需登入）</summary>
    [ApiController]
    [Route("api/promotions")]
    [Produces("application/json")]
    public class PromotionApiController : ControllerBase
    {
        private readonly PromotionService _promotionSvc;
        private readonly ISpanShopDBContext _db;

        public PromotionApiController(PromotionService promotionSvc, ISpanShopDBContext db)
        {
            _promotionSvc = promotionSvc;
            _db = db;
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/promotions/active
        // 回傳目前進行中的活動（Status==1, StartTime<=now<=EndTime）
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// 取得目前進行中的活動列表
        /// </summary>
        /// <param name="type">活動類型篩選：flashSale / discount / limitedBuy（不帶則全部）</param>
        /// <param name="limit">最多幾筆（預設 5，上限 20）</param>
        [HttpGet("active")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActivePromotions(
            [FromQuery] string? type  = null,
            [FromQuery] int     limit = 5)
        {
            var promotions = await _promotionSvc.GetActivePromotionsAsync(type, limit);

            var items = promotions.Select(p =>
            {
                // 每個 PromotionItem 取其商品的主圖（IsMain 優先，其次 SortOrder 最小）
                var productImageUrls = p.PromotionItems
                    .Select(pi => pi.Product?.ProductImages?
                        .OrderBy(img => img.IsMain == true ? 0 : 1)
                        .ThenBy(img => img.SortOrder ?? 999)
                        .FirstOrDefault()?.ImageUrl)
                    .Where(url => !string.IsNullOrEmpty(url))
                    .Distinct()
                    .Take(4)
                    .ToList();

                return new PromotionListItemDto
                {
                    Id             = p.Id,
                    Title          = p.Name,
                    Subtitle       = string.IsNullOrWhiteSpace(p.Description) ? null : p.Description,
                    Type           = PromotionService.GetTypeCode(p.PromotionType),
                    TypeLabel      = PromotionService.GetTypeLabel(p.PromotionType),
                    BannerImageUrl = productImageUrls.FirstOrDefault(),
                    ProductImages  = productImageUrls!,
                    LinkUrl        = $"/promotion/{p.Id}",
                    StartDate      = p.StartTime,
                    EndDate        = p.EndTime
                };
            }).ToList();

            return Ok(new
            {
                success = true,
                data    = items,
                message = ""
            });
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/promotions/{id}
        // 公開活動詳情（含 bannerImageUrl、productImages）
        // ──────────────────────────────────────────────────────────

        /// <summary>取得單一活動詳情（公開）</summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPromotion(int id)
        {
            var promo = await _db.Promotions
                .AsNoTracking()
                .Include(p => p.PromotionItems)
                    .ThenInclude(pi => pi.Product)
                        .ThenInclude(prod => prod.ProductImages)
                .Include(p => p.PromotionRules)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted && p.Status == 1);

            if (promo == null)
                return NotFound(new { success = false, message = "活動不存在或已結束" });

            var productImageUrls = promo.PromotionItems
                .Select(pi => pi.Product?.ProductImages?
                    .OrderBy(img => img.IsMain == true ? 0 : 1)
                    .ThenBy(img => img.SortOrder ?? 999)
                    .FirstOrDefault()?.ImageUrl)
                .Where(url => !string.IsNullOrEmpty(url))
                .Distinct()
                .Take(4)
                .ToList();

            var rules = promo.PromotionRules
                .Select(r => new
                {
                    ruleType     = r.RuleType,
                    threshold    = r.Threshold,
                    discountType = r.DiscountType,
                    discountValue= r.DiscountValue
                })
                .ToList();

            return Ok(new
            {
                success = true,
                data = new
                {
                    id             = promo.Id,
                    title          = promo.Name,
                    subtitle       = string.IsNullOrWhiteSpace(promo.Description) ? null : promo.Description,
                    type           = PromotionService.GetTypeCode(promo.PromotionType),
                    typeLabel      = PromotionService.GetTypeLabel(promo.PromotionType),
                    bannerImageUrl = productImageUrls.FirstOrDefault(),
                    productImages  = productImageUrls!,
                    linkUrl        = $"/promotion/{promo.Id}",
                    startDate      = promo.StartTime,
                    endDate        = promo.EndTime,
                    rules
                }
            });
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/promotions/{id}/products
        // 公開活動商品列表（分頁）
        // ──────────────────────────────────────────────────────────

        /// <summary>取得活動的商品列表（公開，支援分頁）</summary>
        [HttpGet("{id:int}/products")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPromotionProducts(
            int id,
            [FromQuery] int    page      = 1,
            [FromQuery] int    pageSize  = 20,
            [FromQuery] string sortBy    = "default",
            [FromQuery] string priceOrder = "")
        {
            page     = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var exists = await _db.Promotions
                .AnyAsync(p => p.Id == id && !p.IsDeleted && p.Status == 1);

            if (!exists)
                return NotFound(new { success = false, message = "活動不存在或已結束" });

            var query = _db.PromotionItems
                .AsNoTracking()
                .Where(pi => pi.PromotionId == id
                          && !pi.Product.IsDeleted
                          && pi.Product.Status == 1)   // 只顯示已上架商品，防止點擊後 404
                .Include(pi => pi.Product)
                    .ThenInclude(p => p.ProductImages);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // 排序
            IQueryable<PromotionItem> ordered = sortBy switch
            {
                "sales"   => query.OrderByDescending(pi => pi.SoldCount),
                "priceAsc"  when priceOrder == "asc"  => query.OrderBy(pi => pi.DiscountPrice ?? pi.OriginalPrice),
                "priceDesc" when priceOrder == "desc" => query.OrderByDescending(pi => pi.DiscountPrice ?? pi.OriginalPrice),
                _ => query.OrderBy(pi => pi.Id)
            };

            if (!string.IsNullOrEmpty(priceOrder) && sortBy != "priceAsc" && sortBy != "priceDesc")
            {
                ordered = priceOrder == "asc"
                    ? query.OrderBy(pi => pi.DiscountPrice ?? pi.OriginalPrice)
                    : query.OrderByDescending(pi => pi.DiscountPrice ?? pi.OriginalPrice);
            }

            var items = await ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(pi => new
                {
                    productId     = pi.ProductId,
                    productName   = pi.Product.Name,
                    imageUrl      = pi.Product.ProductImages
                                       .Where(img => img.IsMain == true)
                                       .Select(img => img.ImageUrl)
                                       .FirstOrDefault()
                                   ?? pi.Product.ProductImages
                                       .OrderBy(img => img.SortOrder)
                                       .Select(img => img.ImageUrl)
                                       .FirstOrDefault(),
                    originalPrice = pi.OriginalPrice,
                    discountPrice = pi.DiscountPrice,
                    discountPercent = pi.DiscountPercent,
                    soldCount     = pi.SoldCount,
                    quantityLimit = pi.QuantityLimit,
                    stockLimit    = pi.StockLimit,
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = new
                {
                    items,
                    totalCount,
                    page,
                    pageSize,
                    totalPages
                }
            });
        }
    }
}
