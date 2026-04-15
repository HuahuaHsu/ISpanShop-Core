using ISpanShop.Models.DTOs.ContentModeration;
using ISpanShop.Models.EfModels;
using ISpanShop.Repositories.ContentModeration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Services.ContentModeration;
	public class SensitiveWordService : ISensitiveWordService
	{
		private readonly ISensitiveWordRepository _repo;

		public SensitiveWordService(ISensitiveWordRepository repo)
		{
			_repo = repo;
		}

		/// <inheritdoc/>
		public async Task<List<string>> GetActiveWordListAsync()
		{
			var words = await _repo.GetAllWordsAsync();
			return words.ToList();
		}

		/// <inheritdoc/>
		public async Task<(List<string> HighRisk, List<string> LowRisk)> GetActiveWordsByRiskAsync()
			=> await _repo.GetActiveWordsGroupedAsync();

		public async Task<List<SensitiveWordDto>> GetAllAsync()
		{
			var entities = await _repo.GetAllAsync();

			// 將 Entity 轉換成 DTO
			return entities.Select(e => new SensitiveWordDto
			{
				Id = e.Id,
				Word = e.Word,
				CategoryId = e.CategoryId,
				Category = e.CategoryNavigation?.Name ?? e.Category ?? "未分類",
				// 【修改這裡】如果 e.IsActive 是 null，就預設為 false
				IsActive = e.IsActive ?? false,
				// 【修改這裡】如果 e.CreatedTime 是 null，就給它一個基本時間 (或當下時間)
				CreatedTime = e.CreatedTime ?? DateTime.MinValue
			}).ToList();
		}

		public async Task<SensitiveWordDto> GetByIdAsync(int id)
		{
			var entity = await _repo.GetByIdAsync(id);
			if (entity == null) return null;

			return new SensitiveWordDto
			{
				Id = entity.Id,
				Word = entity.Word,
				CategoryId = entity.CategoryId,
				Category = entity.CategoryNavigation?.Name ?? entity.Category ?? "未分類",
				// 【修改這裡】同上，給予預設值防呆
				IsActive = entity.IsActive ?? false,
				CreatedTime = entity.CreatedTime ?? DateTime.MinValue
			};
		}

		public async Task CreateAsync(SensitiveWordDto dto)
		{
			var entity = new SensitiveWord
			{
				Word = dto.Word,
				CategoryId = dto.CategoryId,
				// dto.IsActive 是 bool，塞進 bool? 的 entity.IsActive 沒問題 (隱含轉換)
				IsActive = dto.IsActive,
				CreatedTime = DateTime.Now
			};

			await _repo.CreateAsync(entity);
		}

		public async Task UpdateAsync(SensitiveWordDto dto)
		{
			var entity = await _repo.GetByIdAsync(dto.Id);

			if (entity != null)
			{
				entity.Word = dto.Word;
				entity.CategoryId = dto.CategoryId;
				entity.IsActive = dto.IsActive;

				await _repo.UpdateAsync(entity);
			}
		}

		public async Task DeleteAsync(int id)
		{
			var entity = await _repo.GetByIdAsync(id);
			if (entity != null)
			{
				await _repo.DeleteAsync(entity);
			}
		}

		// [新功能實作] 自動偵測內容是否包含敏感字
		public async Task<bool> HasSensitiveWordAsync(string content)
		{
			if (string.IsNullOrWhiteSpace(content)) return false;

			// 取得所有啟用的敏感字 (例如：["廣告", "詐騙", "三字經"])
			var sensitiveWords = await _repo.GetAllWordsAsync();

			// 使用 Any 來檢查內容中是否包含任何一個字詞 (這裡可以未來視需求增加正規表示式檢查)
			return sensitiveWords.Any(word => content.Contains(word));
		}
	}