using ISpanShop.Models.DTOs.Admins;
using ISpanShop.Models.DTOs.Common;

namespace ISpanShop.Services.Admins
{
	public interface IAdminService
	{
		// 查詢
		IEnumerable<AdminDto> GetAllAdmins(AdminCriteria criteria = null);
		PagedResult<AdminDto> SearchPaged(AdminCriteria criteria);
		AdminDto? GetAdminById(int adminId);
		IEnumerable<AdminLevelDto> GetAllAdminLevels();
		IEnumerable<PermissionDto> GetAllPermissions();
		IEnumerable<AdminLevelDto> GetSelectableAdminLevels();

		// 管理員帳號
		(bool IsSuccess, string Message) CreateAdmin(AdminCreateDto dto);
		(bool IsSuccess, string Message) UpdateAdmin(AdminUpdateDto dto);
		(bool IsSuccess, string Message) DeactivateAdmin(int userId, int currentUserId);
		(bool IsSuccess, string Message) ResetAdminPassword(AdminResetPasswordDto dto);

		// 身分管理
		(bool IsSuccess, string Message) CreateAdminLevel(AdminLevelCreateDto dto);
		(bool IsSuccess, string Message) UpdateAdminLevelConfig(AdminLevelUpdateDto dto);
		(bool IsSuccess, string Message) DeleteAdminLevel(int adminLevelId);

		// 原有方法
		AdminDto? VerifyLogin(string account, string password, string? ipAddress);
		(bool IsSuccess, string Message) ChangePassword(AdminChangePasswordDto dto);
		bool UpdateAdminRole(int adminId, int roleId, int currentAdminId);
		string GetNextAccount();

        /// <summary>取得管理員及其擁有的所有權限清單</summary>
        AdminPermissionDto GetAdminWithPermissions(int adminId);
	}
}