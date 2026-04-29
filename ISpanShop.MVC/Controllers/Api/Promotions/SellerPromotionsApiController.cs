using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ISpanShop.Models.DTOs.Promotions;
using ISpanShop.Models.EfModels;
using ISpanShop.Services.Promotions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.MVC.Controllers.Api.Promotions
{
    /// <summary>賣家促銷活動管理 API</summary>
    [ApiController]
    [Route("api/seller/promotions")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = "FrontendJwt")]
    public class SellerPromotionsApiController : ControllerBase
    {
        private readonly PromotionService _promotionService;
        private readonly ISpanShopDBContext _db;

        public SellerPromotionsApiController(PromotionService promotionService, ISpanShopDBContext db)
        {
            _promotionService = promotionService;
            _db = db;
        }

        /// <summary>
        /// 根據活動類型與前端傳入的折扣值建立 PromotionRule 清單。
        /// Type1（限時特賣）→ DiscountType=2（百分比）；Type2（滿額折扣）→ DiscountType=1（金額）+ Threshold；
        /// Type3（限量搶購）→ DiscountType=1（金額）。
        /// </summary>
        private static List<PromotionRule> BuildRules(int promotionType, decimal? discountValue, decimal? minimumAmount)
        {
            var rules = new List<PromotionRule>();
            if (discountValue is null or <= 0) return rules;

            rules.Add(new PromotionRule
            {
                RuleType      = 1,
                Threshold     = (promotionType == 2 ? minimumAmount : null) ?? 0,
                DiscountType  = promotionType == 1 ? 2 : 1,   // 1=限時特賣 → 百分比(2)；其他 → 金額(1)
                DiscountValue = discountValue.Value
            });
            return rules;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("無法識別使用者身份");
            }
            return userId;
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/seller/promotions
        // 取得賣家自己的活動列表
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// 取得賣家活動列表（分頁）
        /// </summary>
        /// <param name="status">all（全部）、pending（待審核）、active（進行中）、upcoming（即將開始）、rejected（已拒絕）、ended（已結束）</param>
        /// <param name="page">頁碼（預設 1）</param>
        /// <param name="pageSize">每頁筆數（預設 10，上限 50）</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPromotions(
            [FromQuery] string? status = "all",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var sellerId = GetCurrentUserId();

                var statusFilter = status?.ToLowerInvariant() == "all" ? null : status;

                var (items, totalCount) = await _promotionService.GetSellerPromotionsAsync(
                    sellerId, statusFilter, page, pageSize);

                var dtoItems = items.Select(p =>
                {
                    var rule = p.PromotionRules?.FirstOrDefault();
                    return new SellerPromotionListDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        PromotionType = p.PromotionType,
                        PromotionTypeLabel = PromotionService.GetTypeLabel(p.PromotionType),
                        DiscountValue = rule?.DiscountValue,
                        MinimumAmount = rule?.Threshold,
                        ProductCount = p.PromotionItems?.Count ?? 0,
                        StartTime = p.StartTime,
                        EndTime = p.EndTime,
                        Status = p.Status,
                        StatusText = PromotionService.GetStatusText(p.Status, p.StartTime, p.EndTime),
                        RejectReason = p.RejectReason,
                        CreatedAt = p.CreatedAt,
                        ReviewedAt = p.ReviewedAt
                    };
                }).ToList();

                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        items = dtoItems,
                        page,
                        pageSize,
                        totalCount,
                        totalPages
                    }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
        }

        // ──────────────────────────────────────────────────────────
        // POST /api/seller/promotions
        // 新增活動
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// 新增促銷活動
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreatePromotion([FromBody] CreatePromotionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "資料驗證失敗", errors = ModelState });
            }

            try
            {
                var sellerId = GetCurrentUserId();

                if (dto.EndTime <= dto.StartTime)
                {
                    return BadRequest(new { success = false, message = "結束時間必須晚於開始時間" });
                }

                var promotion = new Promotion
                {
                    SellerId = sellerId,
                    Name = dto.Name,
                    Description = dto.Description ?? string.Empty,
                    PromotionType = dto.PromotionType,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    PromotionRules = BuildRules(dto.PromotionType, dto.DiscountValue, dto.MinimumAmount)
                };

                var created = await _promotionService.CreatePromotionAsync(promotion);

                return CreatedAtAction(nameof(GetPromotion), new { id = created.Id }, new
                {
                    success = true,
                    message = "活動建立成功，待審核中",
                    data = new { id = created.Id }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/seller/promotions/{id}
        // 取得單一活動詳情
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// 取得單一活動詳情（需驗證是否為該賣家的活動）
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetPromotion(int id)
        {
            try
            {
                var sellerId = GetCurrentUserId();
                var promotion = await _promotionService.GetPromotionByIdAsync(id);

                if (promotion == null)
                {
                    return NotFound(new { success = false, message = "找不到此活動" });
                }

                if (promotion.SellerId != sellerId)
                {
                    return StatusCode(403, new { success = false, message = "無權存取此活動" });
                }

                var detailRule = promotion.PromotionRules?.FirstOrDefault();
                return Ok(new
                {
                    success = true,
                    data = new SellerPromotionListDto
                    {
                        Id = promotion.Id,
                        Name = promotion.Name,
                        Description = promotion.Description,
                        PromotionType = promotion.PromotionType,
                        PromotionTypeLabel = PromotionService.GetTypeLabel(promotion.PromotionType),
                        DiscountValue = detailRule?.DiscountValue,
                        MinimumAmount = detailRule?.Threshold,
                        StartTime = promotion.StartTime,
                        EndTime = promotion.EndTime,
                        Status = promotion.Status,
                        StatusText = PromotionService.GetStatusText(promotion.Status, promotion.StartTime, promotion.EndTime),
                        RejectReason = promotion.RejectReason,
                        CreatedAt = promotion.CreatedAt,
                        ReviewedAt = promotion.ReviewedAt
                    }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
        }

        // ──────────────────────────────────────────────────────────
        // PUT /api/seller/promotions/{id}
        // 編輯活動（已拒絕：完整編輯；即將開始：僅限名稱和描述）
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// 編輯促銷活動（已拒絕：完整編輯並重新送審；即將開始：僅更新名稱與描述）
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdatePromotion(int id, [FromBody] UpdatePromotionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "資料驗證失敗", errors = ModelState });
            }

            try
            {
                var sellerId = GetCurrentUserId();
                var promotion = await _promotionService.GetPromotionByIdAsync(id);

                if (promotion == null)
                {
                    return NotFound(new { success = false, message = "找不到此活動" });
                }

                if (promotion.SellerId != sellerId)
                {
                    return StatusCode(403, new { success = false, message = "無權編輯此活動" });
                }

                var now = DateTime.Now;
                bool isRejected = promotion.Status == 2;
                bool isUpcoming = promotion.Status == 1 && promotion.StartTime > now;

                if (!isRejected && !isUpcoming)
                {
                    return BadRequest(new { success = false, message = "只能編輯已拒絕或即將開始的活動" });
                }

                if (isUpcoming)
                {
                    // 即將開始：僅允許更新描述，保持 status=1 不重新送審
                    // 名稱、時間、類型、折扣一律不可修改（保障買家期待）
                    promotion.Description = dto.Description ?? string.Empty;
                    await _promotionService.UpdatePromotionAsync(promotion);
                    return Ok(new { success = true, message = "活動描述已更新" });
                }

                // 已拒絕：完整編輯，重新送審（status → 0）
                if (dto.EndTime <= dto.StartTime)
                {
                    return BadRequest(new { success = false, message = "結束時間必須晚於開始時間" });
                }

                promotion.Name = dto.Name;
                promotion.Description = dto.Description ?? string.Empty;
                promotion.PromotionType = dto.PromotionType;
                promotion.StartTime = dto.StartTime;
                promotion.EndTime = dto.EndTime;
                promotion.Status = 0;
                promotion.RejectReason = null;

                await _promotionService.UpdatePromotionAsync(promotion);

                // 替換折扣規則：先刪除舊規則，再寫入新規則
                var oldRules = await _db.PromotionRules.Where(r => r.PromotionId == id).ToListAsync();
                _db.PromotionRules.RemoveRange(oldRules);
                var newRules = BuildRules(dto.PromotionType, dto.DiscountValue, dto.MinimumAmount);
                foreach (var rule in newRules)
                {
                    rule.PromotionId = id;
                    _db.PromotionRules.Add(rule);
                }
                await _db.SaveChangesAsync();

                return Ok(new { success = true, message = "活動更新成功，重新送審中" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ──────────────────────────────────────────────────────────
        // DELETE /api/seller/promotions/{id}
        // 刪除活動（已拒絕、即將開始、已結束可刪除；進行中、待審核不可刪除）
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// 刪除促銷活動（已拒絕、即將開始、已結束可刪除）
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeletePromotion(int id)
        {
            try
            {
                var sellerId = GetCurrentUserId();
                var promotion = await _promotionService.GetPromotionByIdAsync(id);

                if (promotion == null)
                {
                    return NotFound(new { success = false, message = "找不到此活動" });
                }

                if (promotion.SellerId != sellerId)
                {
                    return StatusCode(403, new { success = false, message = "無權刪除此活動" });
                }

                var now = DateTime.Now;
                bool isRejected = promotion.Status == 2;
                bool isForceEnded = promotion.Status == 3;
                bool isUpcoming = promotion.Status == 1 && promotion.StartTime > now;
                bool isExpired = promotion.Status == 1 && promotion.EndTime <= now;

                if (!isRejected && !isForceEnded && !isUpcoming && !isExpired)
                {
                    return BadRequest(new { success = false, message = "待審核的活動請使用「撤銷送審」，進行中的活動請先提早結束後再刪除" });
                }

                await _promotionService.DeletePromotionAsync(id);

                return Ok(new { success = true, message = "活動已刪除" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ──────────────────────────────────────────────────────────
        // DELETE /api/seller/promotions/{id}/cancel-review
        // 撤銷送審（待審核 → 刪除）
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// 撤銷待審核活動（等同刪除，僅允許 status=0 的活動）
        /// </summary>
        [HttpDelete("{id:int}/cancel-review")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CancelReview(int id)
        {
            try
            {
                var sellerId = GetCurrentUserId();
                var promotion = await _promotionService.GetPromotionByIdAsync(id);

                if (promotion == null)
                {
                    return NotFound(new { success = false, message = "找不到此活動" });
                }

                if (promotion.SellerId != sellerId)
                {
                    return StatusCode(403, new { success = false, message = "無權操作此活動" });
                }

                if (promotion.Status != 0)
                {
                    return BadRequest(new { success = false, message = "只能撤銷「待審核」的活動" });
                }

                await _promotionService.DeletePromotionAsync(id);

                return Ok(new { success = true, message = "已撤銷送審，活動已刪除" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ──────────────────────────────────────────────────────────
        // PUT /api/seller/promotions/{id}/end-early
        // 提早結束（進行中 → 已結束）
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// 提早結束進行中的活動（status=1 進行中 → status=3 已結束）
        /// </summary>
        [HttpPut("{id:int}/end-early")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> EndEarly(int id)
        {
            try
            {
                var sellerId = GetCurrentUserId();
                var promotion = await _promotionService.GetPromotionByIdAsync(id);

                if (promotion == null)
                {
                    return NotFound(new { success = false, message = "找不到此活動" });
                }

                if (promotion.SellerId != sellerId)
                {
                    return StatusCode(403, new { success = false, message = "無權操作此活動" });
                }

                var now = DateTime.Now;
                bool isActive = promotion.Status == 1 && promotion.StartTime <= now && promotion.EndTime > now;

                if (!isActive)
                {
                    return BadRequest(new { success = false, message = "只能提早結束「進行中」的活動" });
                }

                promotion.Status = 3;
                promotion.EndTime = now;
                await _promotionService.UpdatePromotionAsync(promotion);

                return Ok(new { success = true, message = "活動已提早結束" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
