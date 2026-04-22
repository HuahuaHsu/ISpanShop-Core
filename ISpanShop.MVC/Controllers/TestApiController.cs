using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

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

		// GET api/TestApi/cleanup-promotions
		[HttpGet("cleanup-promotions")]
		public async Task<IActionResult> CleanupPromotions()
		{
			var log = new StringBuilder();
			log.AppendLine("===== 促銷活動資料清理 =====\n");

			// 步驟 1: 查看無效活動
			log.AppendLine("步驟 1: 查看沒有有效 SellerId 的活動");
			var invalidPromotions = await _context.Promotions
				.Where(p => !_context.Users.Any(u => u.Id == p.SellerId) || p.IsDeleted)
				.Select(p => new { p.Id, p.Name, p.SellerId, p.Status })
				.ToListAsync();

			log.AppendLine($"找到 {invalidPromotions.Count} 筆無效活動");
			foreach (var p in invalidPromotions)
			{
				log.AppendLine($"  - ID: {p.Id}, Name: {p.Name}, SellerId: {p.SellerId}");
			}

			// 步驟 2: 刪除無效活動
			int deletedCount = 0;
			if (invalidPromotions.Any())
			{
				log.AppendLine("\n步驟 2: 刪除無效活動");
				var invalidIds = invalidPromotions.Select(p => p.Id).ToList();

				var rulesToDelete = await _context.PromotionRules.Where(r => invalidIds.Contains(r.PromotionId)).ToListAsync();
				_context.PromotionRules.RemoveRange(rulesToDelete);

				var itemsToDelete = await _context.PromotionItems.Where(i => invalidIds.Contains(i.PromotionId)).ToListAsync();
				_context.PromotionItems.RemoveRange(itemsToDelete);

				var promotionsToDelete = await _context.Promotions.Where(p => invalidIds.Contains(p.Id)).ToListAsync();
				_context.Promotions.RemoveRange(promotionsToDelete);

				await _context.SaveChangesAsync();
				deletedCount = promotionsToDelete.Count;
				log.AppendLine($"已刪除 {deletedCount} 筆無效活動");
			}

			// 步驟 3: 查看現有賣場
			log.AppendLine("\n步驟 3: 查看現有賣場");
			var stores = await _context.Stores
				.Include(s => s.User)
				.Where(s => s.StoreStatus == 1)
				.Select(s => new
				{
					s.Id,
					s.StoreName,
					s.UserId,
					s.User.Account,
					ProductCount = s.Products.Count(p => !p.IsDeleted)
				})
				.OrderBy(s => s.Id)
				.ToListAsync();

			log.AppendLine($"找到 {stores.Count} 個營業中的賣場:");
			foreach (var s in stores)
			{
				log.AppendLine($"  - StoreId: {s.Id}, StoreName: {s.StoreName}, UserId: {s.UserId}, Products: {s.ProductCount}");
			}

			// 步驟 4: 查看剩餘活動
			log.AppendLine("\n步驟 4: 查看剩餘活動");
			var remainingPromotions = await _context.Promotions
				.Include(p => p.Seller)
					.ThenInclude(u => u.Stores)
				.Where(p => !p.IsDeleted)
				.OrderBy(p => p.Id)
				.ToListAsync();

			log.AppendLine($"剩餘 {remainingPromotions.Count} 筆活動:");
			foreach (var p in remainingPromotions)
			{
				string typeLabel = p.PromotionType switch { 1 => "限時特賣", 2 => "滿額折扣", 3 => "限量搶購", _ => "其他" };
				string statusLabel = p.Status switch
				{
					0 => "待審核",
					1 when p.StartTime > DateTime.Now => "即將開始",
					1 when p.EndTime < DateTime.Now => "已結束",
					1 => "進行中",
					2 => "已拒絕",
					_ => "未知"
				};
				var storeName = p.Seller.Stores.FirstOrDefault(s => s.StoreStatus == 1)?.StoreName ?? "無賣場";
				log.AppendLine($"  - {p.Name} ({typeLabel}, {statusLabel}) - {storeName}");
			}

			// 步驟 5: 如果活動太少，新增測試資料
			log.AppendLine("\n步驟 5: 檢查是否需要新增測試資料");
			int promotionCount = remainingPromotions.Count;
			log.AppendLine($"目前活動數: {promotionCount}");
			int addedCount = 0;

			if (promotionCount < 5)
			{
				log.AppendLine("活動數量不足，開始新增測試資料...");
				var now = DateTime.Now;

				foreach (var store in stores.Where(s => s.ProductCount > 0))
				{
					int userId = store.UserId;
					string storeName = store.StoreName;

					if (!await _context.Promotions.AnyAsync(p => p.SellerId == userId && p.Status == 1 && p.StartTime <= now && p.EndTime >= now && !p.IsDeleted))
					{
						_context.Promotions.Add(new ISpanShop.Models.EfModels.Promotion
						{
							Name = $"{storeName} 限時特賣",
							Description = "全館指定商品限時優惠中",
							PromotionType = 1,
							StartTime = now.AddDays(-2),
							EndTime = now.AddDays(5),
							Status = 1,
							SellerId = userId,
							CreatedAt = now.AddDays(-3),
							IsDeleted = false
						});
						log.AppendLine($"  ✓ 為 {storeName} 新增「進行中」活動");
						addedCount++;
					}

					if (!await _context.Promotions.AnyAsync(p => p.SellerId == userId && p.Status == 0 && !p.IsDeleted))
					{
						_context.Promotions.Add(new ISpanShop.Models.EfModels.Promotion
						{
							Name = $"{storeName} 滿額優惠",
							Description = "消費滿千折百，滿兩千折三百",
							PromotionType = 2,
							StartTime = now.AddDays(3),
							EndTime = now.AddDays(10),
							Status = 0,
							SellerId = userId,
							CreatedAt = now.AddHours(-6),
							IsDeleted = false
						});
						log.AppendLine($"  ✓ 為 {storeName} 新增「待審核」活動");
						addedCount++;
					}

					if (!await _context.Promotions.AnyAsync(p => p.SellerId == userId && p.Status == 1 && p.StartTime > now && !p.IsDeleted))
					{
						_context.Promotions.Add(new ISpanShop.Models.EfModels.Promotion
						{
							Name = $"{storeName} 限量搶購",
							Description = "熱銷商品限量搶購，售完為止",
							PromotionType = 3,
							StartTime = now.AddDays(2),
							EndTime = now.AddDays(8),
							Status = 1,
							SellerId = userId,
							ReviewedBy = 1,
							ReviewedAt = now.AddHours(-12),
							CreatedAt = now.AddDays(-1),
							IsDeleted = false
						});
						log.AppendLine($"  ✓ 為 {storeName} 新增「即將開始」活動");
						addedCount++;
					}
				}

				await _context.SaveChangesAsync();
				log.AppendLine("測試資料新增完成！");
			}
			else
			{
				log.AppendLine($"活動數量充足 ({promotionCount} 筆)，跳過新增");
			}

			// 步驟 6: 最終確認
			log.AppendLine("\n===== 最終確認 - 所有活動列表 =====");
			var finalPromotions = await _context.Promotions
				.Include(p => p.Seller)
					.ThenInclude(u => u.Stores)
				.Where(p => !p.IsDeleted)
				.OrderBy(p => p.Status)
				.ThenBy(p => p.StartTime)
				.ToListAsync();

			foreach (var p in finalPromotions)
			{
				string typeLabel = p.PromotionType switch { 1 => "限時特賣", 2 => "滿額折扣", 3 => "限量搶購", _ => "其他" };
				string statusLabel = p.Status switch
				{
					0 => "待審核",
					1 when p.StartTime > DateTime.Now => "即將開始",
					1 when p.EndTime < DateTime.Now => "已結束",
					1 => "進行中",
					2 => "已拒絕",
					_ => "未知"
				};
				var storeName = p.Seller.Stores.FirstOrDefault(s => s.StoreStatus == 1)?.StoreName ?? "無賣場";
				log.AppendLine($"[{p.Id}] {p.Name} | {typeLabel} | {statusLabel} | {storeName}");
			}

			log.AppendLine("\n===== 統計資訊 =====");
			var total = finalPromotions.Count;
			var pending = finalPromotions.Count(p => p.Status == 0);
			var active = finalPromotions.Count(p => p.Status == 1 && p.StartTime <= DateTime.Now && p.EndTime >= DateTime.Now);
			var upcoming = finalPromotions.Count(p => p.Status == 1 && p.StartTime > DateTime.Now);
			var rejected = finalPromotions.Count(p => p.Status == 2);
			var ended = finalPromotions.Count(p => p.Status == 1 && p.EndTime < DateTime.Now);

			log.AppendLine($"總活動數: {total}");
			log.AppendLine($"待審核: {pending}");
			log.AppendLine($"進行中: {active}");
			log.AppendLine($"即將開始: {upcoming}");
			log.AppendLine($"已拒絕: {rejected}");
			log.AppendLine($"已結束: {ended}");

			log.AppendLine("\n✅ 資料清理與測試資料新增完成！");

			return Ok(new
			{
				success = true,
				deletedCount,
				addedCount,
				finalCount = total,
				stats = new
				{
					total,
					pending,
					active,
					upcoming,
					rejected,
					ended
				},
				log = log.ToString()
			});
		}
	}
}