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
				.AsNoTracking()
				.Include(o => o.User)
				.Include(o => o.Store)
				.Include(o => o.Coupon)
				.Include(o => o.OrderDetails)
					.ThenInclude(od => od.Product)
						.ThenInclude(p => p.ProductImages)
				.Include(o => o.OrderDetails)
					.ThenInclude(od => od.Product)
						.ThenInclude(p => p.ProductVariants)
							.ThenInclude(pv => pv.ProductImages)
				.Include(o => o.ReturnRequests)
					.ThenInclude(rr => rr.ReturnRequestImages)
				.Include(o => o.ReturnRequests)
					.ThenInclude(rr => rr.ReturnRequestItems)
						.ThenInclude(ri => ri.OrderDetail)
							.ThenInclude(od => od.Product)
								.ThenInclude(p => p.ProductImages)
				.Include(o => o.OrderReviews)
				.AsSplitQuery()
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
					if (rr != null && rr.Status == 0)
					{
						rr.Status = 2;
						rr.UpdatedAt = DateTime.Now;
					}
				}
				else if (status == 5) // 退貨/款中 (Refund/Return in Progress = 5)
				{
					// 自動產生退貨申請紀錄 (如果不存在進行中的申請)
					var hasActiveRequest = order.ReturnRequests.Any(r => r.Status == 0);
					if (!hasActiveRequest)
					{
						var newRequest = new ReturnRequest
						{
							OrderId = order.Id,
							ReasonCategory = "管理員發起",
							ReasonDescription = "由管理員手動發起退貨流程",
							Status = 0, // 待處理
							CreatedAt = DateTime.Now
						};
						_context.ReturnRequests.Add(newRequest);
					}
				}
				else if (status == 6) // 已退款 (Refunded = 6)
				{
					// 如果有退貨申請，標記為已核准 (1: Approved) 並更新時間
					var rr = order.ReturnRequests.OrderByDescending(x => x.CreatedAt).FirstOrDefault();
					if (rr != null && rr.Status == 0)
					{
						rr.Status = 1;
						rr.UpdatedAt = DateTime.Now;
					}
				}

				await _context.SaveChangesAsync();
			}
		}

		public async Task CreateReturnRequestAsync(ReturnRequest request)
		{
			_context.ReturnRequests.Add(request);
			await _context.SaveChangesAsync();
		}

		public async Task<List<Order>> GetOrdersByMemberIdAsync(int memberId)
		{
			return await _context.Orders
				.AsNoTracking()
				.Include(o => o.Store)
				.Include(o => o.OrderReviews)
				.Include(o => o.OrderDetails)
					.ThenInclude(od => od.Product)
						.ThenInclude(p => p.ProductImages)
				.Include(o => o.OrderDetails)
					.ThenInclude(od => od.Product)
						.ThenInclude(p => p.ProductVariants)
							.ThenInclude(pv => pv.ProductImages)
				.Where(o => o.UserId == memberId)
				.OrderByDescending(o => o.CreatedAt)
				.ToListAsync();
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

			// 如果涉及退貨相關的篩選或排序，則載入退貨資訊
			bool isReturnRelated = criteria.Statuses != null && (criteria.Statuses.Contains(5) || criteria.Statuses.Contains(6));
			if (isReturnRelated || criteria.DateDimension == 4 || criteria.DateDimension == 5 || criteria.SortBy == "RefundDate" || criteria.SortBy == "ReturnRequestDate")
			{
				query = query.Include(o => o.ReturnRequests);
			}

			// A1. 基礎資訊篩選
			if (!string.IsNullOrEmpty(criteria.Keyword))
			{
				var kw = criteria.Keyword.Trim();
				query = query.Where(o =>
					o.OrderNumber.Contains(kw) ||
					o.UserId.ToString() == kw ||
					o.RecipientName.Contains(kw) ||
					o.RecipientPhone.Contains(kw) ||
					(o.User != null && o.User.MemberProfile != null && o.User.MemberProfile.FullName.Contains(kw)));
			}

			// A1.5 會員 ID 篩選 (前台個人中心需求)
			if (criteria.UserId.HasValue)
			{
				query = query.Where(o => o.UserId == criteria.UserId.Value);
			}

			// A2. 訂單狀態
			if (criteria.Statuses != null && criteria.Statuses.Any())
			{
				var byteStatuses = criteria.Statuses.Select(s => (byte)s).ToList();
				query = query.Where(o => o.Status.HasValue && byteStatuses.Contains(o.Status.Value));
			}
			// 如果 Statuses 為空，則不加任何 Where 條件，即查詢全部狀態 (預設行為已包含)

			// A3. 金額區間
			if (criteria.MinAmount.HasValue)
				query = query.Where(o => o.FinalAmount >= criteria.MinAmount.Value);
			if (criteria.MaxAmount.HasValue)
				query = query.Where(o => o.FinalAmount <= criteria.MaxAmount.Value);

			// A4 & A5. 日期篩選
			if (criteria.StartDate.HasValue)
			{
				query = criteria.DateDimension switch
				{
					2 => query.Where(o => o.PaymentDate >= criteria.StartDate.Value),
					3 => query.Where(o => o.CompletedAt >= criteria.StartDate.Value),
					4 => query.Where(o => o.ReturnRequests.OrderByDescending(r => r.CreatedAt).FirstOrDefault().CreatedAt >= criteria.StartDate.Value),
					5 => query.Where(o => o.ReturnRequests.OrderByDescending(r => r.UpdatedAt).FirstOrDefault().UpdatedAt >= criteria.StartDate.Value),
					_ => query.Where(o => o.CreatedAt >= criteria.StartDate.Value)
				};
			}
			if (criteria.EndDate.HasValue)
			{
				query = criteria.DateDimension switch
				{
					2 => query.Where(o => o.PaymentDate <= criteria.EndDate.Value),
					3 => query.Where(o => o.CompletedAt <= criteria.EndDate.Value),
					4 => query.Where(o => o.ReturnRequests.OrderByDescending(r => r.CreatedAt).FirstOrDefault().CreatedAt <= criteria.EndDate.Value),
					5 => query.Where(o => o.ReturnRequests.OrderByDescending(r => r.UpdatedAt).FirstOrDefault().UpdatedAt <= criteria.EndDate.Value),
					_ => query.Where(o => o.CreatedAt <= criteria.EndDate.Value)
				};
			}

			// A6. 商店過濾
			if (criteria.StoreId.HasValue)
				query = query.Where(o => o.StoreId == criteria.StoreId.Value);
			if (!string.IsNullOrEmpty(criteria.StoreName))
				query = query.Where(o => o.Store != null && o.Store.StoreName.Contains(criteria.StoreName));

			// C. 庫存狀態篩選 (1=充足, 2=不足)
			// 注意：這是一個較複雜的篩選，需檢查訂單內的所有商品規格
			if (criteria.StockStatus.HasValue)
			{
				if (criteria.StockStatus == 1) // 充足：所有品項庫存 >= 訂購數
				{
					query = query.Where(o => o.OrderDetails.All(od => 
						_context.ProductVariants.Any(v => v.Id == od.VariantId && v.Stock >= od.Quantity)));
				}
				else if (criteria.StockStatus == 2) // 不足：至少有一項庫存 < 訂購數
				{
					query = query.Where(o => o.OrderDetails.Any(od => 
						_context.ProductVariants.Any(v => v.Id == od.VariantId && v.Stock < od.Quantity)));
				}
			}

			// B. 動態排序
			query = criteria.SortBy switch
			{
				"TotalAmount" => criteria.IsDescending ? query.OrderByDescending(o => o.FinalAmount) : query.OrderBy(o => o.FinalAmount),
				"Status" => criteria.IsDescending ? query.OrderByDescending(o => o.Status) : query.OrderBy(o => o.Status),
				"MemberName" => criteria.IsDescending ? query.OrderByDescending(o => o.User.MemberProfile.FullName) : query.OrderBy(o => o.User.MemberProfile.FullName),
				"ReturnRequestDate" => criteria.IsDescending ? query.OrderByDescending(o => o.ReturnRequests.OrderByDescending(r => r.CreatedAt).FirstOrDefault().CreatedAt) : query.OrderBy(o => o.ReturnRequests.OrderByDescending(r => r.CreatedAt).FirstOrDefault().CreatedAt),
				"RefundDate" => criteria.IsDescending ? query.OrderByDescending(o => o.ReturnRequests.OrderByDescending(r => r.UpdatedAt).FirstOrDefault().UpdatedAt) : query.OrderBy(o => o.ReturnRequests.OrderByDescending(r => r.UpdatedAt).FirstOrDefault().UpdatedAt),
				"StockStatus" => criteria.IsDescending 
					? query.OrderByDescending(o => o.OrderDetails.Count(od => _context.ProductVariants.Any(v => v.Id == od.VariantId && v.Stock < od.Quantity)))
					: query.OrderBy(o => o.OrderDetails.Count(od => _context.ProductVariants.Any(v => v.Id == od.VariantId && v.Stock < od.Quantity))),
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
					StoreName = o.Store != null ? o.Store.StoreName : "平台",
					ReturnRequestCreatedAt = o.ReturnRequests?.OrderByDescending(r => r.CreatedAt).FirstOrDefault()?.CreatedAt,
					RefundDate = o.ReturnRequests?.OrderByDescending(r => r.UpdatedAt).FirstOrDefault()?.UpdatedAt
				};

				if (isShipmentWorkstation && o.OrderDetails != null)
				{
					dto.ItemsSummary = string.Join(", ", o.OrderDetails.Select(od => $"{od.ProductName} x{od.Quantity}"));
					if (o.PaymentDate.HasValue)
					{
						// 避免伺服器時區差異導致負數
						var now = DateTime.Now;
						var diff = now > o.PaymentDate.Value ? now - o.PaymentDate.Value : TimeSpan.Zero;
						
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

			// 依據 Enum: 3=已完成 (Completed)
			var currentNetRevenue = currentOrders.Where(o => o.Status == 3).Sum(o => o.FinalAmount);
			var prevNetRevenue = prevOrders.Where(o => o.Status == 3).Sum(o => o.FinalAmount);

			var currentTotalOrders = currentOrders.Count(o => o.Status > 0);
			var prevTotalOrders = prevOrders.Count(o => o.Status > 0);

			var currentReturns = currentOrders.Count(o => o.Status == 4 || o.Status == 5 || o.Status == 6);
			var prevReturns = prevOrders.Count(o => o.Status == 4 || o.Status == 5 || o.Status == 6);

			var currentOrderIds = currentOrders.Select(o => o.Id).ToList();
			var prevOrderIds = prevOrders.Select(o => o.Id).ToList();

			var currentItemsSold = currentOrderIds.Any() ? await _context.OrderDetails.Where(od => currentOrderIds.Contains(od.OrderId)).SumAsync(od => od.Quantity) : 0;
			var prevItemsSold = prevOrderIds.Any() ? await _context.OrderDetails.Where(od => prevOrderIds.Contains(od.OrderId)).SumAsync(od => od.Quantity) : 0;

			// 顧客行為 (新註冊會員) - 排除管理員 (RoleId = 1)
			var currentNewMembers = await _context.Users.CountAsync(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate && u.RoleId != 1);
			var prevNewMembers = await _context.Users.CountAsync(u => u.CreatedAt >= prevStartDate && u.CreatedAt <= prevEndDate && u.RoleId != 1);

			// 本期下單過的不重複會員與回購會員
			var curUserGroups = currentOrders.Where(o => o.Status > 0).GroupBy(o => o.UserId).Select(g => new { UserId = g.Key, Count = g.Count() }).ToList();
			var preUserGroups = prevOrders.Where(o => o.Status > 0).GroupBy(o => o.UserId).Select(g => new { UserId = g.Key, Count = g.Count() }).ToList();

			// 營運效率 (出貨時長) - 狀態 >= 2 (運送中或已完成) 且有付款與完成時間
			var curShipped = currentOrders.Where(o => o.Status >= 2 && o.PaymentDate.HasValue && o.CompletedAt.HasValue).ToList();
			var preShipped = prevOrders.Where(o => o.Status >= 2 && o.PaymentDate.HasValue && o.CompletedAt.HasValue).ToList();

			return new DashboardKpiRawDataDto
			{
				NetRevenue = currentNetRevenue,
				PrevNetRevenue = prevNetRevenue,
				TotalOrders = currentTotalOrders,
				PrevTotalOrders = prevTotalOrders,
				ReturnOrders = currentReturns,
				PrevReturnOrders = prevReturns,
				TotalItemsSold = currentItemsSold,
				PrevTotalItemsSold = prevItemsSold,
				PendingShipmentCount = await query.CountAsync(o => o.Status == 1),
				PendingRefundCount = await _context.ReturnRequests.CountAsync(rr => rr.Status == 0),
				LowStockProductCount = await _context.ProductVariants.AsNoTracking().CountAsync(v => 
					v.IsDeleted != true && 
					v.Product != null && 
					v.Product.IsDeleted != true && 
					(!storeId.HasValue || v.Product.StoreId == storeId.Value) &&
					(v.Stock ?? 0) <= (v.SafetyStock ?? 0)),

				NewMemberCount = currentNewMembers,
				PrevNewMemberCount = prevNewMembers,
				UniqueMemberCount = curUserGroups.Count,
				PrevUniqueMemberCount = preUserGroups.Count,
				RepeatMemberCount = curUserGroups.Count(m => m.Count > 1),
				PrevRepeatMemberCount = preUserGroups.Count(m => m.Count > 1),

				TotalFulfillmentTicks = (double)curShipped.Sum(o => (o.CompletedAt.Value - o.PaymentDate.Value).Ticks),
				PrevTotalFulfillmentTicks = (double)preShipped.Sum(o => (o.CompletedAt.Value - o.PaymentDate.Value).Ticks),
				ShippedOrderCount = curShipped.Count,
				PrevShippedOrderCount = preShipped.Count
			};
		}

		public async Task<ApexChartDataDto> GetCategoryCompositionBarChartAsync(int? storeId, DateTime startDate, DateTime endDate)
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

		public async Task<ApexChartDataDto> GetCategoryCompositionPieChartAsync(int? storeId, DateTime startDate, DateTime endDate)
		{
			// 需求：改為抓取全站「主類別」構成 (商品數量)
			var query = _context.Products
				.Include(p => p.Category)
				.ThenInclude(c => c.Parent)
				.AsNoTracking();

			if (storeId.HasValue) query = query.Where(p => p.StoreId == storeId.Value);

			var groupedData = await query
				.Select(p => new
				{
					ParentCategoryName = p.Category.ParentId == null ? p.Category.Name : p.Category.Parent.Name
				})
				.GroupBy(x => x.ParentCategoryName)
				.Select(g => new { CategoryName = g.Key ?? "未分類", ProductCount = g.Count() })
				.OrderByDescending(x => x.ProductCount)
				.ToListAsync();

			var dto = new ApexChartDataDto();
			var seriesData = new List<decimal>();

			// TOP 10 策略：前 10 名保留，其餘歸類為「其他」
			var top10 = groupedData.Take(10).ToList();
			var others = groupedData.Skip(10).ToList();

			foreach (var item in top10)
			{
				dto.Labels.Add(item.CategoryName);
				seriesData.Add(item.ProductCount);
			}

			if (others.Any())
			{
				dto.Labels.Add("其他");
				seriesData.Add(others.Sum(x => x.ProductCount));
			}

			dto.Series.Add(new ChartSeriesDto { Name = "類別構成比例", Data = seriesData });
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

			// 生成從 startDate 到 endDate 的月份標籤 (確保每個月份都有點)
			DateTime current = new DateTime(startDate.Year, startDate.Month, 1);
			// 避免無限迴圈，設定安全邊界
			int safetyCount = 0;
			while (current <= endDate && safetyCount < 100)
			{
				dto.Labels.Add($"{current.Year}/{current.Month:D2}");
				var monthData = monthlyData.FirstOrDefault(m => m.Year == current.Year && m.Month == current.Month);
				seriesData.Add(monthData != null ? monthData.Revenue : 0);
				current = current.AddMonths(1);
				safetyCount++;
			}

			dto.Series.Add(new ChartSeriesDto { Name = "營收", Data = seriesData });
			return dto;
		}

		public async Task<List<TopProductSalesDto>> GetTopSellingCategoriesAsync(int? storeId, DateTime startDate, DateTime endDate, string orderBy)
		{
			// 需求變更：顯示「主類別」排行，而非熱銷商品
			var query = _context.OrderDetails
				.Include(od => od.Order)
				.Include(od => od.Product)
					.ThenInclude(p => p.Category)
						.ThenInclude(c => c.Parent)
				.Where(od => od.Order.CreatedAt >= startDate && od.Order.CreatedAt <= endDate && od.Order.Status == 3);

			if (storeId.HasValue) query = query.Where(od => od.Order.StoreId == storeId.Value);

			var groupedQuery = query
				.Select(od => new
				{
					ParentCategoryName = od.Product.Category.ParentId == null ? od.Product.Category.Name : od.Product.Category.Parent.Name,
					Quantity = od.Quantity,
					Revenue = (od.Price ?? 0) * od.Quantity
				})
				.GroupBy(x => x.ParentCategoryName)
				.Select(g => new TopProductSalesDto
				{
					CategoryName = g.Key ?? "未分類",
					ProductName = g.Key ?? "未分類", // 這裡為了與前端相容，ProductName 存放類別名稱
					SalesVolume = g.Sum(x => x.Quantity),
					SalesRevenue = g.Sum(x => x.Revenue)
				});

			return orderBy.ToLower() == "volume"
				? await groupedQuery.OrderByDescending(x => x.SalesVolume).Take(10).ToListAsync()
				: await groupedQuery.OrderByDescending(x => x.SalesRevenue).Take(10).ToListAsync();
		}

		public async Task<ApexChartDataDto> GetCategoryContributionAsync(int? storeId, DateTime startDate, DateTime endDate)
		{
			// 需求：改為抓取「主類別」貢獻度
			var query = _context.OrderDetails
				.Include(od => od.Order)
				.Include(od => od.Product)
				.ThenInclude(p => p.Category)
				.ThenInclude(c => c.Parent)
				.Where(od => od.Order.CreatedAt >= startDate && od.Order.CreatedAt <= endDate && od.Order.Status == 3);

			if (storeId.HasValue) query = query.Where(od => od.Order.StoreId == storeId.Value);

			var groupedData = await query
				.Select(od => new
				{
					ParentCategoryName = od.Product.Category.ParentId == null ? od.Product.Category.Name : od.Product.Category.Parent.Name,
					Revenue = (od.Price ?? 0) * od.Quantity
				})
				.GroupBy(x => x.ParentCategoryName)
				.Select(g => new { CategoryName = g.Key ?? "未分類", Revenue = g.Sum(x => x.Revenue) })
				.OrderByDescending(x => x.Revenue)
				.ToListAsync();

			var dto = new ApexChartDataDto();
			var seriesData = new List<decimal>();

			// TOP 10 策略：前 10 名保留，其餘歸類為「其他」
			var top10 = groupedData.Take(10).ToList();
			var others = groupedData.Skip(10).ToList();

			foreach (var item in top10)
			{
				dto.Labels.Add(item.CategoryName);
				seriesData.Add(item.Revenue);
			}

			if (others.Any())
			{
				dto.Labels.Add("其他");
				seriesData.Add(others.Sum(x => x.Revenue));
			}

			dto.Series.Add(new ChartSeriesDto { Name = "營收佔比", Data = seriesData });
			return dto;
		}

		public async Task<ApexChartDataDto> GetCategoryDetailAsync(int? storeId, DateTime startDate, DateTime endDate, string type, string categoryName)
		{
			var dto = new ApexChartDataDto();
			var seriesData = new List<decimal>();

			if (type == "Composition")
			{
				// 處理「類別構成」的下鑽：顯示子類別下的商品數量
				var query = _context.Products
					.Include(p => p.Category)
					.ThenInclude(c => c.Parent)
					.AsNoTracking();

				if (storeId.HasValue) query = query.Where(p => p.StoreId == storeId.Value);

				if (categoryName == "其他")
				{
					var groupedData = await query
						.Select(p => new
						{
							ParentCategoryName = p.Category.ParentId == null ? p.Category.Name : p.Category.Parent.Name,
							CategoryName = p.Category.Name
						})
						.GroupBy(x => x.ParentCategoryName)
						.Select(g => new { CategoryName = g.Key ?? "未分類", Count = g.Count() })
						.OrderByDescending(x => x.Count)
						.Skip(10)
						.ToListAsync();

					foreach (var item in groupedData)
					{
						dto.Labels.Add(item.CategoryName);
						seriesData.Add(item.Count);
					}
				}
				else
				{
					var groupedData = await query
						.Where(p => (p.Category.ParentId == null ? p.Category.Name : p.Category.Parent.Name) == categoryName)
						.GroupBy(p => p.Category.Name)
						.Select(g => new { CategoryName = g.Key ?? "未分類", Count = g.Count() })
						.OrderByDescending(x => x.Count)
						.ToListAsync();

					foreach (var item in groupedData)
					{
						dto.Labels.Add(item.CategoryName);
						seriesData.Add(item.Count);
					}
				}
				dto.Series.Add(new ChartSeriesDto { Name = "商品數量", Data = seriesData });
			}
			else
			{
				// 原有銷售/營收邏輯
				var query = _context.OrderDetails
					.Include(od => od.Order)
					.Include(od => od.Product)
					.ThenInclude(p => p.Category)
					.ThenInclude(c => c.Parent)
					.Where(od => od.Order.CreatedAt >= startDate && od.Order.CreatedAt <= endDate && od.Order.Status == 3);

				if (storeId.HasValue) query = query.Where(od => od.Order.StoreId == storeId.Value);

				if (categoryName == "其他")
				{
					var groupedData = await query
						.Select(od => new
						{
							ParentCategoryName = od.Product.Category.ParentId == null ? od.Product.Category.Name : od.Product.Category.Parent.Name,
							Quantity = od.Quantity,
							Revenue = (od.Price ?? 0) * od.Quantity
						})
						.GroupBy(x => x.ParentCategoryName)
						.Select(g => new { 
							CategoryName = g.Key ?? "未分類", 
							Sales = (decimal)g.Sum(x => x.Quantity),
							Revenue = g.Sum(x => x.Revenue)
						})
						.OrderByDescending(x => type == "Sales" ? x.Sales : x.Revenue)
						.Skip(10)
						.ToListAsync();

					foreach (var item in groupedData)
					{
						dto.Labels.Add(item.CategoryName);
						seriesData.Add(type == "Sales" ? item.Sales : item.Revenue);
					}
				}
				else
				{
					var groupedData = await query
						.Where(od => (od.Product.Category.ParentId == null ? od.Product.Category.Name : od.Product.Category.Parent.Name) == categoryName)
						.GroupBy(od => od.Product.Category.Name)
						.Select(g => new { 
							CategoryName = g.Key ?? "未分類", 
							Sales = (decimal)g.Sum(od => od.Quantity),
							Revenue = g.Sum(od => (od.Price ?? 0) * od.Quantity)
						})
						.OrderByDescending(x => type == "Sales" ? x.Sales : x.Revenue)
						.ToListAsync();

					foreach (var item in groupedData)
					{
						dto.Labels.Add(item.CategoryName);
						seriesData.Add(type == "Sales" ? item.Sales : item.Revenue);
					}
				}
				dto.Series.Add(new ChartSeriesDto { Name = type == "Sales" ? "銷售量" : "營收額", Data = seriesData });
			}

			return dto;
		}

		public async Task<List<CategoryMonthlyRevenueDto>> GetCategoryMonthlyRevenueAsync(int? storeId, DateTime startDate, DateTime endDate)
		{
			var query = _context.OrderDetails
				.Include(od => od.Order)
				.Include(od => od.Product)
				.ThenInclude(p => p.Category)
				.ThenInclude(c => c.Parent)
				.Where(od => od.Order.CreatedAt >= startDate && od.Order.CreatedAt <= endDate && od.Order.Status == 3);

			if (storeId.HasValue) query = query.Where(od => od.Order.StoreId == storeId.Value);

			return await query
				.Select(od => new
				{
					ParentCategoryName = od.Product.Category.ParentId == null ? od.Product.Category.Name : od.Product.Category.Parent.Name,
					Month = od.Order.CreatedAt.Value.Month,
					Revenue = (od.Price ?? 0) * od.Quantity
				})
				.GroupBy(x => new { x.ParentCategoryName, x.Month })
				.Select(g => new CategoryMonthlyRevenueDto
				{
					CategoryName = g.Key.ParentCategoryName ?? "未分類",
					Month = g.Key.Month,
					Revenue = g.Sum(x => x.Revenue)
				})
				.ToListAsync();
		}
	}
}
