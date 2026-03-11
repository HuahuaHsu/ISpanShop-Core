using ISpanShop.Models.EfModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using ISpanShop.MVC.Models; // 確保有引用這個，ErrorViewModel 才不會紅線

namespace ISpanShop.MVC.Controllers
{
	public class HomeController : Controller
	{
		// 1. 宣告資料庫上下文
		private readonly ISpanShopDBContext _context;

		// 2. 建構函式注入 _context
		public HomeController(ISpanShopDBContext context)
		{
			_context = context;
		}

		// 3. 修改後的 Index 方法
		public async Task<IActionResult> Index()
		{
			// 抓取訂單並傳給 View
			var orders = await _context.Orders
				.OrderByDescending(o => o.CreatedAt)
				.ToListAsync();

			// 如果 Orders 資料表完全沒資料，orders 會是空列表，不會報錯
			return View(orders);
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}