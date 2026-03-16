using ISpanShop.Models.DTOs.Admins;
using ISpanShop.Models.DTOs.Members;
using ISpanShop.Models.DTOs.Common;
using ISpanShop.Repositories.Admins;
using ISpanShop.Repositories.Members;
using System.Text.RegularExpressions;
using BCrypt.Net;

namespace ISpanShop.Services.Admins;

/// <summary>
/// 管理員服務實現 - 處理管理員相關業務邏輯
/// </summary>
public class AdminService : IAdminService
{
	private readonly IAdminRepository _adminRepository;
	private readonly ILoginHistoryRepository _loginHistoryRepository;

	public AdminService(IAdminRepository adminRepository, ILoginHistoryRepository loginHistoryRepository)
	{
		_adminRepository = adminRepository ?? throw new ArgumentNullException(nameof(adminRepository));
		_loginHistoryRepository = loginHistoryRepository ?? throw new ArgumentNullException(nameof(loginHistoryRepository));
	}

	public IEnumerable<AdminDto> GetAllAdmins(AdminCriteria criteria = null) //新增取得所有管理員（含實際權限)
	{
		var admins = _adminRepository.GetAllAdmins(criteria).ToList();
		foreach (var admin in admins)
		{
			if (admin.AdminLevelId.HasValue)
			{
				admin.ActualPermissions = _adminRepository
					.GetPermissionsByAdminLevel(admin.AdminLevelId.Value)
					.ToList();
			}
			else
			{
				admin.ActualPermissions = new List<PermissionDto>();
			}
		}
		return admins;
	}

	public PagedResult<AdminDto> SearchPaged(AdminCriteria criteria)
	{
		var admins = _adminRepository.SearchPaged(criteria, out int totalCount).ToList();
		foreach (var admin in admins)
		{
			if (admin.AdminLevelId.HasValue)
			{
				admin.ActualPermissions = _adminRepository
					.GetPermissionsByAdminLevel(admin.AdminLevelId.Value)
					.ToList();
			}
			else
			{
				admin.ActualPermissions = new List<PermissionDto>();
			}
		}

		return PagedResult<AdminDto>.Create(admins, totalCount, criteria.PageNumber, criteria.PageSize);
	}

	public AdminDto? GetAdminById(int adminId)
	{
		return _adminRepository.GetAdminById(adminId);
	}

	
	public IEnumerable<AdminLevelDto> GetSelectableAdminLevels()
	{
		return _adminRepository.GetSelectableAdminLevels();
	}



	public (bool IsSuccess, string Message) DeactivateAdmin(int userId, int currentUserId)
	{
		try
		{
			// 1. userId != currentUserId（不可停用自己）
			if (userId == currentUserId)
			{
				return (false, "無法停用自己的帳號");
			}

			// 2. 若目標是超級管理員，檢查超級管理員數量 > 1
			var targetAdmin = _adminRepository.GetAdminById(userId);
			if (targetAdmin?.AdminLevelId == 1)
			{
				int superAdminCount = _adminRepository.GetSuperAdminCount();
				if (superAdminCount <= 1)
				{
					return (false, "至少需保留一位超級管理員");
				}
			}

			// 3. 呼叫 Repository.DeactivateAdmin
			bool success = _adminRepository.DeactivateAdmin(userId);

			if (success)
			{
				return (true, "管理員已停用");
			}
			else
			{
				return (false, "停用管理員失敗");
			}
		}
		catch (Exception ex)
		{
			return (false, $"發生錯誤: {ex.Message}");
		}
	}

