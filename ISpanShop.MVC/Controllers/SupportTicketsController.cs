using Microsoft.AspNetCore.Mvc;
using ISpanShop.Services;
using ISpanShop.MVC.Models.ViewModels;
using System.Threading.Tasks;
using System.Linq;

namespace ISpanShop.MVC.Controllers
{
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
	}
}