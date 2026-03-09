using ISpanShop.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Services
{
	public interface ISensitiveWordService
	{
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
	}
}
