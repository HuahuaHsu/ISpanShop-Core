using ISpanShop.Models.DTOs.Members;
using ISpanShop.Models.DTOs.Common;
using ISpanShop.Models.EfModels;
using ISpanShop.Repositories.Members;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ISpanShop.Services.Members
{
	/// <summary>
	/// 會員服務實現 - 處理會員相關業務邏輯
	/// </summary>
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

		public void UpdateMemberProfile(UpdateMemberProfileDto dto)
		{
			var userInDb = _repo.GetById(dto.Id);
			if (userInDb == null) throw new Exception("找不到該會員");

			// 更新基本資訊 (Email, Account)
			userInDb.Email = dto.Email;
			
			// 更新詳細資料 (MemberProfile)
			if (userInDb.MemberProfile != null)
			{
				userInDb.MemberProfile.FullName = dto.FullName;
				userInDb.MemberProfile.PhoneNumber = dto.PhoneNumber;
				userInDb.MemberProfile.Gender = dto.Gender;
				userInDb.MemberProfile.DateOfBirth = dto.Birthday;
			}

			_repo.Update(userInDb);
		}

		public void UpdateMemberProfile(MemberDto dto)
		{
			var userInDb = _repo.GetById(dto.Id);
			if (userInDb == null) throw new Exception("找不到該會員");

			// 更新 Users 表中的基本資訊
			userInDb.Email = dto.Email;
			userInDb.IsBlacklisted = dto.IsBlacklisted;

			// 更新 MemberProfiles 表
			if (userInDb.MemberProfile != null)
			{
				userInDb.MemberProfile.FullName = dto.FullName;
				userInDb.MemberProfile.PhoneNumber = dto.PhoneNumber;
				userInDb.MemberProfile.Gender = dto.Gender;
				userInDb.MemberProfile.DateOfBirth = dto.Birthday;
			}

			// 更新 Addresses 表中的預設地址
			var defaultAddress = userInDb.Addresses.FirstOrDefault(a => a.IsDefault == true)
								  ?? userInDb.Addresses.FirstOrDefault();
			if (defaultAddress != null)
			{
				defaultAddress.City = dto.City;
				defaultAddress.Region = dto.Region;
				defaultAddress.Street = dto.Address; // 對應 Street 欄位
			}

			_repo.Update(userInDb);
		}

		private MemberDto MapToDto(User u, List<MembershipLevel> levels)
		{
			var profile = u.MemberProfile;
			var address = u.Addresses.FirstOrDefault(a => a.IsDefault == true)
						  ?? u.Addresses.FirstOrDefault();

			return new MemberDto
			{
				Id = u.Id,
				Account = u.Account,
				Email = u.Email,
				IsBlacklisted = u.IsBlacklisted ?? false,
				IsSeller = profile?.IsSeller ?? false,
				RoleName = u.Role?.RoleName,

				FullName = profile?.FullName ?? "未設定",
				PhoneNumber = profile?.PhoneNumber ?? "未設定",
				Gender = profile?.Gender,
				Birthday = profile?.DateOfBirth,
				PointBalance = profile?.PointBalance ?? 0,
				TotalSpending = profile?.TotalSpending ?? 0,

				// 根據累計消費金額動態計算等級名稱
				LevelName = GetLevelNameBySpending(profile?.TotalSpending, levels),

				// 如果有預設頭像 URL 生成邏輯
				AvatarUrl = profile?.FullName != null 
					? $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(profile.FullName)}&background=random&color=fff&size=128"
					: null,

				City = address?.City ?? "",
				Region = address?.Region ?? "",
				Address = address?.Street ?? ""
			};
		}

		private string GetLevelNameBySpending(decimal? spending, List<MembershipLevel> levels)
		{
			if (levels == null || !levels.Any()) return "一般會員";

			// 確保 spending 有值，若無則視為 0
			decimal currentSpending = spending ?? 0;

			// 找符合條件且 MinSpending 最高的等級
			var matchedLevel = levels
				.Where(l => currentSpending >= l.MinSpending)
				.OrderByDescending(l => l.MinSpending)
				.FirstOrDefault();

			return matchedLevel?.LevelName ?? "一般會員";
		}
	}
}
