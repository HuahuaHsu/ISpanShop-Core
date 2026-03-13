using Microsoft.AspNetCore.Mvc;
using ISpanShop.Repositories.Support;
using ISpanShop.Services.Support;
using ISpanShop.MVC.Areas.Admin.Models.Support;
using ISpanShop.Models.EfModels;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace ISpanShop.MVC.Areas.Admin.Controllers.Support
{
	[Area("Admin")]

	public class SupportTicketsController : Controller
	{
		private readonly ISupportTicketService _service;
		private readonly ISpanShopDBContext _context;

		// 依賴注入 Service
		public SupportTicketsController(ISupportTicketService service, ISpanShopDBContext context)
		{
			_service = service;
			_context = context;
		}

		// 模擬一個取得當前管理員等級的服務 (目前假裝是 1: IT工程部)
		private int GetCurrentAdminLevelId() => 1;

		// 列表頁面 - 顯示所有客服工單
		public async Task<IActionResult> Index()
		{
			var dtos = await _service.GetAllAsync();

			// DTO 轉 ViewModel
			var vms = dtos.Select(d => new SupportTicketVm
			{
				Id = d.Id,
				Subject = d.Subject,
				Category = d.Category,
				Status = d.Status,
				AdminReply = d.AdminReply,
				CreatedAt = d.CreatedAt
			}).ToList();

			return View(vms);
		}

		[HttpGet]
		public async Task<IActionResult> Process(int id)
		{
			// 1. 取得當前登入的部門 ID (目前假裝是 1: IT工程部)
			int currentDeptId = GetCurrentAdminLevelId();

			// 2. 撈取工單，同時把發起人的 User 實體也拉出來 (因為你要看他有沒有被停權)
			var ticket = await _context.SupportTickets
				.Include(t => t.User)
				.FirstOrDefaultAsync(t => t.Id == id);

			if (ticket == null) return NotFound("找不到該工單");

			// 🚨 核心權限防護：如果 IT 人員輸入網址硬闖「商品單 (Category 2)」，直接擋掉！
			// Category: 0:訂單, 1:帳號, 2:檢舉
			if (currentDeptId == 1 && ticket.Category != 1)
			{
				return Unauthorized("權限不足：IT 部門僅能處理帳號類工單。");
			}

			// 3. 針對 IT 部門的需求：撈取該使用者的最近 5 筆登入 Log
			if (currentDeptId == 1)
			{
				var loginLogs = await _context.LoginHistories
					.Where(l => l.UserId == ticket.UserId)
					.OrderByDescending(l => l.LoginTime)
					.Take(5)
					.ToListAsync();

				ViewBag.LoginLogs = loginLogs; // 傳給前端畫面顯示
			}

			return View(ticket);
		}

		[HttpPost]
		public async Task<IActionResult> SuspendUser(int userId, int ticketId)
		{
			var user = await _context.Users.FindAsync(userId);
			if (user != null)
			{
				user.IsBlacklisted = true;
				await _context.SaveChangesAsync();
			}

			return RedirectToAction("Process", new { id = ticketId });
		}

		[HttpPost]
		public async Task<IActionResult> ReplyTicket(int ticketId, string adminReply, byte status)
		{
			// 1. 從資料庫抓出這張工單
			var ticket = await _context.SupportTickets.FindAsync(ticketId);

			if (ticket == null)
			{
				return NotFound("找不到該工單");
			}

			// 2. 更新回覆內容與狀態
			ticket.AdminReply = adminReply;
			ticket.Status = status;

			// 3. 如果狀態被改成「已結案 (2)」，順便押上結案時間
			if (status == 2 && ticket.ResolvedAt == null)
			{
				ticket.ResolvedAt = DateTime.Now;
			}

			// 4. 存檔
			await _context.SaveChangesAsync();

			// 5. 存檔後，重新導向回這個處理畫面，讓使用者看到最新狀態
			return RedirectToAction("Process", new { id = ticketId });
		}

		[HttpGet]
		public async Task<IActionResult> Reply(int id)
		{
			var dto = await _service.GetTicketDetailsAsync(id);
			if (dto == null) return NotFound();

			var vm = new SupportTicketVm
			{
				Id = dto.Id,
				Subject = dto.Subject,
				Category = dto.Category,
				Status = dto.Status,
				Description = dto.Description,
				AttachmentUrl = dto.AttachmentUrl,
				OrderId = dto.OrderId,
				AdminReply = dto.AdminReply,
				CreatedAt = dto.CreatedAt
			};

			return View(vm);
		}

		// --- 3. 接收管理員的回覆並結案 (POST) ---
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Reply(SupportTicketVm vm)
		{
			// 這裡我們只要確保有填寫回覆內容即可
			if (!string.IsNullOrEmpty(vm.AdminReply))
			{
				// 呼叫 Service 執行回覆與結案邏輯
				await _service.ReplyAndCloseTicketAsync(vm.Id, vm.AdminReply);
				return RedirectToAction(nameof(Index)); // 回覆完導回列表
			}

			ModelState.AddModelError("AdminReply", "回覆內容不能為空白！");
			return View(vm); // 如果沒填寫，留在原畫面並顯示錯誤
		}
	} // <-- 確保全部都被包在這個 Controller 的大括號裡面
}