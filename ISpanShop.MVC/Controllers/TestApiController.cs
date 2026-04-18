using Microsoft.AspNetCore.Mvc;

namespace ISpanShop.MVC.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TestApiController : ControllerBase
	{
		private readonly ISpanShop.Models.EfModels.ISpanShopDBContext _context;

		public TestApiController(ISpanShop.Models.EfModels.ISpanShopDBContext context)
		{
			_context = context;
		}

		[HttpGet]
		public IActionResult Get()
		{
			// 回傳一段簡單的 JSON 資料
			return Ok(new
			{
				success = true,
				message = "恭喜！CORS 設定大成功，前後端連線無障礙！"
			});
		}

		// 用來快速生成測試訂單： GET api/TestApi/generate-orders?userId=1
		[HttpGet("generate-orders")]
		public async Task<IActionResult> GenerateOrders(int userId)
		{
			var user = _context.Users.Find(userId);
			if (user == null) return NotFound("找不到該會員");

			var products = _context.Products.Take(5).ToList();
			if (!products.Any()) return BadRequest("資料庫中沒有商品，無法生成訂單");

			var orders = new List<ISpanShop.Models.EfModels.Order>();
			var random = new Random();

			for (int i = 0; i < 3; i++)
			{
				var order = new ISpanShop.Models.EfModels.Order
				{
					UserId = userId,
					OrderNumber = "TEST" + DateTime.Now.Ticks.ToString().Substring(10),
					StoreId = products[0].StoreId,
					TotalAmount = 0,
					FinalAmount = 0,
					Status = (byte)random.Next(0, 4), // 隨機狀態
					RecipientName = "測試收件人",
					RecipientPhone = "0912345678",
					RecipientAddress = "台北市大安區測試路123號",
					CreatedAt = DateTime.Now.AddDays(-random.Next(1, 10)),
					OrderDetails = new List<ISpanShop.Models.EfModels.OrderDetail>()
				};

				// 隨機選 1-3 個商品
				var orderProducts = products.OrderBy(x => random.Next()).Take(random.Next(1, 4));
				decimal total = 0;

				foreach (var p in orderProducts)
				{
					var price = p.MinPrice ?? 100;
					var qty = random.Next(1, 3);
					order.OrderDetails.Add(new ISpanShop.Models.EfModels.OrderDetail
					{
						ProductId = p.Id,
						ProductName = p.Name,
						Price = price,
						Quantity = qty,
						VariantName = "預設規格"
					});
					total += price * qty;
				}

				order.TotalAmount = total;
				order.FinalAmount = total;
				orders.Add(order);
			}

			_context.Orders.AddRange(orders);
			await _context.SaveChangesAsync();

			return Ok($"已成功為 UserId {userId} 生成 3 筆測試訂單");
		}
	}
}