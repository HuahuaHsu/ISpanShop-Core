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
                    .ThenInclude(p => p.ProductImages)
                .Include(pi => pi.Product)
                    .ThenInclude(p => p.ProductVariants);

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

            // Step 1：DB 查詢，取出有庫存規格的最低價（nullable，空時為 null）
            var rawItems = await ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(pi => new
                {
                    productId           = pi.ProductId,
                    productName         = pi.Product.Name,
                    imageUrl            = pi.Product.ProductImages
                                            .Where(img => img.IsMain == true)
                                            .Select(img => img.ImageUrl)
                                            .FirstOrDefault()
                                        ?? pi.Product.ProductImages
                                            .OrderBy(img => img.SortOrder)
                                            .Select(img => img.ImageUrl)
                                            .FirstOrDefault(),
                    storedOriginalPrice = pi.OriginalPrice,
                    storedDiscountPrice = pi.DiscountPrice,
                    discountPercent     = pi.DiscountPercent,
                    soldCount           = pi.SoldCount,
                    quantityLimit       = pi.QuantityLimit,
                    stockLimit          = pi.StockLimit,
                    // 有庫存（Stock > 0 且未刪除）的規格最低價；無庫存時為 null
                    minInStockPrice     = pi.Product.ProductVariants
                                            .Where(v => v.IsDeleted != true && v.Stock > 0)
                                            .Min(v => (decimal?)v.Price),
                })
                .ToListAsync();

            // Step 2：在記憶體中計算正確的原價與折扣價
            var items = rawItems.Select(r =>
            {
                var originalPrice = r.minInStockPrice ?? r.storedOriginalPrice;
                var discountPrice = r.discountPercent != null && r.discountPercent > 0
                    ? (decimal?)Math.Round(originalPrice * r.discountPercent.Value / 100m, 0)
                    : r.storedDiscountPrice;
                return new
                {
                    productId       = r.productId,
                    productName     = r.productName,
                    imageUrl        = r.imageUrl,
                    originalPrice,
                    discountPrice,
                    discountPercent = r.discountPercent,
                    soldCount       = r.soldCount,
                    quantityLimit   = r.quantityLimit,
                    stockLimit      = r.stockLimit,
                };
            }).ToList();

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

        // ──────────────────────────────────────────────────────────
        // GET /api/promotions/product/{productId}
        // 取得指定商品目前參加的進行中活動（含活動項目與主要規則）
        // ──────────────────────────────────────────────────────────

        /// <summary>取得商品目前參加的進行中活動（公開）</summary>
        [HttpGet("product/{productId:int}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPromotionsByProduct(int productId)
        {
            var now = DateTime.Now;

            var promotionItems = await _db.PromotionItems
                .AsNoTracking()
                .Where(pi => pi.ProductId == productId
                          && !pi.Promotion.IsDeleted
                          && pi.Promotion.Status == 1
                          && pi.Promotion.StartTime <= now
                          && pi.Promotion.EndTime >= now)
                .Include(pi => pi.Promotion)
                    .ThenInclude(p => p.PromotionRules)
                .Include(pi => pi.Product)
                    .ThenInclude(p => p.ProductVariants)
                .ToListAsync();

            var data = promotionItems.Select(pi =>
            {
                var rule = pi.Promotion.PromotionRules.FirstOrDefault();

                // 有庫存（Stock > 0 且未刪除）的規格最低價；回退至 OriginalPrice
                var minAvailablePrice = pi.Product?.ProductVariants
                    .Where(v => v.IsDeleted != true && (v.Stock ?? 0) > 0)
                    .Min(v => (decimal?)v.Price) ?? pi.OriginalPrice;

                var discountPrice = pi.DiscountPercent != null && pi.DiscountPercent > 0
                    ? (decimal?)Math.Round(minAvailablePrice * pi.DiscountPercent.Value / 100m, 0)
                    : pi.DiscountPrice;

                return new
                {
                    id              = pi.Promotion.Id,
                    title           = pi.Promotion.Name,
                    type            = PromotionService.GetTypeCode(pi.Promotion.PromotionType),
                    typeLabel       = PromotionService.GetTypeLabel(pi.Promotion.PromotionType),
                    endDate         = pi.Promotion.EndTime,
                    originalPrice   = minAvailablePrice,
                    discountPrice,
                    discountPercent = pi.DiscountPercent,
                    quantityLimit   = pi.QuantityLimit,
                    stockLimit      = pi.StockLimit,
                    soldCount       = pi.SoldCount,
                    remainingStock  = pi.StockLimit.HasValue ? (int?)(pi.StockLimit.Value - pi.SoldCount) : null,
                    linkUrl         = $"/promotion/{pi.Promotion.Id}",
                    rule            = rule != null ? new
                    {
                        ruleType      = rule.RuleType,
                        threshold     = rule.Threshold,
                        discountType  = rule.DiscountType,
                        discountValue = rule.DiscountValue
                    } : null
                };
            }).ToList();

            return Ok(new { success = true, data, message = "" });
        }
    }
}
