using ISpanShop.Models.EfModels;
using ISpanShop.Models.DTOs.Members;
using ISpanShop.Repositories.Members;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ISpanShop.Repositories.Members.Implementations
{
	public class MemberRepository : IMemberRepository
	{
		private readonly ISpanShopDBContext _context;

		public MemberRepository(ISpanShopDBContext context)
		{
			_context = context;
		}

		public User GetById(int id)
		{
			return _context.Users
				.AsNoTracking()
				.Include(u => u.Role)
				.Include(u => u.Addresses)
				.Include(u => u.MemberProfile)
					.ThenInclude(mp => mp.Level)
				.FirstOrDefault(u => u.Id == id && u.Role.RoleName != "SuperAdmin" && u.Role.RoleName != "Admin");
		}

		public IEnumerable<User> Search(string keyword, string status)
		{
			var query = _context.Users
				.AsNoTracking()
				.Include(u => u.Role)
				.Include(u => u.MemberProfile)
					.ThenInclude(mp => mp.Level)
				.Where(u => u.Role.RoleName != "SuperAdmin" && u.Role.RoleName != "Admin");

			if (!string.IsNullOrEmpty(keyword))
			{
				query = query.Where(u =>
					u.Account.Contains(keyword) ||
					u.Email.Contains(keyword) ||
					(u.MemberProfile != null &&
					 (u.MemberProfile.FullName.Contains(keyword) || u.MemberProfile.PhoneNumber.Contains(keyword)))
				);
			}

			if (!string.IsNullOrEmpty(status))
			{
				if (status == "normal")
				{
					query = query.Where(u => u.IsBlacklisted != true);
				}
				else if (status == "blocked")
				{
					query = query.Where(u => u.IsBlacklisted == true);
				}
			}

			return query.ToList();
		}

		public IEnumerable<MembershipLevel> GetAllLevels()
		{
			return _context.MembershipLevels.AsNoTracking().ToList();
		}

		public IEnumerable<User> SearchPaged(MemberCriteria criteria, out int totalCount)
		{
			var query = _context.Users
				.AsNoTracking()
				.Include(u => u.Role)
				.Include(u => u.MemberProfile)
					.ThenInclude(mp => mp.Level)
				.Where(u => u.Role.RoleName != "SuperAdmin" && u.Role.RoleName != "Admin");

			// 關鍵字搜尋 (姓名、帳號、Email、電話)
			if (!string.IsNullOrEmpty(criteria.Keyword))
			{
				query = query.Where(u =>
					u.Account.Contains(criteria.Keyword) ||
					u.Email.Contains(criteria.Keyword) ||
					(u.MemberProfile != null &&
					 (u.MemberProfile.FullName.Contains(criteria.Keyword) || u.MemberProfile.PhoneNumber.Contains(criteria.Keyword)))
				);
			}

			// 狀態篩選
			if (!string.IsNullOrEmpty(criteria.Status))
			{
				if (criteria.Status == "normal")
				{
					query = query.Where(u => u.IsBlacklisted != true);
				}
				else if (criteria.Status == "blocked")
				{
					query = query.Where(u => u.IsBlacklisted == true);
				}
			}

			// 身分篩選 (買家/賣家)
			if (criteria.RoleId.HasValue)
			{
				if (criteria.RoleId == 1) // 買家
				{
					query = query.Where(u => u.MemberProfile != null && u.MemberProfile.IsSeller == false);
				}
				else if (criteria.RoleId == 2) // 賣家
				{
					query = query.Where(u => u.MemberProfile != null && u.MemberProfile.IsSeller == true);
				}
			}

			// 等級篩選 (改為動態門檻篩選)
			if (criteria.LevelId.HasValue)
			{
				var allLevels = _context.MembershipLevels.OrderBy(l => l.MinSpending).ToList();
				var targetLevel = allLevels.FirstOrDefault(l => l.Id == criteria.LevelId.Value);

				if (targetLevel != null)
				{
					decimal min = targetLevel.MinSpending;
					var nextLevel = allLevels.FirstOrDefault(l => l.MinSpending > min);

					if (nextLevel != null)
					{
						query = query.Where(u => u.MemberProfile != null &&
												 u.MemberProfile.TotalSpending >= min &&
												 u.MemberProfile.TotalSpending < nextLevel.MinSpending);
					}
					else
					{
						query = query.Where(u => u.MemberProfile != null &&
												 u.MemberProfile.TotalSpending >= min);
					}
				}
			}

			totalCount = query.Count();

			// 排序
			query = ApplySorting(query, criteria.SortColumn, criteria.IsAscending);

			return query
				.Skip((criteria.PageNumber - 1) * criteria.PageSize)
				.Take(criteria.PageSize)
				.ToList();
		}

		private IQueryable<User> ApplySorting(IQueryable<User> query, string sortColumn, bool isAscending)
		{
			switch (sortColumn?.ToLower())
			{
				case "account":
					return isAscending ? query.OrderBy(u => u.Account) : query.OrderByDescending(u => u.Account);
				case "fullname":
				case "name":
					return isAscending ? query.OrderBy(u => u.MemberProfile.FullName) : query.OrderByDescending(u => u.MemberProfile.FullName);
				case "email":
					return isAscending ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email);
				case "levelname":
				case "level":
					return isAscending ? query.OrderBy(u => u.MemberProfile.TotalSpending) : query.OrderByDescending(u => u.MemberProfile.TotalSpending);
				case "pointbalance":
				case "points":
					return isAscending ? query.OrderBy(u => u.MemberProfile.PointBalance) : query.OrderByDescending(u => u.MemberProfile.PointBalance);
				case "isblacklisted":
				case "status":
					return isAscending ? query.OrderBy(u => u.IsBlacklisted) : query.OrderByDescending(u => u.IsBlacklisted);
				case "createdat":
					return isAscending ? query.OrderBy(u => u.CreatedAt) : query.OrderByDescending(u => u.CreatedAt);
				default:
					return isAscending ? query.OrderBy(u => u.Id) : query.OrderByDescending(u => u.Id);
			}
		}

		public void Update(User user)
		{
			_context.Users.Update(user);
			_context.SaveChanges();
		}
	}
}
