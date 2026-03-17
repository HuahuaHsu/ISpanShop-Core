using ISpanShop.Models.DTOs.ContentModeration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Services.ContentModeration;
	public interface ISensitiveWordService
	{
		/// <summary>回傳所有啟用中 (IsActive = true) 的敏感字字串清單，供審核引擎使用</summary>
		Task<List<string>> GetActiveWordListAsync();

		/// <summary>
		/// 回傳啟用中的敏感字依風險等級分組：
		/// HighRisk = 名稱或描述命中即自動退回；LowRisk = 僅出現一次需人工確認。
		/// </summary>
		Task<(List<string> HighRisk, List<string> LowRisk)> GetActiveWordsByRiskAsync();

		// 取得所有敏感字 (回傳 DTO)
		Task<List<SensitiveWordDto>> GetAllAsync();

		// 根據 ID 取得單筆敏感字 (回傳 DTO)
		Task<SensitiveWordDto> GetByIdAsync(int id);

		// 新增敏感字 (接收 DTO)
		Task CreateAsync(SensitiveWordDto dto);

		// 更新敏感字 (接收 DTO)
		Task UpdateAsync(SensitiveWordDto dto);

		// 刪除敏感字 (只要傳 ID 進來即可)
		Task DeleteAsync(int id);

		// [新功能] 自動偵測內容是否包含敏感字
		Task<bool> HasSensitiveWordAsync(string content);
	}