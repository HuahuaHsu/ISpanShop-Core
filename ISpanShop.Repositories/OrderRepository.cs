using ISpanShop.Models.EfModels;
using ISpanShop.Models.EfModels.DTOs;
using ISpanShop.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Repositories
{
	public class OrderRepository : IOrderRepository
	{
		private readonly ISpanShopDBContext _context;

		public OrderRepository(ISpanShopDBContext context)
		{
			_context = context;
		}

		public async Task<(IEnumerable<Order> Items, int TotalCount)> GetPagedOrdersAsync(OrderSearchDto search)
		{
			var query = _context.Orders
				.Include(o => o.User)
				.Include(o => o.Store)
				.AsQueryable();

			// 篩選條件
			if (!string.IsNullOrWhiteSpace(search.OrderNumber))
			{
				query = query.Where(o => o.OrderNumber.Contains(search.OrderNumber));
			}

			if (search.Status.HasValue)
			{
				query = query.Where(o => o.Status == (byte)search.Status.Value);
			}

			if (search.StartDate.HasValue)
			{
				query = query.Where(o => o.CreatedAt >= search.StartDate.Value);
			}

			if (search.EndDate.HasValue)
			{
				// 包含當天 23:59:59
				var endDate = search.EndDate.Value.Date.AddDays(1).AddTicks(-1);
				query = query.Where(o => o.CreatedAt <= endDate);
			}

			// 計算總筆數
			int totalCount = await query.CountAsync();

			// 執行分頁與排序
			var items = await query
				.OrderByDescending(o => o.CreatedAt)
				.Skip((search.PageNumber - 1) * search.PageSize)
				.Take(search.PageSize)
				.ToListAsync();

			return (items, totalCount);
		}
	}
}
