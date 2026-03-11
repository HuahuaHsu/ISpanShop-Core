using ISpanShop.MVC.Areas.Admin.Models.Promotions;
using Microsoft.AspNetCore.Mvc;

namespace ISpanShop.MVC.Areas.Admin.Controllers.Promotions
{
    [Area("Admin")]
    [Route("Admin/Promotions")]
    public class PromotionsController : Controller
    {
        // ===================================================================
        // Mock 資料（靜態，跨 request 持久）
        // ===================================================================
        private static int _nextId = 9;

        private static readonly List<MockSellerVm> _sellers = new()
        {
            new() { Id = 1, Name = "Apple 台灣授權店" },
            new() { Id = 2, Name = "Samsung 官方旗艦店" },
            new() { Id = 3, Name = "數位精品館" },
            new() { Id = 4, Name = "家電特賣城" },
            new() { Id = 5, Name = "遊戲天堂" },
        };

        private static readonly List<MockProductVm> _products = new()
        {
            new() { Id = 1,  Name = "iPhone 15 Pro 128GB",       Price = 38900, CategoryName = "手機",     ImageUrl = "https://via.placeholder.com/60" },
            new() { Id = 2,  Name = "Samsung Galaxy S24",         Price = 32900, CategoryName = "手機",     ImageUrl = "https://via.placeholder.com/60" },
            new() { Id = 3,  Name = "AirPods Pro 第二代",          Price = 7490,  CategoryName = "耳機",     ImageUrl = "https://via.placeholder.com/60" },
            new() { Id = 4,  Name = "MacBook Air M2",             Price = 37900, CategoryName = "筆電",     ImageUrl = "https://via.placeholder.com/60" },
            new() { Id = 5,  Name = "Sony WH-1000XM5",           Price = 10900, CategoryName = "耳機",     ImageUrl = "https://via.placeholder.com/60" },
            new() { Id = 6,  Name = "Apple Watch Series 9",      Price = 12900, CategoryName = "智慧手錶", ImageUrl = "https://via.placeholder.com/60" },
            new() { Id = 7,  Name = "Nintendo Switch OLED",      Price = 10980, CategoryName = "遊戲主機", ImageUrl = "https://via.placeholder.com/60" },
            new() { Id = 8,  Name = "Dyson V15 吸塵器",            Price = 23900, CategoryName = "家電",     ImageUrl = "https://via.placeholder.com/60" },
            new() { Id = 9,  Name = "LEGO 星際大戰套組",            Price = 4990,  CategoryName = "玩具",     ImageUrl = "https://via.placeholder.com/60" },
            new() { Id = 10, Name = "Bose QuietComfort 45",      Price = 9990,  CategoryName = "耳機",     ImageUrl = "https://via.placeholder.com/60" },
        };

