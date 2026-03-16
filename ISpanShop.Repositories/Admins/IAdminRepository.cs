using ISpanShop.Models.DTOs.Admins;
using System.Collections.Generic;

namespace ISpanShop.Repositories.Admins
{
	
	public interface IAdminRepository
	{
		
		/// <summary>取得所有管理員資訊</summary>
		IEnumerable<AdminDto> GetAllAdmins(AdminCriteria criteria = null);

		/// <summary>分頁搜尋管理員</summary>
		IEnumerable<AdminDto> SearchPaged(AdminCriteria criteria, out int totalCount);

		/// <summary>由 ID 取得一個管理員</summary>
		AdminDto? GetAdminById(int adminId);

		/// <summary>更新管理員角色</summary>
		bool UpdateAdminRole(int adminId, int roleId);

		/// <summary>取得可選擇的管理員層級（排除超級管理員）</summary>
		IEnumerable<AdminLevelDto> GetSelectableAdminLevels();

		/// <summary>透過帳號取得管理員</summary>
		AdminDto? GetAdminByAccount(string account);

		/// <summary>取得下一個管理員序列號</summary>
		int GetNextAdminSequence();

		/// <summary>新增管理員</summary>
		bool CreateAdmin(string account, string email, string passwordHash, int roleId, int adminLevelId);

		/// <summary>停用管理員 (軟刪除)</summary>
		bool DeactivateAdmin(int userId);

		/// <summary>檢查帳號是否存在</summary>
		bool IsAccountExists(string account);

		/// <summary>變更密碼</summary>
		bool ChangePassword(int userId, string newPasswordHash);

		/// <summary>設定為完成首次登入</summary>
		bool SetFirstLoginComplete(int userId);

		/// <summary>取得超級管理員的數量</summary>
		int GetSuperAdminCount();

		IEnumerable<PermissionDto> GetPermissionsByAdminLevel(int adminLevelId);
		IEnumerable<AdminLevelDto> GetAllAdminLevels();
		bool UpdateAdminLevel(int userId, int adminLevelId, bool isBlacklisted);
		bool CreateAdminLevel(AdminLevelCreateDto dto);
		bool UpdateAdminLevelConfig(AdminLevelUpdateDto dto);
		bool DeleteAdminLevel(int adminLevelId);
		bool HasAdminsBoundToLevel(int adminLevelId);
		IEnumerable<PermissionDto> GetAllPermissions();

		/// <summary>重設密碼並強制下次登入修改</summary>
		bool ResetPassword(int userId, string passwordHash);

		/// <summary>取得管理員及其擁有的所有權限清單</summary>
		AdminLoginClaimsDto GetAdminWithPermissions(int adminId);
	}
}