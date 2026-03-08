using ISpanShop.Models.DTOs;
using ISpanShop.Repositories.Interfaces;
using ISpanShop.Services.Interfaces;

namespace ISpanShop.Services
{
	/// <summary>
	/// 管理員服務實現 - 處理管理員相關業務邏輯
	/// </summary>
	public class AdminService : IAdminService
	{
		private readonly IAdminRepository _adminRepository;
		private readonly IAdminRoleRepository _roleRepository;

		public AdminService(IAdminRepository adminRepository, IAdminRoleRepository roleRepository)
		{
			_adminRepository = adminRepository ?? throw new ArgumentNullException(nameof(adminRepository));
			_roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
		}

		public IEnumerable<AdminDto> GetAllAdmins()
		{
			return _adminRepository.GetAllAdmins();
		}

		public IEnumerable<AdminPermissionDto> GetAllPermissions()
		{
			return _roleRepository.GetAllPermissions();
		}

		public bool UpdateAdminRole(int adminId, int roleId, int currentAdminId)
		{
			if (adminId == currentAdminId)
			{
				throw new InvalidOperationException("無法修改您自己的角色。請聯繫其他管理員協助。");
			}
			return _adminRepository.UpdateAdminRole(adminId, roleId);
		}
	}
}