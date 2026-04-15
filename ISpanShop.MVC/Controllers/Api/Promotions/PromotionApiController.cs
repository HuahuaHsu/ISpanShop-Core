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

            var items = promotions.Select(p => new PromotionListItemDto
            {
                Id             = p.Id,
                Title          = p.Name,
                Subtitle       = string.IsNullOrWhiteSpace(p.Description) ? null : p.Description,
                Type           = PromotionService.GetTypeCode(p.PromotionType),
                TypeLabel      = PromotionService.GetTypeLabel(p.PromotionType),
                BannerImageUrl = null,          // DB 無此欄位，待補
                LinkUrl        = $"/promotion/{p.Id}",
                StartDate      = p.StartTime,
                EndDate        = p.EndTime
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
