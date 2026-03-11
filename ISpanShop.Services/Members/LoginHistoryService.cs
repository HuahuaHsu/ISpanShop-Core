using ISpanShop.Models.DTOs.Members;
using ISpanShop.Models.DTOs.Common;
using ISpanShop.Repositories.Members;
using System.Collections.Generic;

namespace ISpanShop.Services.Members
{
	/// <summary>
	/// 登入紀錄服務實現 - 處理登入紀錄相關業務邏輯
	/// </summary>
	public class LoginHistoryService : ILoginHistoryService
	{
		private readonly ILoginHistoryRepository _loginHistoryRepository;

		public LoginHistoryService(ILoginHistoryRepository loginHistoryRepository)
		{
			_loginHistoryRepository = loginHistoryRepository ?? throw new ArgumentNullException(nameof(loginHistoryRepository));
		}

		public IEnumerable<LoginHistoryDto> GetAllLoginHistories()
		{
			return _loginHistoryRepository.GetAll();
		}

		public PagedResult<LoginHistoryDto> SearchPagedLoginHistories(LoginHistoryCriteria criteria)
		{
			return _loginHistoryRepository.SearchPaged(criteria);
		}
	}
}