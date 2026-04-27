using ISpanShop.MVC.Models.Dto;
using ISpanShop.Services.Promotions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISpanShop.MVC.Controllers.Api.Promotions
{
    /// <summary>前台活動 API（唯讀，不需登入）</summary>
    [ApiController]
    [Route("api/promotions")]
    [Produces("application/json")]
    public class PromotionApiController : ControllerBase
    {
        private readonly PromotionService _promotionSvc;

        public PromotionApiController(PromotionService promotionSvc)
        {
            _promotionSvc = promotionSvc;
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
    }
}
