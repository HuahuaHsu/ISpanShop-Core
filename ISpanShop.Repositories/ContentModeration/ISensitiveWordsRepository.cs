using ISpanShop.Models.EfModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Repositories.ContentModeration
{
	public interface ISensitiveWordRepository
	{
		// 取得所有敏感字列表 (通常會用 List 回傳)
		Task<List<SensitiveWord>> GetAllAsync();
		Task<IEnumerable<string>> GetAllWordsAsync();

		// 根據 ID 取得單筆敏感字 (用來做修改或刪除前的查詢)
		Task<SensitiveWord> GetByIdAsync(int id);

		// 新增敏感字
		Task CreateAsync(SensitiveWord sensitiveWord);

		// 更新敏感字 (例如更改分類或切換啟用狀態)
		Task UpdateAsync(SensitiveWord sensitiveWord);

		// 刪除敏感字
		Task DeleteAsync(SensitiveWord sensitiveWord);
	}
}
