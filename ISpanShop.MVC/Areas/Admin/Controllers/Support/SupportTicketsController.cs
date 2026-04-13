using System;
using Microsoft.AspNetCore.Mvc;
using ISpanShop.Repositories.Support;
using ISpanShop.Services.Support;
using ISpanShop.MVC.Areas.Admin.Models.Support;
using ISpanShop.Models.EfModels;
using ISpanShop.Models.Seeding;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using ISpanShop.MVC.Middleware;

namespace ISpanShop.MVC.Areas.Admin.Controllers.Support
{
	[Area("Admin")]
	[RequirePermission("ticket_manage")]
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

		// 列表頁面 - 顯示所有客服工單 (支援分頁)
		[HttpGet]
		public async Task<IActionResult> Index(int page = 1)
		{
			int pageSize = 10;
			var dtos = await _service.GetAllAsync();

			// 計算分頁資訊
			int totalCount = dtos.Count;
			int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

			// 確保當前頁數在合理範圍內
			page = page < 1 ? 1 : page;
			if (totalPages > 0 && page > totalPages) page = totalPages;

			// 進行分頁切割
			var pagedDtos = dtos.OrderByDescending(d => d.CreatedAt)
							   .Skip((page - 1) * pageSize)
							   .Take(pageSize)
							   .ToList();

			// DTO 轉 ViewModel
			var vms = pagedDtos.Select(d => new SupportTicketVm
			{
				Id = d.Id,
				Subject = d.Subject,
				Category = d.Category,
				Status = d.Status,
				AdminReply = d.AdminReply,
				CreatedAt = d.CreatedAt
			}).ToList();

			// 將分頁資訊存入 ViewBag
			ViewBag.CurrentPage = page;
			ViewBag.TotalPages = totalPages;

			return View(vms);
		}

		[HttpGet]
		public async Task<IActionResult> Process(int id, int page = 1)
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

			ViewBag.Page = page; // 記錄當前是從哪一頁過來的
			return View(ticket);
		}

		[HttpPost]
		public async Task<IActionResult> SuspendUser(int userId, int ticketId, int page = 1)
		{
			var user = await _context.Users.FindAsync(userId);
			if (user != null)
			{
				user.IsBlacklisted = true;
				await _context.SaveChangesAsync();
			}

			return RedirectToAction("Process", new { id = ticketId, page = page });
		}

		[HttpPost]
		public async Task<IActionResult> ReplyTicket(int ticketId, string adminReply, byte status, int page = 1)
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

			TempData["SuccessMessage"] = "工單回覆已成功送出！";
			TempData["ProcessedTicketId"] = ticketId; // 記錄處理過的工單 ID

			// 5. 存檔後，跳轉回客服工單管理列表頁，並帶回原本的頁碼
			return RedirectToAction(nameof(Index), new { page = page });
		}

		// --- 4. 展示與測試工具 Action ---
		[HttpPost]
		public async Task<IActionResult> GenerateTestTickets()
		{
			// 直接強制補充 20 筆工單
			await DataSeeder.GenerateSupportTicketsAsync(_context, 20);
			TempData["SuccessMessage"] = "已成功生成 20 筆測試用客服工單！";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		public async Task<IActionResult> ClearAllTickets()
		{
			// 刪除所有工單資料 (Demo 方便用)
			var allTickets = await _context.SupportTickets.ToListAsync();
			_context.SupportTickets.RemoveRange(allTickets);
			await _context.SaveChangesAsync();
			
			TempData["SuccessMessage"] = "已成功清除所有工單資料！";
			return RedirectToAction(nameof(Index));
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
	}
}