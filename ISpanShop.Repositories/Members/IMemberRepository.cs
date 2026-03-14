using ISpanShop.Models.EfModels;
using ISpanShop.Models.DTOs.Members;
using System.Collections.Generic;

namespace ISpanShop.Repositories.Members
{
	public interface IMemberRepository
	{
		// 搜尋功能 (包含關鍵字與狀態篩選)
		IEnumerable<User> Search(string keyword, string status);

		// 分頁與進階搜尋
		IEnumerable<User> SearchPaged(MemberCriteria criteria, out int totalCount);

		// 取得所有會員等級
		IEnumerable<MembershipLevel> GetAllLevels();

		// 取得單一會員
		User GetById(int id);

		// 更新
		void Update(User user);
	}
}
	

