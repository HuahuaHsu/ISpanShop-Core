using ISpanShop.Models.EfModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Repositories.Interfaces
{
	public interface IMemberRepository
	{
		// 搜尋功能 (包含關鍵字與狀態篩選)
		IEnumerable<User> Search(string keyword, string status);

		// 取得單一會員
		User GetById(int id);

		// 更新
		void Update(User user);
	}
}
	

