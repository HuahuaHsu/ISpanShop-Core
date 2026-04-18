using ISpanShop.Models.DTOs.Support;
using ISpanShop.Repositories.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISpanShop.Services.Support
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
			return MapToDtos(entities);
		}

		public async Task<List<SupportTicketDto>> GetByUserIdAsync(int userId)
		{
			var entities = await _repo.GetByUserIdAsync(userId);
			return MapToDtos(entities);
		}

		private List<SupportTicketDto> MapToDtos(List<ISpanShop.Models.EfModels.SupportTicket> entities)
		{
			return entities.Select(e => new SupportTicketDto
			{
				Id = e.Id,
				UserId = e.UserId,
				OrderId = e.OrderId,
				Category = e.Category,
				Subject = e.Subject,
				Description = e.Description,
				AttachmentUrl = e.AttachmentUrl,
				Status = e.Status, // 直接指派，因為 DTO 已經改為 byte?
				AdminReply = e.AdminReply,
				CreatedAt = e.CreatedAt, // 直接指派，因為 DTO 已經改為 DateTime?
				ResolvedAt = e.ResolvedAt
			}).ToList();
		}

		public async Task ReplyAndCloseTicketAsync(int id, string adminReply)
		{
			var ticket = await _repo.GetByIdAsync(id);
			if (ticket != null)
			{
				ticket.AdminReply = adminReply;
				ticket.Status = 2; 
				ticket.ResolvedAt = DateTime.Now;
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
				Status = e.Status,
				AdminReply = e.AdminReply,
				CreatedAt = e.CreatedAt,
				ResolvedAt = e.ResolvedAt
			};
		}

		public async Task CreateAsync(SupportTicketDto dto)
		{
			var entity = new ISpanShop.Models.EfModels.SupportTicket
			{
				UserId = dto.UserId,
				OrderId = dto.OrderId,
				Category = dto.Category,
				Subject = dto.Subject,
				Description = dto.Description,
				AttachmentUrl = dto.AttachmentUrl,
				Status = 0, // 待處理
				CreatedAt = DateTime.Now
			};

			await _repo.CreateAsync(entity);
		}
	}
}