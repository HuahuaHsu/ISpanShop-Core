using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ISpanShop.Models.EfModels; // 你的 EF Models 命名空間

namespace ISpanShop.Repositories
{
	// 1. 介面
	public interface ISensitiveWordsRepository
	{
		// 取得所有敏感字清單
		Task<List<string>> GetAllWordsAsync();
	}

	// 2. 實作
	public class SensitiveWordsRepository : ISensitiveWordsRepository
	{
		private readonly ISpanShopDBContext _context; // 替換成你真正的 DbContext 名稱

		public SensitiveWordsRepository(ISpanShopDBContext context)
		{
			_context = context;
		}

		public async Task<List<string>> GetAllWordsAsync()
		{
			// 假設你的 SensitiveWords 表裡面有一個欄位叫 Word 或 Keyword
			return await _context.SensitiveWords
				.Select(s => s.Word)
				.ToListAsync();
		}
	}
}
