using Microsoft.AspNetCore.Mvc;
using ISpanShop.Repositories.Support;
using ISpanShop.Services.Support;
using ISpanShop.MVC.Areas.Admin.Models.Support;
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

		// 依賴注入 Service
		public SupportTicketsController(ISupportTicketService service)
		{
			_service = service;
		}

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