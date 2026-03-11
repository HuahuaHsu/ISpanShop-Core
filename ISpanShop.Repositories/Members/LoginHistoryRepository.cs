using ISpanShop.Models.DTOs.Members;
using ISpanShop.Models.DTOs.Common;
using ISpanShop.Models.EfModels;
using ISpanShop.Repositories.Members;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ISpanShop.Repositories.Members
{
	/// <summary>
	/// 登入紀錄 Repository 實作 - 負責資料庫查詢
	/// </summary>
	public class LoginHistoryRepository : ILoginHistoryRepository
	{
		private readonly ISpanShopDBContext _context;

		public LoginHistoryRepository(ISpanShopDBContext context)
		{
			_context = context;
		}

		public IEnumerable<LoginHistoryDto> GetAll()
		{
			return _context.LoginHistories
				.AsNoTracking()
				.Include(lh => lh.User)
				.OrderByDescending(lh => lh.LoginTime)
				.Select(lh => new LoginHistoryDto
				{
					Id = lh.Id,
					UserId = lh.UserId,
					UserAccount = lh.User.Account,
					LoginTime = lh.LoginTime,
					Ipaddress = lh.Ipaddress,
					IsSuccessful = lh.IsSuccessful
				})
				.ToList();
		}

		public PagedResult<LoginHistoryDto> SearchPaged(LoginHistoryCriteria criteria)
		{
			// 驗證條件
			criteria.Validate();

			// 建立查詢的初始 IQueryable
			IQueryable<LoginHistory> query = _context.LoginHistories
				.AsNoTracking()
				.Include(lh => lh.User);

			// 應用搜尋條件 - Keyword (帳號或 IP)
			if (!string.IsNullOrWhiteSpace(criteria.Keyword))
			{
				var keyword = criteria.Keyword.Trim();
				query = query.Where(lh =>
					lh.User.Account.Contains(keyword) ||
					lh.Ipaddress.Contains(keyword)
				);
			}

			// 應用篩選條件 - IsSuccessful
			if (criteria.IsSuccessful.HasValue)
			{
				query = query.Where(lh => lh.IsSuccessful == criteria.IsSuccessful.Value);
			}

			// 取得篩選後的總筆數
			int totalCount = query.Count();

			// 應用排序
			query = ApplySorting(query, criteria.SortColumn, criteria.IsAscending);

			// 應用分頁
			var items = query
				.Skip((criteria.PageNumber - 1) * criteria.PageSize)
				.Take(criteria.PageSize)
				.Select(lh => new LoginHistoryDto
				{
					Id = lh.Id,
					UserId = lh.UserId,
					UserAccount = lh.User.Account,
					LoginTime = lh.LoginTime,
					Ipaddress = lh.Ipaddress,
					IsSuccessful = lh.IsSuccessful
				})
				.ToList();

			// 返回分頁結果
			return PagedResult<LoginHistoryDto>.Create(
				items,
				totalCount,
				criteria.PageNumber,
				criteria.PageSize
			);
		}

		/// <summary>
		/// 根據欄位名稱動態應用排序
		/// </summary>
		private IQueryable<LoginHistory> ApplySorting(
			IQueryable<LoginHistory> query,
			string sortColumn,
			bool isAscending)
		{
			// 預設排序欄位為 LoginTime
			if (string.IsNullOrWhiteSpace(sortColumn))
			{
				sortColumn = "LoginTime";
			}

			// 根據欄位名稱進行排序
			switch (sortColumn.ToLower())
			{
				case "id":
					return isAscending
						? query.OrderBy(lh => lh.Id)
						: query.OrderByDescending(lh => lh.Id);

				case "useraccount":
				case "account":
					return isAscending
						? query.OrderBy(lh => lh.User.Account)
						: query.OrderByDescending(lh => lh.User.Account);

				case "logintime":
					return isAscending
						? query.OrderBy(lh => lh.LoginTime)
						: query.OrderByDescending(lh => lh.LoginTime);

				case "ipaddress":
				case "ip":
					return isAscending
						? query.OrderBy(lh => lh.Ipaddress)
						: query.OrderByDescending(lh => lh.Ipaddress);

				case "issuccessful":
				case "status":
					return isAscending
						? query.OrderBy(lh => lh.IsSuccessful)
						: query.OrderByDescending(lh => lh.IsSuccessful);

				default:
					// 預設按登入時間降序
					return query.OrderByDescending(lh => lh.LoginTime);
			}
		}

		public int GetCount()
		{
			return _context.LoginHistories.Count();
		}

		public void AddRange(List<LoginHistoryDto> loginHistories)
		{
			if (loginHistories == null || !loginHistories.Any())
			{
				return;
			}

			// 將 DTO 轉換為 Entity
			var entities = loginHistories.Select(dto => new LoginHistory
			{
				UserId = dto.UserId,
				LoginTime = dto.LoginTime,
				Ipaddress = dto.Ipaddress,
				IsSuccessful = dto.IsSuccessful
			}).ToList();

			// 批次新增
			_context.LoginHistories.AddRange(entities);
			_context.SaveChanges();
		}
	}
}