        private static readonly List<PromotionStoredVm> _store = new()
        {
            new() {
                Id = 1, Name = "iPhone 15 週年特賣", PromotionType = 1,
                StartTime = DateTime.Now.AddDays(2), EndTime = DateTime.Now.AddDays(9),
                Status = 0, SellerId = 1, SellerName = "Apple 台灣授權店",
                CreatedAt = DateTime.Now.AddDays(-1),
                Items = new() { new() { ProductId = 1, ProductName = "iPhone 15 Pro 128GB", OriginalPrice = 38900, DiscountPrice = 36900 } }
            },
            new() {
                Id = 2, Name = "耳機音響優惠節", PromotionType = 1,
                StartTime = DateTime.Now.AddDays(-2), EndTime = DateTime.Now.AddDays(5),
                Status = 1, SellerId = 3, SellerName = "數位精品館",
                CreatedAt = DateTime.Now.AddDays(-5),
                Items = new()
                {
                    new() { ProductId = 3, ProductName = "AirPods Pro 第二代", OriginalPrice = 7490, DiscountPrice = 6490, SoldCount = 25 },
                    new() { ProductId = 5, ProductName = "Sony WH-1000XM5",   OriginalPrice = 10900, DiscountPrice = 9500, SoldCount = 18 }
                }
            },
            new() {
                Id = 3, Name = "夏季購物滿額折扣", PromotionType = 2,
                StartTime = DateTime.Now.AddDays(-3), EndTime = DateTime.Now.AddDays(4),
                Status = 1, SellerId = 4, SellerName = "家電特賣城",
                CreatedAt = DateTime.Now.AddDays(-7),
                Rules = new()
                {
                    new() { RuleType = 1, Threshold = 1000, DiscountType = 1, DiscountValue = 100 },
                    new() { RuleType = 1, Threshold = 2000, DiscountType = 1, DiscountValue = 250 },
                    new() { RuleType = 1, Threshold = 3000, DiscountType = 1, DiscountValue = 400 }
                }
            },
            new() {
                Id = 4, Name = "Switch OLED 限量搶購", PromotionType = 3,
                StartTime = DateTime.Now.AddDays(1), EndTime = DateTime.Now.AddDays(3),
                Status = 1, SellerId = 5, SellerName = "遊戲天堂",
                CreatedAt = DateTime.Now.AddDays(-2),
                Items = new() { new() { ProductId = 7, ProductName = "Nintendo Switch OLED", OriginalPrice = 10980, DiscountPrice = 9980, QuantityLimit = 2, StockLimit = 50 } }
            },
            new() {
                Id = 5, Name = "3C家電滿件優惠", PromotionType = 2,
                StartTime = DateTime.Now.AddDays(3), EndTime = DateTime.Now.AddDays(10),
                Status = 0, SellerId = 4, SellerName = "家電特賣城",
                CreatedAt = DateTime.Now.AddHours(-6),
                Rules = new() { new() { RuleType = 2, Threshold = 3, DiscountType = 2, DiscountValue = 10 } }
            },
            new() {
                Id = 6, Name = "低價傾銷活動（已拒絕）", PromotionType = 1,
                StartTime = DateTime.Now.AddDays(1), EndTime = DateTime.Now.AddDays(8),
                Status = 2, SellerId = 2, SellerName = "Samsung 官方旗艦店",
                CreatedAt = DateTime.Now.AddDays(-3), ReviewedAt = DateTime.Now.AddDays(-1),
                RejectReason = "活動折扣幅度超出規定範圍，請修正後重新申請",
                Items = new() { new() { ProductId = 2, ProductName = "Samsung Galaxy S24", OriginalPrice = 32900, DiscountPrice = 15000 } }
            },
            new() {
                Id = 7, Name = "春節連假大促銷", PromotionType = 1,
                StartTime = DateTime.Now.AddDays(-20), EndTime = DateTime.Now.AddDays(-10),
                Status = 3, SellerId = 1, SellerName = "Apple 台灣授權店",
                CreatedAt = DateTime.Now.AddDays(-25),
                Items = new() { new() { ProductId = 4, ProductName = "MacBook Air M2", OriginalPrice = 37900, DiscountPrice = 34900, SoldCount = 15 } }
            },
            new() {
                Id = 8, Name = "Apple Watch 新品上市優惠", PromotionType = 3,
                StartTime = DateTime.Now.AddDays(5), EndTime = DateTime.Now.AddDays(12),
                Status = 1, SellerId = 1, SellerName = "Apple 台灣授權店",
                CreatedAt = DateTime.Now.AddDays(-1),
                Items = new() { new() { ProductId = 6, ProductName = "Apple Watch Series 9", OriginalPrice = 12900, DiscountPrice = 11900, QuantityLimit = 3, StockLimit = 100 } }
            },
        };

