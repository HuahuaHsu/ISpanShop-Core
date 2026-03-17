using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ISpanShop.Models.EfModels; // 你的 EF Models 命名空間

namespace ISpanShop.Repositories.ContentModeration
{
	public class SensitiveWordRepository : ISensitiveWordRepository
	{
		// 注入你的 DbContext
		private readonly ISpanShopDBContext _context;

		public SensitiveWordRepository(ISpanShopDBContext context)
		{
			_context = context;
		}

		// 實作：取得所有敏感字
		public async Task<List<SensitiveWord>> GetAllAsync()
		{
			// 將資料表轉換成 List，並用建立時間倒序排列（最新建立的在最上面）
			return await _context.SensitiveWords
				.Include(w => w.CategoryNavigation)
				.OrderByDescending(w => w.CreatedTime)
				.ToListAsync();
		}

		// 實作：根據 ID 找敏感字
		public async Task<SensitiveWord> GetByIdAsync(int id)
		{
			return await _context.SensitiveWords
				.Include(w => w.CategoryNavigation)
				.FirstOrDefaultAsync(w => w.Id == id);
		}

		// 實作：新增
		public async Task CreateAsync(SensitiveWord sensitiveWord)
		{
			_context.SensitiveWords.Add(sensitiveWord);
			await _context.SaveChangesAsync();
		}

		// 實作：更新
		public async Task UpdateAsync(SensitiveWord sensitiveWord)
		{
			_context.SensitiveWords.Update(sensitiveWord);
			await _context.SaveChangesAsync();
		}

		// 實作：刪除
		public async Task DeleteAsync(SensitiveWord sensitiveWord)
		{
			_context.SensitiveWords.Remove(sensitiveWord);
			await _context.SaveChangesAsync();
		}

		public async Task<IEnumerable<string>> GetAllWordsAsync()
		{
			// 取得所有「啟用中」的敏感字字串列表
			return await _context.SensitiveWords
				.Where(w => w.IsActive == true)
				.Select(w => w.Word)
				.ToListAsync();
		}
	}
}
