using ISpanShop.Models.EfModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Repositories.Support;

public class SupportTicketRepository : ISupportTicketRepository
{
	private readonly ISpanShopDBContext _context;
	public SupportTicketRepository(ISpanShopDBContext context) => _context = context;

	public async Task<List<SupportTicket>> GetAllAsync()
	{
		// 這裡可以 Include(t => t.User) 來取得使用者名稱
		return await _context.SupportTickets
			.OrderByDescending(t => t.CreatedAt)
			.ToListAsync();
	}

	public async Task<List<SupportTicket>> GetByUserIdAsync(int userId)
	{
		return await _context.SupportTickets
			.Where(t => t.UserId == userId)
			.OrderByDescending(t => t.CreatedAt)
			.ToListAsync();
	}

	public async Task<SupportTicket> GetByIdAsync(int id) =>
		await _context.SupportTickets.FirstOrDefaultAsync(t => t.Id == id);

	public async Task UpdateAsync(SupportTicket ticket)
	{
		_context.SupportTickets.Update(ticket);
		await _context.SaveChangesAsync();
	}

	public async Task CreateAsync(SupportTicket ticket)
	{
		_context.SupportTickets.Add(ticket);
		await _context.SaveChangesAsync();
	}
}