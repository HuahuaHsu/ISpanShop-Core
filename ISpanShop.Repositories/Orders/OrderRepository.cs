using ISpanShop.Models.DTOs;
using ISpanShop.Models.DTOs.Orders;
using ISpanShop.Models.EfModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Repositories.Orders
{
	public class OrderRepository : IOrderRepository
	{
		private readonly ISpanShopDBContext _context;

		public OrderRepository(ISpanShopDBContext context)
		{
			_context = context;
		}

		public async Task<IDictionary<byte, int>> GetOrderStatusCountsAsync()
		{
			return await _context.Orders
				.GroupBy(o => o.Status)
				.Select(g => new { Status = g.Key ?? 0, Count = g.Count() })
				.ToDictionaryAsync(x => x.Status, x => x.Count);
		}

		public async Task<Order> GetOrderByIdAsync(long id)
		{
			return await _context.Orders
				.Include(o => o.User)
				.Include(o => o.Store)
				.Include(o => o.OrderDetails)
					.ThenInclude(od => od.Product)
				.Include(o => o.ReturnRequests)
					.ThenInclude(rr => rr.ReturnRequestImages)
				.FirstOrDefaultAsync(o => o.Id == id);
		}

		public async Task UpdateStatusAsync(long id, byte status)
		{
			var order = await _context.Orders.Include(o => o.ReturnRequests).FirstOrDefaultAsync(o => o.Id == id);
			if (order != null)
			{
				order.Status = status;
				if (status == 3) // 已完成 (Completed = 3)
				{
					order.CompletedAt = DateTime.Now;
					
					// 如果有退貨申請，標記為已拒絕 (2: Rejected)
					var rr = order.ReturnRequests.OrderByDescending(x => x.CreatedAt).FirstOrDefault();
					if (rr != null && rr.Status == 0) rr.Status = 2;
				}
				else if (status == 6) // 已退款 (Refunded = 6)
				{
					// 如果有退貨申請，標記為已核准 (1: Approved)
					var rr = order.ReturnRequests.OrderByDescending(x => x.CreatedAt).FirstOrDefault();
					if (rr != null && rr.Status == 0) rr.Status = 1;
				}

				await _context.SaveChangesAsync();
			}
		}

		public async Task<PagedResultDto<OrderListDto>> GetFilteredOrdersAsync(OrderSearchDto criteria)
		{
			var query = _context.Orders
				.Include(o => o.User)
				.ThenInclude(u => u.MemberProfile)
				.Include(o => o.Store)
				.AsQueryable();

			// 如果是出貨工作台 (只有待出貨)，載入明細以生成摘要
			bool isShipmentWorkstation = criteria.Statuses != null && criteria.Statuses.Count == 1 && criteria.Statuses.Contains(1);
			if (isShipmentWorkstation)
			{
				query = query.Include(o => o.OrderDetails);
			}

			// A1. 基礎資訊篩選
			if (!string.IsNullOrEmpty(criteria.Keyword))
			{
				var kw = criteria.Keyword.Trim();
				query = query.Where(o =>
					o.OrderNumber.Contains(kw) ||
					o.UserId.ToString() == kw ||
					o.RecipientName.Contains(kw) ||
					o.RecipientPhone.Contains(kw));
			}

			// A2. 訂單狀態
			if (criteria.Statuses != null && criteria.Statuses.Any())
			{
				var byteStatuses = criteria.Statuses.Select(s => (byte)s).ToList();
				query = query.Where(o => o.Status.HasValue && byteStatuses.Contains(o.Status.Value));
			}

			// A3. 金額區間
			if (criteria.MinAmount.HasValue)
				query = query.Where(o => o.FinalAmount >= criteria.MinAmount.Value);
			if (criteria.MaxAmount.HasValue)
				query = query.Where(o => o.FinalAmount <= criteria.MaxAmount.Value);

			// A4 & A5. 日期篩選
			if (criteria.StartDate.HasValue || criteria.EndDate.HasValue)
			{
				DateTime start = criteria.StartDate ?? DateTime.MinValue;
				DateTime end = criteria.EndDate ?? DateTime.MaxValue;

				query = criteria.DateDimension switch
				{
					2 => query.Where(o => o.PaymentDate >= start && o.PaymentDate <= end),
					3 => query.Where(o => o.CompletedAt >= start && o.CompletedAt <= end),
					_ => query.Where(o => o.CreatedAt >= start && o.CreatedAt <= end)
				};
			}

			// A6. 商店過濾
			if (criteria.StoreId.HasValue)
				query = query.Where(o => o.StoreId == criteria.StoreId.Value);
			if (!string.IsNullOrEmpty(criteria.StoreName))
				query = query.Where(o => o.Store != null && o.Store.StoreName.Contains(criteria.StoreName));

			// B. 動態排序
			query = criteria.SortBy switch
			{
				"TotalAmount" => criteria.IsDescending ? query.OrderByDescending(o => o.FinalAmount) : query.OrderBy(o => o.FinalAmount),
				"Status" => criteria.IsDescending ? query.OrderByDescending(o => o.Status) : query.OrderBy(o => o.Status),
				"MemberName" => criteria.IsDescending ? query.OrderByDescending(o => o.User.MemberProfile.FullName) : query.OrderBy(o => o.User.MemberProfile.FullName),
				_ => criteria.IsDescending ? query.OrderByDescending(o => o.CreatedAt) : query.OrderBy(o => o.CreatedAt)
			};

			var totalCount = await query.CountAsync();
			var orders = await query
				.Skip((criteria.PageNumber - 1) * criteria.PageSize)
				.Take(criteria.PageSize)
				.ToListAsync();

			var items = orders.Select(o => {
				var dto = new OrderListDto
				{
					OrderId = (int)o.Id,
					OrderUuid = o.OrderNumber,
					MemberName = o.User != null && o.User.MemberProfile != null ? o.User.MemberProfile.FullName : "無",
					TotalAmount = o.FinalAmount,
					StatusId = o.Status.HasValue ? (int)o.Status.Value : 0,
					StatusName = o.Status switch {
						0 => "待付款", 1 => "待出貨", 2 => "運送中", 3 => "已完成",
						4 => "已取消", 5 => "退貨/款中", 6 => "已退款", _ => "未知"
					},
					OrderDate = o.CreatedAt ?? DateTime.MinValue,
					PaymentDate = o.PaymentDate,
					CompletedAt = o.CompletedAt,
					RecipientName = o.RecipientName,
					RecipientPhone = o.RecipientPhone,
					StoreName = o.Store != null ? o.Store.StoreName : "平台"
				};

				if (isShipmentWorkstation && o.OrderDetails != null)
				{
					dto.ItemsSummary = string.Join(", ", o.OrderDetails.Select(od => $"{od.ProductName} x{od.Quantity}"));
					if (o.PaymentDate.HasValue)
					{
						var diff = DateTime.Now - o.PaymentDate.Value;
						dto.WaitingTime = diff.TotalHours >= 24 
							? $"{(int)diff.TotalDays}天{diff.Hours}小時" 
							: $"{(int)diff.TotalHours}小時{diff.Minutes}分";
					}
				}
				return dto;
			}).ToList();

			return new PagedResultDto<OrderListDto> { Items = items, TotalCount = totalCount, PageNumber = criteria.PageNumber, PageSize = criteria.PageSize };
		}

		// -------------------------------------------------------------------------
		// 以下為圖表與 KPI 查詢實作 (透過 GroupBy, Sum, Count 等彙整 DB 資料)
		// -------------------------------------------------------------------------

		public async Task<DashboardKpiRawDataDto> GetDashboardKpisAsync(int? storeId, DateTime startDate, DateTime endDate, DateTime prevStartDate, DateTime prevEndDate)
		{
			var query = _context.Orders.AsNoTracking();
			if (storeId.HasValue) query = query.Where(o => o.StoreId == storeId.Value);

			var currentOrders = await query.Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate).ToListAsync();
			var prevOrders = await query.Where(o => o.CreatedAt >= prevStartDate && o.CreatedAt <= prevEndDate).ToListAsync();

			// 依據新 Enum: 3=已完成 (Completed)
			var currentNetRevenue = currentOrders.Where(o => o.Status == 3).Sum(o => o.FinalAmount);
			var prevNetRevenue = prevOrders.Where(o => o.Status == 3).Sum(o => o.FinalAmount);

			var lowStockProductCount = await _context.ProductVariants.AsNoTracking().CountAsync(p => p.Stock < 10);

			var currentOrderIds = currentOrders.Select(o => o.Id).ToList();
			var prevOrderIds = prevOrders.Select(o => o.Id).ToList();

			var currentItemsSold = currentOrderIds.Any() ? await _context.OrderDetails.Where(od => currentOrderIds.Contains(od.OrderId)).SumAsync(od => od.Quantity) : 0;
			var prevItemsSold = prevOrderIds.Any() ? await _context.OrderDetails.Where(od => prevOrderIds.Contains(od.OrderId)).SumAsync(od => od.Quantity) : 0;

			return new DashboardKpiRawDataDto
			{
				NetRevenue = currentNetRevenue,
				PrevNetRevenue = prevNetRevenue,
				TotalOrders = currentOrders.Count,
				PrevTotalOrders = prevOrders.Count,
				ReturnOrders = currentOrders.Count(o => o.Status == 4), // 4=已取消 (Cancelled)
				PrevReturnOrders = prevOrders.Count(o => o.Status == 4),
				TotalItemsSold = currentItemsSold,
				PrevTotalItemsSold = prevItemsSold,
				// 待出貨數通常顯示「當前所有」需要處理的訂單，不應受時間區間限制
				PendingShipmentCount = await _context.Orders.CountAsync(o => o.Status == 1), 
				PendingRefundCount = 0, 
				LowStockProductCount = lowStockProductCount
			};
		}

		public async Task<ApexChartDataDto> GetProductSalesBarChartAsync(int? storeId, DateTime startDate, DateTime endDate)
		{
			var query = _context.OrderDetails
				.Include(od => od.Order)
				.Include(od => od.Product)
				.Where(od => od.Order.CreatedAt >= startDate && od.Order.CreatedAt <= endDate && od.Order.Status == 3);

			if (storeId.HasValue) query = query.Where(od => od.Order.StoreId == storeId.Value);

			var groupedData = await query
				.GroupBy(od => od.Product.Name)
				.Select(g => new { ProductName = g.Key, TotalSales = g.Sum(od => od.Quantity) })
				.OrderByDescending(x => x.TotalSales)
				.Take(10)
				.ToListAsync();

			var dto = new ApexChartDataDto();
			var seriesData = new List<decimal>();

			foreach (var item in groupedData)
			{
				dto.Labels.Add(item.ProductName ?? "未命名商品");
				seriesData.Add(item.TotalSales);
			}

			dto.Series.Add(new ChartSeriesDto { Name = "銷售量", Data = seriesData });
			return dto;
		}

		public async Task<ApexChartDataDto> GetProductSalesPieChartAsync(int? storeId, DateTime startDate, DateTime endDate)
		{
			// 需求：改為抓取「類別」占比 (銷售量)
			var query = _context.OrderDetails
				.Include(od => od.Order)
				.Include(od => od.Product)
				.ThenInclude(p => p.Category)
				.Where(od => od.Order.CreatedAt >= startDate && od.Order.CreatedAt <= endDate && od.Order.Status == 3);

			if (storeId.HasValue) query = query.Where(od => od.Order.StoreId == storeId.Value);

			var groupedData = await query
				.GroupBy(od => od.Product.Category.Name)
				.Select(g => new { CategoryName = g.Key, TotalSales = g.Sum(od => od.Quantity) })
				.OrderByDescending(x => x.TotalSales)
				.ToListAsync();

			var dto = new ApexChartDataDto();
			var seriesData = new List<decimal>();

			foreach (var item in groupedData)
			{
				dto.Labels.Add(item.CategoryName ?? "未分類");
				seriesData.Add(item.TotalSales);
			}

			dto.Series.Add(new ChartSeriesDto { Name = "類別銷售比例", Data = seriesData });
			return dto;
		}

		public async Task<ApexChartDataDto> GetMonthlySalesTrendAsync(int? storeId, DateTime startDate, DateTime endDate)
		{
			// 僅計算已完成訂單 (Status == 3: Completed)
			var query = _context.Orders.Where(o => o.CreatedAt.HasValue && o.CreatedAt >= startDate && o.CreatedAt <= endDate && o.Status == 3);

			if (storeId.HasValue) query = query.Where(o => o.StoreId == storeId.Value);

			var monthlyData = await query
				.GroupBy(o => new { o.CreatedAt.Value.Year, o.CreatedAt.Value.Month })
				.Select(g => new { g.Key.Year, g.Key.Month, Revenue = g.Sum(o => o.FinalAmount) })
				.ToListAsync();

			var dto = new ApexChartDataDto();
			var seriesData = new List<decimal>();

			// 生成從 startDate 到 endDate 的月份標籤
			DateTime current = new DateTime(startDate.Year, startDate.Month, 1);
			while (current <= endDate)
			{
				dto.Labels.Add($"{current.Year}/{current.Month:D2}");
				var monthData = monthlyData.FirstOrDefault(m => m.Year == current.Year && m.Month == current.Month);
				seriesData.Add(monthData != null ? monthData.Revenue : 0);
				current = current.AddMonths(1);
			}

			dto.Series.Add(new ChartSeriesDto { Name = "月營收", Data = seriesData });
			return dto;
		}

		public async Task<List<TopProductSalesDto>> GetTop10ProductsAsync(int? storeId, DateTime startDate, DateTime endDate, string orderBy)
		{
			var query = _context.OrderDetails
				.Include(od => od.Order)
				.Include(od => od.Product)
				.ThenInclude(p => p.Category)
				.Where(od => od.Order.CreatedAt >= startDate && od.Order.CreatedAt <= endDate && od.Order.Status == 3);

			if (storeId.HasValue) query = query.Where(od => od.Order.StoreId == storeId.Value);

			var groupedQuery = query
				.GroupBy(od => new { od.Product.Name, CategoryName = od.Product.Category.Name })
				.Select(g => new TopProductSalesDto
				{
					ProductName = g.Key.Name,
					CategoryName = g.Key.CategoryName ?? "未分類",
					SalesVolume = g.Sum(od => od.Quantity),
					SalesRevenue = g.Sum(od => (od.Price ?? 0) * od.Quantity)
				});

			return orderBy.ToLower() == "volume"
				? await groupedQuery.OrderByDescending(x => x.SalesVolume).Take(10).ToListAsync()
				: await groupedQuery.OrderByDescending(x => x.SalesRevenue).Take(10).ToListAsync();
		}

		public async Task<ApexChartDataDto> GetCategoryContributionAsync(int? storeId, DateTime startDate, DateTime endDate)
		{
			var query = _context.OrderDetails
				.Include(od => od.Order)
				.Include(od => od.Product)
				.ThenInclude(p => p.Category)
				.Where(od => od.Order.CreatedAt >= startDate && od.Order.CreatedAt <= endDate && od.Order.Status == 3);

			if (storeId.HasValue) query = query.Where(od => od.Order.StoreId == storeId.Value);

			var groupedData = await query
				.GroupBy(od => od.Product.Category.Name)
				.Select(g => new { CategoryName = g.Key, Revenue = g.Sum(od => (od.Price ?? 0) * od.Quantity) })
				.OrderByDescending(x => x.Revenue)
				.ToListAsync();

			var dto = new ApexChartDataDto();
			var seriesData = new List<decimal>();

			foreach (var item in groupedData)
			{
				dto.Labels.Add(item.CategoryName ?? "未分類");
				seriesData.Add(item.Revenue);
			}

			dto.Series.Add(new ChartSeriesDto { Name = "營收佔比", Data = seriesData });
			return dto;
		}
	}
}
