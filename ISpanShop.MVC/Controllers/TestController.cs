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
			//測試連線
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
