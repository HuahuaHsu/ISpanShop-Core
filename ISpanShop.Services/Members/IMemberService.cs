using ISpanShop.Models.DTOs.Members;
using ISpanShop.Models.DTOs.Common;
using ISpanShop.Models.EfModels;
using System.Collections.Generic;

namespace ISpanShop.Services.Members
{
	/// <summary>
	/// 會員服務介面 - 處理會員相關業務邏輯
	/// </summary>
	public interface IMemberService
	{
		/// <summary>
		/// 搜尋會員（支援關鍵字、狀態篩選）
		/// </summary>
		/// <param name="keyword">關鍵字（帳號、姓名、Email、電話）</param>
		/// <param name="status">狀態（normal, blocked）</param>
		/// <returns>會員 DTO 列表</returns>
		IEnumerable<MemberDto> Search(string keyword, string status);

		/// <summary>
		/// 分頁進階搜尋
		/// </summary>
		PagedResult<MemberDto> SearchPaged(MemberCriteria criteria);

		/// <summary>
		/// 取得所有會員等級
		/// </summary>
		IEnumerable<MembershipLevel> GetAllMembershipLevels();

		/// <summary>
		/// 根據 ID 取得會員詳細資料
		/// </summary>
		/// <param name="id">會員 ID</param>
		/// <returns>會員 DTO</returns>
		MemberDto GetMemberById(int id);

		/// <summary>
		/// 更新會員狀態（如黑名單）
		/// </summary>
		/// <param name="dto">會員 DTO</param>
		void UpdateMemberStatus(MemberDto dto);

		/// <summary>
		/// 會員自行更新詳細資料 (專用 DTO)
		/// </summary>
		/// <param name="dto">更新 DTO</param>
		void UpdateMemberProfile(UpdateMemberProfileDto dto);
	}
}