	public (bool IsSuccess, string Message) ResetAdminPassword(AdminResetPasswordDto dto)
	{
		try
		{
			// 1. 密碼檢查
			if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 8)
			{
				return (false, "臨時密碼至少需 8 個字元");
			}

			// 2. Hash 新密碼
			string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

			// 3. 呼叫 Repository.ResetPassword()
			bool success = _adminRepository.ResetPassword(dto.UserId, passwordHash);

			return success
				? (true, "管理員密碼已重設，該員工下次登入需修改密碼")
				: (false, "重設密碼失敗");
		}
		catch (Exception ex)
		{
			return (false, $"發生錯誤: {ex.Message}");
		}
	}

	public AdminDto? VerifyLogin(string account, string password, string? ipAddress)
	{
		try
		{
			// 1. 呼叫 GetAdminByAccount(account)
			var admin = _adminRepository.GetAdminByAccount(account);

			// 2. 帳號不存在 → 也要紀錄失敗 (UserId 為 null)
			if (admin == null)
			{
				_loginHistoryRepository.Add(new LoginHistoryDto
				{
					UserId = null,
					AttemptedAccount = account,
					LoginTime = DateTime.Now,
					Ipaddress = ipAddress ?? "Unknown",
					IsSuccess = false
				});
				return null;
			}

			// 3. 檢查停權狀態
			if (admin.IsBlacklisted)
			{
				_loginHistoryRepository.Add(new LoginHistoryDto
				{
					UserId = admin.UserId,
					AttemptedAccount = account,
					LoginTime = DateTime.Now,
					Ipaddress = ipAddress ?? "Unknown",
					IsSuccess = false
				});
				throw new InvalidOperationException("ACCOUNT_BLACKLISTED");
			}

			// 4. BCrypt.Verify(password, admin.PasswordHash)
			bool isSuccessful = BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash);

			// 紀錄登入歷史
			_loginHistoryRepository.Add(new LoginHistoryDto
			{
				UserId = admin.UserId,
				AttemptedAccount = account,
				LoginTime = DateTime.Now,
				Ipaddress = ipAddress ?? "Unknown",
				IsSuccess = isSuccessful
			});

			if (!isSuccessful)
			{
				// 5. 驗證失敗 → 回傳 null
				return null;
			}

			// 4. 驗證成功 → 回傳 AdminDto（含 IsFirstLogin）
			return admin;
		}
		catch (InvalidOperationException)
		{
			throw;
		}
		catch (Exception)
		{
			return null;
		}
	}

	public (bool IsSuccess, string Message) ChangePassword(AdminChangePasswordDto dto)
	{
		try
		{
			// 1. NewPassword == ConfirmPassword（由 DataAnnotation 在 Model 驗證，此為備用檢查）
			if (dto.NewPassword != dto.ConfirmPassword)
			{
				return (false, "兩次密碼輸入不一致");
			}

			// 2. 新密碼至少 8 碼且含英文與數字
			if (dto.NewPassword.Length < 8)
			{
				return (false, "新密碼至少需 8 個字元");
			}

			if (!Regex.IsMatch(dto.NewPassword, @"[a-zA-Z]"))
			{
				return (false, "新密碼必須包含英文字母");
			}

			if (!Regex.IsMatch(dto.NewPassword, @"[0-9]"))
			{
				return (false, "新密碼必須包含數字");
			}

			// 3. Hash 新密碼
			string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

			// 4. 呼叫 Repository.ChangePassword()
			bool changeSuccess = _adminRepository.ChangePassword(dto.UserId, newPasswordHash);

			if (!changeSuccess)
			{
				return (false, "變更密碼失敗");
			}

			// 5. 呼叫 Repository.SetFirstLoginComplete()
			_adminRepository.SetFirstLoginComplete(dto.UserId);

			return (true, "密碼變更成功");
		}
		catch (Exception ex)
		{
			return (false, $"發生錯誤: {ex.Message}");
		}
	}

	public bool UpdateAdminRole(int adminId, int roleId, int currentAdminId)
	{
		if (adminId == currentAdminId)
		{
			throw new InvalidOperationException("無法修改您自己的角色。請聯繫其他管理員協助。");
		}
		return _adminRepository.UpdateAdminRole(adminId, roleId);
	}

	public IEnumerable<AdminLevelDto> GetAllAdminLevels() // 取得所有身分（含預設權限）
	{
		var levels = _adminRepository.GetAllAdminLevels().ToList();
		foreach (var level in levels)
		{
			level.DefaultPermissions = _adminRepository
				.GetPermissionsByAdminLevel(level.AdminLevelId)
				.ToList();
		}
		return levels;
	}

	public IEnumerable<PermissionDto> GetAllPermissions()
	{
		return _adminRepository.GetAllPermissions();
	}

	public string GetNextAccount()
	{
		int seq = _adminRepository.GetNextAdminSequence();
		return $"ADM{seq:D3}";
	}

	public (bool IsSuccess, string Message) CreateAdmin(AdminCreateDto dto)
	{
		try
		{
			// 1. 不可指派超級管理員身分
			if (dto.AdminLevelId == 1)
				return (false, "無法直接指派超級管理員身分");

			// 2. 自動生成帳號
			int seq = _adminRepository.GetNextAdminSequence();
			string account = $"ADM{seq:D3}";
			string email = $"{account}@ispan.com";

			// 3. 確認帳號不重複
			if (_adminRepository.IsAccountExists(account))
				return (false, "帳號已存在，請稍後重試");

			// 4. Hash 密碼
			string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

			// 5. 新增
			bool success = _adminRepository.CreateAdmin(
				account, email, passwordHash, roleId: 1, dto.AdminLevelId);

			return success
				? (true, "管理員新增成功")
				: (false, "新增管理員失敗");
		}
		catch (Exception ex)
		{
			return (false, $"發生錯誤: {ex.Message}");
		}
	}

	public (bool IsSuccess, string Message) UpdateAdmin(AdminUpdateDto dto)
	{
		try
		{
			// 1. 不可升級為超級管理員
			if (dto.AdminLevelId == 1)
				return (false, "無法將管理員升級為超級管理員");

			bool success = _adminRepository.UpdateAdminLevel(dto.UserId, dto.AdminLevelId, dto.IsBlacklisted);

			return success
				? (true, "管理員資訊更新成功")
				: (false, "更新失敗，請確認管理員是否存在");
		}
		catch (Exception ex)
		{
			return (false, $"發生錯誤: {ex.Message}");
		}
	}

	public (bool IsSuccess, string Message) CreateAdminLevel(AdminLevelCreateDto dto)
	{
		try
		{
			// 1. 名稱不可為空
			if (string.IsNullOrWhiteSpace(dto.LevelName))
				return (false, "身分名稱不可為空");

			bool success = _adminRepository.CreateAdminLevel(dto);

			return success
				? (true, "身分新增成功")
				: (false, "新增身分失敗");
		}
		catch (Exception ex)
		{
			return (false, $"發生錯誤: {ex.Message}");
		}
	}

	public (bool IsSuccess, string Message) UpdateAdminLevelConfig(AdminLevelUpdateDto dto)
	{
		try
		{
			// 1. 系統預設身分不可修改
			if (dto.AdminLevelId == 1)
				return (false, "系統預設身分不可修改");

			bool success = _adminRepository.UpdateAdminLevelConfig(dto);

			return success
				? (true, "身分設定更新成功")
				: (false, "更新身分設定失敗");
		}
		catch (Exception ex)
		{
			return (false, $"發生錯誤: {ex.Message}");
		}
	}

	public (bool IsSuccess, string Message) DeleteAdminLevel(int adminLevelId)
	{
		try
		{
			// 1. 系統預設身分不可刪除
			if (adminLevelId == 1)
				return (false, "系統預設身分不可刪除");

			// 2. 有管理員綁定此身分則拒絕
			if (_adminRepository.HasAdminsBoundToLevel(adminLevelId))
				return (false, "尚有管理員綁定此身分，請先解除綁定");

			bool success = _adminRepository.DeleteAdminLevel(adminLevelId);

			return success
				? (true, "身分刪除成功")
				: (false, "刪除身分失敗");
		}
		catch (Exception ex) 
		{
			return (false, $"發生錯誤: {ex.Message}");
		}
		}

	public AdminLoginClaimsDto GetAdminWithPermissions(int adminId)
	{
		return _adminRepository.GetAdminWithPermissions(adminId);
	}

	
}