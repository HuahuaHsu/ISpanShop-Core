using ISpanShop.Models.EfModels;
using ISpanShop.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace ISpanShop.Repositories.Implementations
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
					// [修正] 根據你的檔案，屬性名稱是 Level
					.ThenInclude(mp => mp.Level)
				// 防止透過 URL ID 存取管理員
				.FirstOrDefault(u => u.Id == id && u.Role.RoleName != "SuperAdmin" && u.Role.RoleName != "Admin");
		}

		public IEnumerable<User> Search(string keyword, string status)
		{
			var query = _context.Users
				.AsNoTracking()
				.Include(u => u.Role)
				.Include(u => u.MemberProfile)
					// [修正] 根據你的檔案，屬性名稱是 Level
					.ThenInclude(mp => mp.Level)
				.Where(u => u.Role.RoleName != "SuperAdmin" && u.Role.RoleName != "Admin");

			if (!string.IsNullOrEmpty(keyword))
			{
				query = query.Where(u =>
					u.Account.Contains(keyword) ||
					u.Email.Contains(keyword) ||
					// [修正] 直接檢查 MemberProfile 屬性 (EF Core 會自動處理 null 檢查)
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

		public void Update(User user)
		{
			_context.Users.Update(user);
			_context.SaveChanges();
		}
	}
}