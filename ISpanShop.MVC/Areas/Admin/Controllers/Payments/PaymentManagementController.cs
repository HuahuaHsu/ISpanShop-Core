using ISpanShop.Models.DTOs.Orders;
using ISpanShop.Models.EfModels;
using ISpanShop.MVC.Areas.Admin.Models.Orders;
using ISpanShop.Services.Payments;
using ISpanShop.Services.Orders;
using Microsoft.AspNetCore.Mvc;
using ISpanShop.Repositories.Orders;
using Microsoft.EntityFrameworkCore;
using ISpanShop.MVC.Middleware;

namespace ISpanShop.MVC.Areas.Admin.Controllers.Payments
{
        [Area("Admin")]
        [RequirePermission("cashflow_manage")]
        public class PaymentManagementController : Controller
        {		private readonly CheckoutService _checkoutService;
		// 1. 新增這行：宣告資料庫上下文
		private readonly ISpanShopDBContext _context;

		// 2. 修改建構函式：把 ISpanShopDBContext 加進去
		public PaymentManagementController(CheckoutService checkoutService, ISpanShopDBContext context)
		{
			_checkoutService = checkoutService;
			_context = context; // 賦值給私有變數，這樣 Index() 才能用它
		}

		public async Task<IActionResult> Index()
		{
			// 1. 去資料庫抓資料，並包含 PaymentLogs 關聯資料
			var orders = await _context.Orders
				.Include(o => o.PaymentLogs) // 包含金流紀錄
				.OrderByDescending(o => o.CreatedAt) // 讓新訂單排在上面
				.ToListAsync();

			// 2. 傳給 View (如果 orders 是 null 就給新列表)
			return View(orders ?? new List<Order>());
		}

		// --- 以下是你原本的 Create 方法，保持不變 ---
		public IActionResult Create() => View();

		[HttpPost]
		public async Task<IActionResult> Create(OrderCreateVM vm)
		{
			// ... 原本的邏輯
			var dto = new CheckoutRequestDTO { /* ... */ };
			var result = await _checkoutService.CreateOrderAsync(dto);
			// ...
			return View(vm);
		}
	}
}