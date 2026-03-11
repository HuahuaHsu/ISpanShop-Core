using ISpanShop.Models.DTOs;
using ISpanShop.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISpanShop.Services
{
	public class SupportTicketService : ISupportTicketService
	{
		private readonly ISupportTicketRepository _repo;

		public SupportTicketService(ISupportTicketRepository repo)
		{
			_repo = repo;
		}

		public async Task<List<SupportTicketDto>> GetAllAsync()
		{
			var entities = await _repo.GetAllAsync();

			// 將 Entity 轉換成 DTO 傳給前端
			return entities.Select(e => new SupportTicketDto
			{
				Id = e.Id,
				UserId = e.UserId,       // 拿掉 ??，因為它本來就是不可為空的 int
				OrderId = e.OrderId,
				Category = e.Category,   // 拿掉 ??，因為它本來就是不可為空的 byte
				Subject = e.Subject,
				Description = e.Description,
				AttachmentUrl = e.AttachmentUrl,

				// 【精準轉型】將 e.Status (byte?) 轉成 byte，若是 null 則給 0
				Status = (byte)(e.Status ?? 0),

				AdminReply = e.AdminReply,

				// DateTime? 給予預設值
				CreatedAt = e.CreatedAt ?? DateTime.MinValue,

				ResolvedAt = e.ResolvedAt
			}).ToList();
		}

		public async Task ReplyAndCloseTicketAsync(int id, string adminReply)
		{
			// 1. 先從資料庫把該筆工單撈出來
			var ticket = await _repo.GetByIdAsync(id);

			if (ticket != null)
			{
				// 2. 執行商業邏輯：填入回覆、改狀態、押結案時間
				ticket.AdminReply = adminReply;
				ticket.Status = 2; // 2 代表「已結案」
				ticket.ResolvedAt = DateTime.Now;

				// 3. 叫 Repository 幫忙存進資料庫
				await _repo.UpdateAsync(ticket);
			}
		}
		public async Task<SupportTicketDto> GetTicketDetailsAsync(int id)
		{
			var e = await _repo.GetByIdAsync(id);
			if (e == null) return null;

			return new SupportTicketDto
			{
				Id = e.Id,
				UserId = e.UserId,
				OrderId = e.OrderId,
				Category = e.Category,
				Subject = e.Subject,
				Description = e.Description,
				AttachmentUrl = e.AttachmentUrl,
				Status = (byte)(e.Status ?? 0),
				AdminReply = e.AdminReply,
				CreatedAt = e.CreatedAt ?? DateTime.MinValue,
				ResolvedAt = e.ResolvedAt
			};
		}
	}
}