        // ===================================================================
        // Index
        // ===================================================================
        [HttpGet]
        public IActionResult Index(string? keyword, string? status, int? type, int page = 1, int pageSize = 10)
        {
            var now = DateTime.Now;
            var all = _store.Where(p => !p.IsDeleted);

            var query = all;

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(p =>
                    p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    p.SellerName.Contains(keyword, StringComparison.OrdinalIgnoreCase));

            if (type.HasValue)
                query = query.Where(p => p.PromotionType == type.Value);

            query = status switch
            {
                "pending"  => query.Where(p => p.Status == 0),
                "active"   => query.Where(p => p.Status == 1 && p.StartTime <= now && p.EndTime >= now),
                "upcoming" => query.Where(p => p.Status == 1 && p.StartTime > now),
                "rejected" => query.Where(p => p.Status == 2),
                "ended"    => query.Where(p => p.Status == 3 || (p.Status == 1 && p.EndTime < now)),
                _          => query
            };

            var totalCount = query.Count();
            var vm = new PromotionIndexVm
            {
                Keyword      = keyword,
                StatusFilter = status,
                TypeFilter   = type,
                TotalCount   = totalCount,
                CurrentPage  = page,
                PageSize     = pageSize,
                TotalPages   = (int)Math.Ceiling(totalCount / (double)pageSize),

                PendingCount  = all.Count(p => p.Status == 0),
                ActiveCount   = all.Count(p => p.Status == 1 && p.StartTime <= now && p.EndTime >= now),
                UpcomingCount = all.Count(p => p.Status == 1 && p.StartTime > now),
                EndedCount    = all.Count(p => p.Status == 3 || (p.Status == 1 && p.EndTime < now)),

                Items = query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new PromotionListItemVm
                    {
                        Id            = p.Id,
                        Name          = p.Name,
                        PromotionType = p.PromotionType,
                        StartTime     = p.StartTime,
                        EndTime       = p.EndTime,
                        Status        = p.Status,
                        SellerName    = p.SellerName,
                        ItemCount     = p.Items.Count + p.Rules.Count
                    }).ToList()
            };

            return View("~/Areas/Admin/Views/Promotions/Index.cshtml", vm);
        }

        // ===================================================================
        // Create GET
        // ===================================================================
        [HttpGet("Create")]
        public IActionResult Create()
        {
            var vm = new PromotionFormVm
            {
                StartTime = DateTime.Now.AddDays(1).Date.AddHours(10),
                EndTime   = DateTime.Now.AddDays(8).Date.AddHours(23).AddMinutes(59),
                Sellers   = _sellers.ToList()
            };
            return View("~/Areas/Admin/Views/Promotions/Create.cshtml", vm);
        }

