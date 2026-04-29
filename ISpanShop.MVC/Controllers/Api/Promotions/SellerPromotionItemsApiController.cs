using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ISpanShop.Models.EfModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.MVC.Controllers.Api.Promotions
{
    /// <summary>賣家促銷活動商品綁定 API</summary>
    [ApiController]
    [Route("api/seller/promotions/{promotionId:int}")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = "FrontendJwt")]
    public class SellerPromotionItemsApiController : ControllerBase
    {
        private readonly ISpanShopDBContext _db;

        public SellerPromotionItemsApiController(ISpanShopDBContext db) => _db = db;

        // ── 取 JWT 中的 userId / storeId ──────────────────────────
        private int? GetUserId()
            => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var v) ? v : null;

        private int? GetStoreId()
            => int.TryParse(User.FindFirst("StoreId")?.Value, out var v) ? v : null;

        // ── 驗證活動屬於當前賣家並回傳 entity（null = 失敗）───────
        private async Task<(Promotion? Promo, IActionResult? Error)> ResolvePromoAsync(int promotionId, int userId)
        {
            var promo = await _db.Promotions.FirstOrDefaultAsync(p => p.Id == promotionId && !p.IsDeleted);
            if (promo == null)
                return (null, NotFound(new { success = false, message = "找不到此活動" }));
            if (promo.SellerId != userId)
                return (null, StatusCode(403, new { success = false, message = "無權存取此活動" }));
            return (promo, null);
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/seller/promotions/{promotionId}/products
        // 取得活動已綁定的商品列表
        // ──────────────────────────────────────────────────────────
        [HttpGet("products")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPromotionProducts(int promotionId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new { success = false, message = "無法識別使用者身份" });

            var (_, err) = await ResolvePromoAsync(promotionId, userId.Value);
            if (err != null) return err;

            var items = await _db.PromotionItems
                .Where(i => i.PromotionId == promotionId)
                .Include(i => i.Product).ThenInclude(p => p.ProductImages)
                .Include(i => i.Product).ThenInclude(p => p.ProductVariants)
                .Select(i => new
                {
                    productId     = i.ProductId,
                    productName   = i.Product.Name,
                    imageUrl      = i.Product.ProductImages
                                        .Where(img => img.IsMain == true)
                                        .Select(img => img.ImageUrl)
                                        .FirstOrDefault()
                                    ?? i.Product.ProductImages
                                        .OrderBy(img => img.SortOrder)
                                        .Select(img => img.ImageUrl)
                                        .FirstOrDefault(),
                    minPrice      = i.Product.MinPrice,
                    originalPrice = i.OriginalPrice,
                    discountPrice = i.DiscountPrice,
                    totalStock    = (int?)i.Product.ProductVariants
                                        .Where(v => v.IsDeleted != true)
                                        .Sum(v => v.Stock),
                    quantityLimit = i.QuantityLimit,
                    stockLimit    = i.StockLimit,
                    soldCount     = i.SoldCount
                })
                .ToListAsync();

            return Ok(new { success = true, data = items });
        }

        // ──────────────────────────────────────────────────────────
        // POST /api/seller/promotions/{promotionId}/products
        // 為活動新增商品（批次）
        // Request body: { "productIds": [1, 2, 3] }
        // ──────────────────────────────────────────────────────────
        [HttpPost("products")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddPromotionProducts(
            int promotionId,
            [FromBody] AddPromotionProductsRequest request)
        {
            var userId  = GetUserId();
            var storeId = GetStoreId();
            if (userId == null)  return Unauthorized(new { success = false, message = "無法識別使用者身份" });
            if (storeId == null) return Unauthorized(new { success = false, message = "無法識別賣家店家，請確認已開通店家" });

            if (request?.ProductIds == null || !request.ProductIds.Any())
                return BadRequest(new { success = false, message = "請提供至少一個商品 ID" });

            var (_, err) = await ResolvePromoAsync(promotionId, userId.Value);
            if (err != null) return err;

            // 只接受屬於本店家、且未刪除的商品
            var validProducts = await _db.Products
                .Where(p => request.ProductIds.Contains(p.Id)
                         && p.StoreId == storeId.Value
                         && !p.IsDeleted)
                .Select(p => new { p.Id, p.MinPrice })
                .ToListAsync();

            if (!validProducts.Any())
                return BadRequest(new { success = false, message = "沒有可新增的有效商品（商品不存在或不屬於您的店家）" });

            // 排除已綁定的商品
            var alreadyBound = await _db.PromotionItems
                .Where(i => i.PromotionId == promotionId)
                .Select(i => i.ProductId)
                .ToListAsync();

            var toAdd = validProducts.Where(p => !alreadyBound.Contains(p.Id)).ToList();

            // 載入活動的折扣規則，用來計算活動價
            var rule = await _db.PromotionRules
                .Where(r => r.PromotionId == promotionId)
                .FirstOrDefaultAsync();

            foreach (var p in toAdd)
            {
                var originalPrice = p.MinPrice ?? 0;
                var (discountPrice, discountPercent) = CalcDiscount(originalPrice, rule);

                _db.PromotionItems.Add(new PromotionItem
                {
                    PromotionId     = promotionId,
                    ProductId       = p.Id,
                    OriginalPrice   = originalPrice,
                    DiscountPrice   = discountPrice,
                    DiscountPercent = discountPercent,
                    SoldCount       = 0
                });
            }

            await _db.SaveChangesAsync();

            var skipped = request.ProductIds.Count - toAdd.Count;
            var msg = $"成功加入 {toAdd.Count} 件商品"
                    + (skipped > 0 ? $"（{skipped} 件已在活動中或不屬於您的店家，略過）" : "");

            return Ok(new { success = true, message = msg, addedCount = toAdd.Count });
        }

        // ──────────────────────────────────────────────────────────
        // DELETE /api/seller/promotions/{promotionId}/products/{productId}
        // 從活動移除指定商品
        // ──────────────────────────────────────────────────────────
        [HttpDelete("products/{productId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemovePromotionProduct(int promotionId, int productId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new { success = false, message = "無法識別使用者身份" });

            var (promo, err) = await ResolvePromoAsync(promotionId, userId.Value);
            if (err != null) return err;

            // 即將開始的活動不能移除商品（保障已加購物車的買家）
            var now = DateTime.Now;
            if (promo!.Status == 1 && promo.StartTime > now)
            {
                return BadRequest(new { success = false, message = "即將開始的活動不能移除商品，以保障已加購物車的買家權益" });
            }

            var item = await _db.PromotionItems
                .FirstOrDefaultAsync(i => i.PromotionId == promotionId && i.ProductId == productId);

            if (item == null)
                return NotFound(new { success = false, message = "此商品不在活動中" });

            _db.PromotionItems.Remove(item);
            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = "商品已從活動中移除" });
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/seller/promotions/{promotionId}/available-products
        // 取得賣家可加入此活動的商品（已上架 + 尚未綁定）
        // 支援關鍵字搜尋、分頁
        // ──────────────────────────────────────────────────────────
        [HttpGet("available-products")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAvailableProducts(
            int promotionId,
            [FromQuery] string? keyword  = null,
            [FromQuery] int     page     = 1,
            [FromQuery] int     pageSize = 20)
        {
            var userId  = GetUserId();
            var storeId = GetStoreId();
            if (userId == null)  return Unauthorized(new { success = false, message = "無法識別使用者身份" });
            if (storeId == null) return Unauthorized(new { success = false, message = "無法識別賣家店家" });

            var (_, err) = await ResolvePromoAsync(promotionId, userId.Value);
            if (err != null) return err;

            pageSize = Math.Clamp(pageSize, 1, 100);
            page     = Math.Max(1, page);

            // 已綁定的商品 ID
            var boundIds = await _db.PromotionItems
                .Where(i => i.PromotionId == promotionId)
                .Select(i => i.ProductId)
                .ToListAsync();

            var query = _db.Products
                .Where(p => p.StoreId == storeId.Value
                         && !p.IsDeleted
                         && !boundIds.Contains(p.Id));

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(p => p.Name.Contains(keyword));

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    id       = p.Id,
                    name     = p.Name,
                    imageUrl = p.ProductImages
                                   .Where(img => img.IsMain == true)
                                   .Select(img => img.ImageUrl)
                                   .FirstOrDefault()
                               ?? p.ProductImages
                                   .OrderBy(img => img.SortOrder)
                                   .Select(img => img.ImageUrl)
                                   .FirstOrDefault(),
                    minPrice   = p.MinPrice,
                    maxPrice   = p.MaxPrice,
                    totalStock = (int?)p.ProductVariants
                                     .Where(v => v.IsDeleted != true)
                                     .Sum(v => v.Stock),
                    status   = p.Status
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = new
                {
                    items      = products,
                    page,
                    pageSize,
                    totalCount,
                    totalPages
                }
            });
        }

        // ──────────────────────────────────────────────────────────
        // POST /api/seller/promotions/{promotionId}/fix-prices
        // 修復現有 PromotionItems 的活動價（DiscountPrice == null 或 0）
        // ──────────────────────────────────────────────────────────

        [HttpPost("fix-prices")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> FixPromotionItemPrices(int promotionId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new { success = false, message = "無法識別使用者身份" });

            var (_, err) = await ResolvePromoAsync(promotionId, userId.Value);
            if (err != null) return err;

            var rule = await _db.PromotionRules
                .Where(r => r.PromotionId == promotionId)
                .FirstOrDefaultAsync();

            var items = await _db.PromotionItems
                .Include(pi => pi.Product)
                .Where(pi => pi.PromotionId == promotionId
                          && (pi.DiscountPrice == null || pi.DiscountPrice == 0))
                .ToListAsync();

            foreach (var item in items)
            {
                if (item.OriginalPrice <= 0)
                    item.OriginalPrice = item.Product?.MinPrice ?? 0;

                var (dp, pct) = CalcDiscount(item.OriginalPrice, rule);
                item.DiscountPrice   = dp;
                item.DiscountPercent = pct;
            }

            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = $"已修復 {items.Count} 筆活動商品價格" });
        }

        // ── 折扣計算：根據活動規則算出活動價和折扣% ──────────────
        private static (decimal? discountPrice, int? discountPercent) CalcDiscount(
            decimal originalPrice, PromotionRule? rule)
        {
            if (rule == null || rule.DiscountValue <= 0 || originalPrice <= 0)
                return (null, null);

            if (rule.DiscountType == 2)
            {
                // 百分比折扣：DiscountValue 是折扣數（例如 20 表示打八折，扣掉 20%）
                var discountPrice = Math.Round(originalPrice * (100 - rule.DiscountValue) / 100, 0);
                var discountPercent = (int)(100 - rule.DiscountValue);
                return (discountPrice, discountPercent);
            }
            else
            {
                // 固定金額折扣：DiscountValue 是減去的金額
                var discountPrice = Math.Max(0, originalPrice - rule.DiscountValue);
                var discountPercent = originalPrice > 0
                    ? (int)Math.Round(discountPrice / originalPrice * 100)
                    : 0;
                return (discountPrice, discountPercent);
            }
        }
    }

    /// <summary>批次新增活動商品的請求 body</summary>
    public class AddPromotionProductsRequest
    {
        public List<int> ProductIds { get; set; } = new();
    }
}
