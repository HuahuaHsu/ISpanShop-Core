using ISpanShop.Models;
using ISpanShop.Models.EfModels;
using Microsoft.AspNetCore.Mvc;

namespace ISpanShop.Controllers
{
	public class TestController : Controller
	{
		private readonly ISpanShopDBContext _context;

		// 注入資料庫連線 (依賴注入)
		public TestController(ISpanShopDBContext context)
		{
			_context = context;
		}

		public IActionResult Index()
		{
			// 這一行是關鍵！它會試著去連資料庫
			// 如果成功，會回傳 true；如果失敗，程式會直接報錯
			bool isConnected = _context.Database.CanConnect();

			if (isConnected)
			{
				return Content("恭喜！資料庫連線成功！ (Success)");
			}
			else
			{
				return Content("資料庫連線失敗... (Failed)");
			}
		}
	}
}
