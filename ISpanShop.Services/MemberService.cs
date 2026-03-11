using ISpanShop.Models.DTOs;
using ISpanShop.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ISpanShop.Services
{
	public class MemberService
	{
		private readonly IMemberRepository _repo;

		public MemberService(IMemberRepository repo)
		{
			_repo = repo;
		}

		public IEnumerable<MemberDto> Search(string keyword, string status)
		{
			var users = _repo.Search(keyword, status);

			return users.Select(u => {
				var profile = u.MemberProfile; // 已經改單數了

				return new MemberDto
				{
					Id = u.Id,
					Account = u.Account,
					Email = u.Email,
					IsBlacklisted = u.IsBlacklisted ?? false,
					IsSeller = u.IsSeller ?? false,
					RoleName = u.Role.RoleName,

					FullName = profile?.FullName ?? "未設定",
					PhoneNumber = profile?.PhoneNumber ?? "未設定",
					PointBalance = profile?.PointBalance ?? 0,

					// [修正] 這裡改成單數 MembershipLevel (或 Level)
					LevelName = profile?.Level?.LevelName ?? "一般會員",

					AvatarUrl = $"https://ui-avatars.com/api/?name={profile?.FullName ?? u.Account}&background=random&color=fff"
				};
			});
		}

		public MemberDto GetMemberById(int id)
		{
			var user = _repo.GetById(id);
			if (user == null) return null;

			var profile = user.MemberProfile;
			var address = user.Addresses.FirstOrDefault(a => a.IsDefault == true)
						  ?? user.Addresses.FirstOrDefault();

			return new MemberDto
			{
				Id = user.Id,
				Account = user.Account,
				Email = user.Email,
				IsBlacklisted = user.IsBlacklisted ?? false,
				IsSeller = user.IsSeller ?? false,
				RoleName = user.Role.RoleName,

				FullName = profile?.FullName,
				PhoneNumber = profile?.PhoneNumber,
				PointBalance = profile?.PointBalance ?? 0,

				// [修正] 這裡改成單數 MembershipLevel
				LevelName = profile?.Level?.LevelName ?? "無等級",

				City = address?.City,
				Region = address?.Region,
				Address = address?.Street
			};
		}

		public void UpdateMemberStatus(MemberDto dto)
		{
			var userInDb = _repo.GetById(dto.Id);
			if (userInDb == null) throw new Exception("找不到該會員");

			userInDb.IsBlacklisted = dto.IsBlacklisted;
			_repo.Update(userInDb);
		}
	}
}