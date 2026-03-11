using ISpanShop.Models.DTOs.Members;
using ISpanShop.Models.DTOs.Common;
using System.Collections.Generic;

namespace ISpanShop.Repositories.Members
{
	/// <summary>
	/// 登入紀錄 Repository 介面
	/// </summary>
	public interface ILoginHistoryRepository
	{
		/// <summary>
		/// 取得所有登入紀錄（含使用者帳號，由新至舊排序）
		/// </summary>
		/// <returns>登入紀錄 DTO 列表</returns>
		IEnumerable<LoginHistoryDto> GetAll();

		/// <summary>
		/// 依條件進行搜尋、排序、分頁查詢
		/// </summary>
		/// <param name="criteria">查詢條件</param>
		/// <returns>分頁結果</returns>
		PagedResult<LoginHistoryDto> SearchPaged(LoginHistoryCriteria criteria);

		/// <summary>
		/// 取得登入紀錄總筆數
		/// </summary>
		/// <returns>總筆數</returns>
		int GetCount();

		/// <summary>
		/// 批次新增登入紀錄
		/// </summary>
		/// <param name="loginHistories">登入紀錄列表</param>
		void AddRange(List<LoginHistoryDto> loginHistories);
	}
}