        // ===================================================================
        // Create POST
        // ===================================================================
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PromotionFormVm model)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (string.IsNullOrWhiteSpace(model.Name))
                return AjaxOrRedirect(isAjax, false, "活動名稱不可為空", null);

            if (model.EndTime <= model.StartTime)
                return AjaxOrRedirect(isAjax, false, "結束時間必須晚於開始時間", null);

            var seller = _sellers.FirstOrDefault(s => s.Id == model.SellerId);
            var newItem = new PromotionStoredVm
            {
                Id            = _nextId++,
                Name          = model.Name.Trim(),
                Description   = model.Description?.Trim(),
                PromotionType = model.PromotionType,
                StartTime     = model.StartTime,
                EndTime       = model.EndTime,
                Status        = 1,  // 管理員建立 → 直接核准
                SellerId      = model.SellerId,
                SellerName    = seller?.Name ?? "未知賣家",
                CreatedAt     = DateTime.Now,
                Items         = (model.Items ?? new()).Select(i => new PromotionItemDetailVm
                {
                    ProductId     = i.ProductId,
                    ProductName   = i.ProductName,
                    OriginalPrice = i.OriginalPrice,
                    DiscountPrice = i.DiscountPrice,
                    QuantityLimit = i.QuantityLimit,
                    StockLimit    = i.StockLimit
                }).ToList(),
                Rules = (model.Rules ?? new()).Select(r => new PromotionRuleDetailVm
                {
                    RuleType      = r.RuleType,
                    Threshold     = r.Threshold,
                    DiscountType  = r.DiscountType,
                    DiscountValue = r.DiscountValue
                }).ToList()
            };
            _store.Add(newItem);

            if (isAjax) return Json(new { success = true, message = "活動建立成功", id = newItem.Id });
            TempData["Success"] = $"活動「{newItem.Name}」已成功建立";
            return RedirectToAction(nameof(Index));
        }

        // ===================================================================
        // Edit GET
        // ===================================================================
        [HttpGet("Edit/{id}")]
        public IActionResult Edit(int id)
        {
            var promo = _store.FirstOrDefault(p => p.Id == id && !p.IsDeleted);
            if (promo == null) return NotFound();

            var now = DateTime.Now;
            bool canEdit     = promo.Status == 0 || (promo.Status == 1 && promo.StartTime > now);
            bool onlyEndTime = promo.Status == 1 && promo.StartTime <= now && promo.EndTime >= now;

            if (!canEdit && !onlyEndTime)
            {
                TempData["Error"] = "此活動目前狀態無法編輯";
                return RedirectToAction(nameof(Detail), new { id });
            }

            var vm = new PromotionFormVm
            {
                Id            = promo.Id,
                Name          = promo.Name,
                Description   = promo.Description,
                PromotionType = promo.PromotionType,
                StartTime     = promo.StartTime,
                EndTime       = promo.EndTime,
                SellerId      = promo.SellerId,
                Items         = promo.Items.Select(i => new PromotionItemFormVm
                {
                    ProductId     = i.ProductId,
                    ProductName   = i.ProductName,
                    OriginalPrice = i.OriginalPrice,
                    DiscountPrice = i.DiscountPrice,
                    QuantityLimit = i.QuantityLimit,
                    StockLimit    = i.StockLimit
                }).ToList(),
                Rules = promo.Rules.Select(r => new PromotionRuleFormVm
                {
                    RuleType      = r.RuleType,
                    Threshold     = r.Threshold,
                    DiscountType  = r.DiscountType,
                    DiscountValue = r.DiscountValue
                }).ToList(),
                Sellers = _sellers.ToList()
            };

            ViewBag.OnlyEndTime      = onlyEndTime && !canEdit;
            ViewBag.PromotionStatus  = promo.Status;
            ViewBag.RejectReason     = promo.RejectReason ?? "";
            return View("~/Areas/Admin/Views/Promotions/Edit.cshtml", vm);
        }

        // ===================================================================
        // Edit POST
        // ===================================================================
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, PromotionFormVm model)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            var promo = _store.FirstOrDefault(p => p.Id == id && !p.IsDeleted);
            if (promo == null)
                return AjaxOrRedirect(isAjax, false, "找不到此活動", null);

            var now     = DateTime.Now;
            bool onlyEndTime = promo.Status == 1 && promo.StartTime <= now && promo.EndTime >= now;

            if (model.EndTime <= (onlyEndTime ? promo.StartTime : model.StartTime))
                return AjaxOrRedirect(isAjax, false, "結束時間必須晚於開始時間", null);

            promo.EndTime = model.EndTime;

            if (!onlyEndTime)
            {
                promo.Name          = model.Name.Trim();
                promo.Description   = model.Description?.Trim();
                promo.PromotionType = model.PromotionType;
                promo.StartTime     = model.StartTime;
                promo.SellerId      = model.SellerId;
                promo.SellerName    = _sellers.FirstOrDefault(s => s.Id == model.SellerId)?.Name ?? promo.SellerName;
                promo.Items         = (model.Items ?? new()).Select(i => new PromotionItemDetailVm
                {
                    ProductId     = i.ProductId,
                    ProductName   = i.ProductName,
                    OriginalPrice = i.OriginalPrice,
                    DiscountPrice = i.DiscountPrice,
                    QuantityLimit = i.QuantityLimit,
                    StockLimit    = i.StockLimit
                }).ToList();
                promo.Rules = (model.Rules ?? new()).Select(r => new PromotionRuleDetailVm
                {
                    RuleType      = r.RuleType,
                    Threshold     = r.Threshold,
                    DiscountType  = r.DiscountType,
                    DiscountValue = r.DiscountValue
                }).ToList();
            }

            var successMsg = $"活動「{promo.Name}」已更新";
            TempData["Success"] = successMsg;
            if (isAjax) return Json(new { success = true, message = successMsg });
            return RedirectToAction(nameof(Index));
        }

        // ===================================================================
        // Detail GET
        // ===================================================================
        [HttpGet("Detail/{id}")]
        public IActionResult Detail(int id)
        {
            var promo = _store.FirstOrDefault(p => p.Id == id && !p.IsDeleted);
            if (promo == null) return NotFound();

            var vm = new PromotionDetailVm
            {
                Id            = promo.Id,
                Name          = promo.Name,
                Description   = promo.Description,
                PromotionType = promo.PromotionType,
                StartTime     = promo.StartTime,
                EndTime       = promo.EndTime,
                Status        = promo.Status,
                SellerName    = promo.SellerName,
                RejectReason  = promo.RejectReason,
                ReviewedAt    = promo.ReviewedAt,
                CreatedAt     = promo.CreatedAt,
                Items         = promo.Items,
                Rules         = promo.Rules
            };

            return View("~/Areas/Admin/Views/Promotions/Detail.cshtml", vm);
        }

        // ===================================================================
        // Approve POST (AJAX)
        // ===================================================================
        [HttpPost("Approve/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id)
        {
            var promo = _store.FirstOrDefault(p => p.Id == id && !p.IsDeleted);
            if (promo == null) return Json(new { success = false, message = "找不到活動" });
            if (promo.Status != 0) return Json(new { success = false, message = "只有待審核的活動才能核准" });

            promo.Status     = 1;
            promo.ReviewedAt = DateTime.Now;
            return Json(new { success = true, message = $"活動「{promo.Name}」已核准" });
        }

        // ===================================================================
        // Reject POST (AJAX)
        // ===================================================================
        [HttpPost("Reject/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int id, [FromForm] string reason)
        {
            var promo = _store.FirstOrDefault(p => p.Id == id && !p.IsDeleted);
            if (promo == null) return Json(new { success = false, message = "找不到活動" });
            if (promo.Status != 0) return Json(new { success = false, message = "只有待審核的活動才能拒絕" });
            if (string.IsNullOrWhiteSpace(reason)) return Json(new { success = false, message = "請填寫拒絕理由" });

            promo.Status       = 2;
            promo.RejectReason = reason.Trim();
            promo.ReviewedAt   = DateTime.Now;
            return Json(new { success = true, message = $"活動「{promo.Name}」已拒絕" });
        }

        // ===================================================================
        // Delete POST (AJAX / Form)
        // ===================================================================
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            var promo = _store.FirstOrDefault(p => p.Id == id && !p.IsDeleted);
            if (promo == null) return AjaxOrRedirect(isAjax, false, "找不到活動", null);

            var now = DateTime.Now;
            if (promo.Status == 1 && promo.StartTime <= now && promo.EndTime >= now)
                return AjaxOrRedirect(isAjax, false, "進行中的活動無法刪除", null);

            promo.IsDeleted = true;
            if (isAjax) return Json(new { success = true, message = "活動已刪除" });
            TempData["Success"] = "活動已刪除";
            return RedirectToAction(nameof(Index));
        }

        // ===================================================================
        // SearchProducts GET (AJAX — 商品選擇 Modal 用)
        // ===================================================================
        [HttpGet("SearchProducts")]
        public IActionResult SearchProducts(string? keyword)
        {
            var query = _products.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(p =>
                    p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    p.CategoryName.Contains(keyword, StringComparison.OrdinalIgnoreCase));

            return Json(query.Select(p => new { p.Id, p.Name, p.Price, p.CategoryName, p.ImageUrl }));
        }

        // ===================================================================
        // 輔助
        // ===================================================================
        private IActionResult AjaxOrRedirect(bool isAjax, bool success, string message, string? redirectUrl)
        {
            if (isAjax) return Json(new { success, message });
            if (success) TempData["Success"] = message; else TempData["Error"] = message;
            return Redirect(redirectUrl ?? Url.Action(nameof(Index))!);
        }
    }
}
