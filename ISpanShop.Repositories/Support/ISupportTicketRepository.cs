using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISpanShop.Models.EfModels;

namespace ISpanShop.Repositories.Support
{
	public interface ISupportTicketRepository
	{
		Task<List<SupportTicket>> GetAllAsync();
		Task<List<SupportTicket>> GetByUserIdAsync(int userId);
		Task<SupportTicket> GetByIdAsync(int id);
		Task UpdateAsync(SupportTicket ticket);
		Task CreateAsync(SupportTicket ticket);
	}
}
