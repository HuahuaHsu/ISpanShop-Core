using ISpanShop.Models.DTOs;
using System.Collections.Generic;

// 位置：ISpanShop.Repositories/Interfaces/IAdminRepository.cs

using ISpanShop.Models.DTOs;

namespace ISpanShop.Repositories.Interfaces
{
	/// <summary>
	/// 管理員資料存取介面
	/// </summary>
	public interface IAdminRepository
	{
		/// <summary>取得所有管理員（含角色名稱）</summary>
		IEnumerable<AdminDto> GetAllAdmins();

		/// <summary>依 ID 取得單一管理員</summary>
		AdminDto? GetAdminById(int adminId);

		/// <summary>更新管理員角色</summary>
		bool UpdateAdminRole(int adminId, int roleId);
	}
}