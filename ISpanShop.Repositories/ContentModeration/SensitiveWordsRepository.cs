using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ISpanShop.Models.EfModels;

namespace ISpanShop.Repositories.ContentModeration
{
    public class SensitiveWordRepository : ISensitiveWordRepository
    {
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

        public async Task CreateAsync(SensitiveWord sensitiveWord)
        {
            _context.SensitiveWords.Add(sensitiveWord);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SensitiveWord sensitiveWord)
        {
            _context.SensitiveWords.Update(sensitiveWord);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(SensitiveWord sensitiveWord)
        {
            _context.SensitiveWords.Remove(sensitiveWord);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 回傳所有啟用中 (IsActive = true) 的敏感字字串清單
        /// </summary>
        public async Task<IEnumerable<string>> GetAllWordsAsync()
        {
            return await _context.SensitiveWords
                .Where(w => w.IsActive == true)
                .Select(w => w.Word)
                .ToListAsync();
        }
	
        // 用來判斷「高風險」分類的關鍵字
        private static readonly string[] _highRiskCategoryKeywords =
            { "高風險", "危險", "違禁", "嚴重", "色情", "暴力", "毒品", "武器", "詐騙", "仿冒", "賭博" };

        /// <inheritdoc/>
        public async Task<(List<string> HighRisk, List<string> LowRisk)> GetActiveWordsGroupedAsync()
        {
            var words = await _context.SensitiveWords
                .AsNoTracking()
                .Where(w => w.IsActive == true)
                .Include(w => w.CategoryNavigation)
                .ToListAsync();

            bool IsHighRisk(SensitiveWord w)
            {
                var catName = w.CategoryNavigation?.Name ?? w.Category ?? string.Empty;
                return _highRiskCategoryKeywords.Any(k => catName.Contains(k));
            }

            var highRisk = words.Where(IsHighRisk).Select(w => w.Word).ToList();
            var lowRisk  = words.Where(w => !IsHighRisk(w)).Select(w => w.Word).ToList();

            // Fallback：若分類全落在同一組，則對半拆分
            if (highRisk.Count == 0 && lowRisk.Count > 0)
            {
                int half = Math.Max(1, lowRisk.Count / 2);
                highRisk = lowRisk.Take(half).ToList();
                lowRisk  = lowRisk.Skip(half).ToList();
            }

            if (lowRisk.Count == 0 && highRisk.Count > 0)
                lowRisk = new List<string>(highRisk);

            return (highRisk, lowRisk);
        }
    }
}
