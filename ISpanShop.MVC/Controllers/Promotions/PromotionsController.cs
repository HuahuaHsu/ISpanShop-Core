using ISpanShop.MVC.Models.Promotions;
using ISpanShop.Models.EfModels;
using ISpanShop.Services.Promotions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.MVC.Controllers.Promotions
{
    [Route("Promotions")]
    public class PromotionsController : Controller
    {
        private readonly ISpanShopDBContext _context;
        private readonly PromotionService _promotionService;

        public PromotionsController(ISpanShopDBContext context, PromotionService promotionService)
        {
            _context = context;
            _promotionService = promotionService;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string? keyword, string? status, int? type, int page = 1, int pageSize = 10)
        {
            var now = DateTime.Now;
            var query = _context.Promotions
                .Include(p => p.Seller).ThenInclude(u => u.Stores)
                .Include(p => p.PromotionItems).Include(p => p.PromotionRules)
                .Where(p => !p.IsDeleted).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(p => p.Name.Contains(keyword) || p.Seller.Account.Contains(keyword) || p.Seller.Stores.Any(s => s.StoreName.Contains(keyword)));
            if (type.HasValue)
                query = query.Where(p => p.PromotionType == type.Value);

            query = status switch {
                "pending" => query.Where(p => p.Status == 0),
                "active" => query.Where(p => p.Status == 1 && p.StartTime <= now && p.EndTime >= now),
                "upcoming" => query.Where(p => p.Status == 1 && p.StartTime > now),
                "rejected" => query.Where(p => p.Status == 2),
                "ended" => query.Where(p => p.Status == 1 && p.EndTime < now),
                _ => query
            };

            var totalCount = await query.CountAsync();
            var allPromotions = await _context.Promotions.Where(p => !p.IsDeleted).ToListAsync();
            var promotions = await query.OrderByDescending(p => p.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var vm = new PromotionIndexVm {
                Keyword = keyword, StatusFilter = status, TypeFilter = type,
                TotalCount = totalCount, CurrentPage = page, PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                AllCount = allPromotions.Count,
                PendingCount = allPromotions.Count(p => p.Status == 0),
                ActiveCount = allPromotions.Count(p => p.Status == 1 && p.StartTime <= now && p.EndTime >= now),
                UpcomingCount = allPromotions.Count(p => p.Status == 1 && p.StartTime > now),
                EndedCount = allPromotions.Count(p => p.Status == 1 && p.EndTime < now),
                ReSubmittedCount = 0,
                RejectedCount = allPromotions.Count(p => p.Status == 2),
                Items = promotions.Select(p => new PromotionListItemVm {
                    Id = p.Id, Name = p.Name, PromotionType = p.PromotionType,
                    StartTime = p.StartTime, EndTime = p.EndTime, Status = p.Status,
                    SellerName = p.Seller.Stores.FirstOrDefault(s => s.StoreStatus == 1)?.StoreName ?? p.Seller.Account,
                    ItemCount = p.PromotionItems.Count + p.PromotionRules.Count
                }).ToList()
            };
            return View(vm);
        }

        [HttpGet("Detail/{id}")]
        public async Task<IActionResult> Detail(int id) {
            var p = await _context.Promotions.Include(x => x.Seller).ThenInclude(u => u.Stores)
                .Include(x => x.PromotionItems).Include(x => x.PromotionRules)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (p == null) { TempData["Error"] = "找不到此活動"; return RedirectToAction(nameof(Index)); }
            return View(p);
        }

        [HttpGet("Create")]
        public IActionResult Create() { TempData["Warning"] = "新增功能開發中"; return RedirectToAction(nameof(Index)); }
        [HttpGet("Edit/{id}")]
        public IActionResult Edit(int id) { TempData["Warning"] = "編輯功能開發中"; return RedirectToAction(nameof(Index)); }

        [HttpPost("Approve/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var p = await _context.Promotions.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (p == null)
                return Json(new { success = false, message = "找不到此活動" });
            if (p.Status != 0 && p.Status != 4)
                return Json(new { success = false, message = "只能核准待審核的活動" });

            var adminId = int.TryParse(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var uid)
                ? uid : (int?)null;

            p.Status     = 1;
            p.ReviewedAt = DateTime.Now;
            p.ReviewedBy = adminId;
            p.RejectReason = null;

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"活動「{p.Name}」已核准" });
        }

        [HttpPost("Reject/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return Json(new { success = false, message = "請填寫拒絕原因" });

            var p = await _context.Promotions.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (p == null)
                return Json(new { success = false, message = "找不到此活動" });
            if (p.Status != 0 && p.Status != 4)
                return Json(new { success = false, message = "只能拒絕待審核的活動" });

            var adminId = int.TryParse(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var uid)
                ? uid : (int?)null;

            p.Status       = 2;
            p.ReviewedAt   = DateTime.Now;
            p.ReviewedBy   = adminId;
            p.RejectReason = reason.Trim();

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"活動「{p.Name}」已拒絕" });
        }

        [HttpPost("SimulateSellerResubmit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SimulateSellerResubmit(int id)
        {
            var p = await _context.Promotions.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (p == null)
                return Json(new { success = false, message = "找不到此活動" });
            if (p.Status != 2)
                return Json(new { success = false, message = "只能對已拒絕的活動執行重新送審" });

            p.Status       = 0;
            p.ReviewedAt   = null;
            p.ReviewedBy   = null;
            p.RejectReason = null;

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"活動「{p.Name}」已重新送審，進入待審核列表" });
        }

        [HttpPost("Delete/{id}")]
        public IActionResult Delete(int id) { TempData["Warning"] = "刪除功能開發中"; return RedirectToAction(nameof(Index)); }

        [HttpGet("GetPromotionDetailsPartial")]
        public async Task<IActionResult> GetPromotionDetailsPartial(int id, bool isReviewMode = false)
        {
            var p = await _context.Promotions
                .Include(x => x.Seller).ThenInclude(u => u.Stores)
                .Include(x => x.PromotionItems).ThenInclude(i => i.Product)
                .Include(x => x.PromotionRules)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (p == null) return NotFound("找不到此活動");

            var sellerStatuses = await _context.Promotions
                .Where(x => x.SellerId == p.SellerId && !x.IsDeleted)
                .Select(x => x.Status)
                .ToListAsync();

            var totalPromos   = sellerStatuses.Count;
            var approvedCount = sellerStatuses.Count(s => s == 1);
            var approvalRate  = totalPromos > 0 ? (int)Math.Round((double)approvedCount / totalPromos * 100) : 0;

            var storeIds = p.Seller.Stores.Select(s => s.Id).ToList();
            double sellerRating = 4.0;
            if (storeIds.Any())
            {
                var avg = await _context.OrderReviews
                    .Where(r => storeIds.Contains(r.Order.StoreId))
                    .AverageAsync(r => (double?)r.Rating);
                if (avg.HasValue) sellerRating = Math.Round(avg.Value, 1);
            }

            var sellerName = p.Seller.Stores.FirstOrDefault(s => s.StoreStatus == 1)?.StoreName
                             ?? p.Seller.Account;

            var vm = new ISpanShop.MVC.Models.Promotions.PromotionDetailVm
            {
                Id            = p.Id,
                Name          = p.Name,
                Description   = p.Description,
                PromotionType = p.PromotionType,
                StartTime     = p.StartTime,
                EndTime       = p.EndTime,
                Status        = p.Status,
                SellerName    = sellerName,
                RejectReason  = p.RejectReason,
                ReviewedAt    = p.ReviewedAt,
                CreatedAt     = p.CreatedAt,

                SellerRating       = sellerRating,
                SellerApprovalRate = approvalRate,
                SellerViolations   = 0,
                SellerTotalPromos  = totalPromos,

                Items = p.PromotionItems.Select(i => new ISpanShop.MVC.Models.Promotions.PromotionItemDetailVm
                {
                    ProductId     = i.ProductId,
                    ProductName   = i.Product?.Name ?? $"商品 #{i.ProductId}",
                    OriginalPrice = i.OriginalPrice,
                    DiscountPrice = i.DiscountPrice,
                    QuantityLimit = i.QuantityLimit,
                    StockLimit    = i.StockLimit,
                    SoldCount     = i.SoldCount
                }).ToList(),

                Rules = p.PromotionRules.Select(r => new ISpanShop.MVC.Models.Promotions.PromotionRuleDetailVm
                {
                    RuleType      = r.RuleType,
                    Threshold     = r.Threshold,
                    DiscountType  = r.DiscountType,
                    DiscountValue = r.DiscountValue
                }).ToList()
            };

            ViewBag.IsReviewMode = isReviewMode;
            return PartialView("_PromotionDetailsPartial", vm);
        }

        [HttpGet("SearchProducts")]
        public IActionResult SearchProducts(string? keyword, int page = 1, int pageSize = 5) {
            var query = _context.Products.Include(p => p.Category).Include(p => p.ProductImages)
                .Where(p => !p.IsDeleted && p.Status == 1).AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword)) query = query.Where(p => p.Name.Contains(keyword));
            int totalCount = query.Count(), totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).Select(p => new {
                p.Id, p.Name, Price = p.MinPrice ?? 0,
                CategoryName = p.Category != null ? p.Category.Name : "未分類",
                ImageUrl = p.ProductImages.FirstOrDefault(img => img.IsMain == true) != null ? 
                    p.ProductImages.FirstOrDefault(img => img.IsMain == true).ImageUrl : "https://via.placeholder.com/60"
            }).ToList();
            return Json(new { items, totalCount, totalPages, currentPage = page });
        }
    }
}
