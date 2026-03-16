using ISpanShop.Models.DTOs.Orders;
using ISpanShop.Models.EfModels;
using ISpanShop.MVC.Areas.Admin.Models.Orders;
using ISpanShop.Services.Payments;
using ISpanShop.Services.Orders;
using Microsoft.AspNetCore.Mvc;
using ISpanShop.Repositories.Orders;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.MVC.Areas.Admin.Controllers.Payments
{
	[Area("Admin")]

	public class PaymentManagementController : Controller
	{
		private readonly CheckoutService _checkoutService;
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
			// 改為抓取 PaymentLogs，並包含 Order 資料
			var logs = await _context.PaymentLogs
				.Include(pl => pl.Order)
				.ThenInclude(o => o.User)
				.OrderByDescending(pl => pl.CreatedAt)
				.ToListAsync();

			return View(logs);
		}

		[HttpPost]
		public async Task<IActionResult> SyncPaymentLogs()
		{
			// 1. 找出所有目前「沒有」任何 PaymentLog 的訂單
			var ordersWithoutLog = await _context.Orders
				.Include(o => o.PaymentLogs)
				.Where(o => !o.PaymentLogs.Any())
				.ToListAsync();

			int createdCount = 0;
			var now = DateTime.Now;

			foreach (var order in ordersWithoutLog)
			{
				// 確保 MerchantTradeNo 不超過 20 字
				// 如果 OrderNumber 太長，就直接用 OrderNumber，不再加後綴
				string mTradeNo = order.OrderNumber;
				if (mTradeNo.Length > 20) mTradeNo = mTradeNo.Substring(0, 20);

				// 根據訂單狀態生成合理的金流紀錄
				var newLog = new PaymentLog
				{
					OrderId = order.Id,
					MerchantTradeNo = mTradeNo, 
					TradeAmt = order.FinalAmount,
					CreatedAt = order.CreatedAt ?? now
				};

				if (order.Status == 1) // 已付款
				{
					newLog.RtnCode = 1;
					newLog.RtnMsg = "付款成功";
					newLog.PaymentDate = order.PaymentDate ?? now;
					// 生成模擬的綠界交易序號
					newLog.TradeNo = "EC" + now.ToString("yyyyMMdd") + order.Id.ToString().PadLeft(8, '0');
					newLog.PaymentType = "Credit"; // 預設為信用卡
				}
				else if (order.Status == 4) // 已取消
				{
					newLog.RtnCode = 1022; // 模擬取消代碼
					newLog.RtnMsg = "訂單已取消";
					newLog.PaymentType = "None";
				}
				else // 待付款 (0)
				{
					newLog.RtnCode = 0;
					newLog.RtnMsg = "等待付款中";
					newLog.PaymentType = "None";
				}

				_context.PaymentLogs.Add(newLog);
				createdCount++;
			}

			if (createdCount > 0)
			{
				await _context.SaveChangesAsync();
				TempData["Success"] = $"已成功為 {createdCount} 筆訂單生成金流紀錄資料！";
			}
			else
			{
				TempData["Info"] = "目前所有訂單皆已有金流紀錄。";
			}

			return RedirectToAction(nameof(Index));
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