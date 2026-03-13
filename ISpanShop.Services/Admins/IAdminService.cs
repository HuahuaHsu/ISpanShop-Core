using ISpanShop.Models.DTOs.Admins;

namespace ISpanShop.Services.Admins
{
	public interface IAdminService
	{
		// 查詢
		IEnumerable<AdminDto> GetAllAdmins();
		IEnumerable<AdminLevelDto> GetAllAdminLevels();
		IEnumerable<PermissionDto> GetAllPermissions();
		IEnumerable<AdminLevelDto> GetSelectableAdminLevels();

		// 管理員帳號
		(bool IsSuccess, string Message) CreateAdmin(AdminCreateDto dto);
		(bool IsSuccess, string Message) UpdateAdmin(AdminUpdateDto dto);
		(bool IsSuccess, string Message) DeactivateAdmin(int userId, int currentUserId);

		// 身分管理
		(bool IsSuccess, string Message) CreateAdminLevel(AdminLevelCreateDto dto);
		(bool IsSuccess, string Message) UpdateAdminLevelConfig(AdminLevelUpdateDto dto);
		(bool IsSuccess, string Message) DeleteAdminLevel(int adminLevelId);

		// 原有方法
		AdminDto? VerifyLogin(string account, string password);
		(bool IsSuccess, string Message) ChangePassword(AdminChangePasswordDto dto);
		bool UpdateAdminRole(int adminId, int roleId, int currentAdminId);
		
	}
}