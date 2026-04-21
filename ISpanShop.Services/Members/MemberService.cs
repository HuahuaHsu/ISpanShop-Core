using ISpanShop.Models.DTOs.Members;
using ISpanShop.Models.DTOs.Common;
using ISpanShop.Models.EfModels;
using ISpanShop.Repositories.Members;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ISpanShop.Services.Members
{
	public class MemberService : IMemberService
	{
		private readonly IMemberRepository _repo;

		public MemberService(IMemberRepository repo)
		{
			_repo = repo ?? throw new ArgumentNullException(nameof(repo));
		}

		public IEnumerable<MemberDto> Search(string keyword, string status)
		{
			var users = _repo.Search(keyword, status);
			var levels = _repo.GetAllLevels().ToList();
			return users.Select(u => MapToDto(u, levels));
		}

		public PagedResult<MemberDto> SearchPaged(MemberCriteria criteria)
		{
			var users = _repo.SearchPaged(criteria, out int totalCount);
			var levels = _repo.GetAllLevels().ToList();
			return new PagedResult<MemberDto>
			{
				Data = users.Select(u => MapToDto(u, levels)).ToList(),
				TotalCount = totalCount,
				CurrentPage = criteria.PageNumber,
				PageSize = criteria.PageSize
			};
		}

		public IEnumerable<MembershipLevel> GetAllMembershipLevels()
		{
			return _repo.GetAllLevels();
		}

		public MemberDto GetMemberById(int id)
		{
			var user = _repo.GetById(id);
			if (user == null) return null;
			var levels = _repo.GetAllLevels().ToList();
			return MapToDto(user, levels);
		}

		public void UpdateMemberStatus(MemberDto dto)
		{
			var userInDb = _repo.GetById(dto.Id);
			if (userInDb == null) throw new Exception("找不到該會員");
			userInDb.IsBlacklisted = dto.IsBlacklisted;
			_repo.Update(userInDb);
		}

		// ── 前台會員自行更新個人資料 ──────────────────────────
		public void UpdateMemberProfile(UpdateMemberProfileDto dto)
		{
			var userInDb = _repo.GetById(dto.Id);
			if (userInDb == null) throw new Exception("找不到該會員");

			userInDb.Email = dto.Email;

			if (userInDb.MemberProfile != null)
			{
				userInDb.MemberProfile.FullName = dto.FullName;
				userInDb.MemberProfile.PhoneNumber = dto.PhoneNumber;
				userInDb.MemberProfile.AvatarUrl = dto.AvatarUrl;
				userInDb.MemberProfile.Gender = dto.Gender;
				userInDb.MemberProfile.DateOfBirth = dto.Birthday;
			}

			_repo.Update(userInDb);
		}

		// ── 後台管理員更新會員資料 ────────────────────────────
		public void UpdateMemberProfile(MemberDto dto)
		{
			var userInDb = _repo.GetById(dto.Id);
			if (userInDb == null) throw new Exception("找不到該會員");

			userInDb.Email = dto.Email;
			userInDb.IsBlacklisted = dto.IsBlacklisted;

			if (userInDb.MemberProfile != null)
			{
				userInDb.MemberProfile.FullName = dto.FullName;
				userInDb.MemberProfile.PhoneNumber = dto.PhoneNumber;
				userInDb.MemberProfile.AvatarUrl = dto.AvatarUrl;
				userInDb.MemberProfile.Gender = dto.Gender;
				userInDb.MemberProfile.DateOfBirth = dto.Birthday;
			}

			var defaultAddress = userInDb.Addresses.FirstOrDefault(a => a.IsDefault == true)
								 ?? userInDb.Addresses.FirstOrDefault();
			if (defaultAddress != null)
			{
				defaultAddress.City = dto.City;
				defaultAddress.Region = dto.Region;
				defaultAddress.Street = dto.Address;
			}

			_repo.Update(userInDb);
		}

		private MemberDto MapToDto(User u, List<MembershipLevel> levels)
		{
			var profile = u.MemberProfile;
			var address = u.Addresses.FirstOrDefault(a => a.IsDefault == true)
						  ?? u.Addresses.FirstOrDefault();

			string phoneNumber = profile?.PhoneNumber ?? "";
			string avatarUrl = !string.IsNullOrEmpty(profile?.AvatarUrl)
				? profile.AvatarUrl
				: profile?.FullName != null
					? $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(profile.FullName)}&background=random&color=fff&size=128"
					: null;

			return new MemberDto
			{
				Id = u.Id,
				Account = u.Account,
				Email = u.Email,
				IsBlacklisted = u.IsBlacklisted ?? false,
				IsSeller = profile?.IsSeller ?? false,
				RoleName = u.Role?.RoleName,
				FullName = profile?.FullName ?? "未設定",
				PhoneNumber = phoneNumber,
				Gender = profile?.Gender,
				Birthday = profile?.DateOfBirth,
				PointBalance = profile?.PointBalance ?? 0,
				TotalSpending = profile?.TotalSpending ?? 0,
				LevelName = GetLevelNameBySpending(profile?.TotalSpending, levels),
				AvatarUrl = avatarUrl,
				City = address?.City ?? "",
				Region = address?.Region ?? "",
				Address = address?.Street ?? ""
			};
		}

		private string GetLevelNameBySpending(decimal? spending, List<MembershipLevel> levels)
		{
			if (levels == null || !levels.Any()) return "一般會員";
			decimal currentSpending = spending ?? 0;
			var matchedLevel = levels
				.Where(l => currentSpending >= l.MinSpending)
				.OrderByDescending(l => l.MinSpending)
				.FirstOrDefault();
			return matchedLevel?.LevelName ?? "一般會員";
		}
	}
}