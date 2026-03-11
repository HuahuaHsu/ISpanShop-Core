using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISpanShop.Models.EfModels;

namespace ISpanShop.Repositories
{
	public interface ISupportTicketRepository
	{
		Task<List<SupportTicket>> GetAllAsync();
		Task<SupportTicket> GetByIdAsync(int id);
		Task UpdateAsync(SupportTicket ticket);
	}
}
