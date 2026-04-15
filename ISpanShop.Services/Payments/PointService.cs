using ISpanShop.Models.DTOs.Members;
using ISpanShop.Models.EfModels;
using ISpanShop.Repositories.Members;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISpanShop.Models.DTOs.Common;

namespace ISpanShop.Services.Payments;
	public class PointService
	{
		private readonly ISpanShopDBContext _context;
        private readonly IPointRepository _repo;

		public PointService(ISpanShopDBContext context, IPointRepository repo)
		{
			_context = context;
            _repo = repo;
		}

		/// <summary>
		/// 取得使用者目前的點數餘額
		/// </summary>
		public async Task<int> GetBalanceAsync(int userId)
		{
			var profile = await _context.MemberProfiles
				.FirstOrDefaultAsync(p => p.UserId == userId);
			return profile?.PointBalance ?? 0;
		}

		/// <summary>
		/// 執行點數異動 (包含檢查餘額與寫入 Log)
		/// </summary>
		public async Task<(bool IsSuccess, string Message)> UpdatePointsAsync(PointUpdateDTO dto)
		{
			using (var transaction = await _context.Database.BeginTransactionAsync())
			{
				try
				{
					var profile = await _context.MemberProfiles
						.FirstOrDefaultAsync(p => p.UserId == dto.UserId);

					if (profile == null) return (false, "找不到會員資料");

					// --- 修正 1: 處理 PointBalance 為 int? 的加減法 ---
					int currentBalance = profile.PointBalance ?? 0; // 如果是 null 則當作 0

					if (dto.ChangeAmount < 0 && (currentBalance + dto.ChangeAmount) < 0)
					{
						return (false, "點數不足，無法折抵");
					}

					// 更新餘額
					profile.PointBalance = currentBalance + dto.ChangeAmount;
					profile.UpdatedAt = DateTime.Now;

					// --- 修正 2: 建立歷史紀錄 ---
					var history = new PointHistory
					{
						UserId = dto.UserId,
						OrderNumber = dto.OrderNumber,
						ChangeAmount = dto.ChangeAmount,
						BalanceAfter = profile.PointBalance ?? 0, // 確保寫入歷史的是實體數值
						Description = dto.Description,
						CreatedAt = DateTime.Now
					};

					// --- 修正 3: 使用 DbContext 正確的 DbSet 名稱 (PointHistories) ---
					_context.PointHistories.Add(history);

					await _context.SaveChangesAsync();
					await transaction.CommitAsync();

					return (true, "點數更新成功");
				}
				catch (Exception ex)
				{
					await transaction.RollbackAsync();
					return (false, $"更新失敗: {ex.Message}");
				}
			}
		}

        /// <summary>
        /// 取得分頁的點數紀錄
        /// </summary>
		public async Task<PagedResult<PointHistory>> GetPagedHistoryAsync(string keyword, int? userId, int page, int pageSize)
        {
            var (items, totalCount) = await _repo.GetPagedPointHistoryAsync(keyword, userId, page, pageSize);

            return new PagedResult<PointHistory>
            {
                Data = items.ToList(),
                TotalCount = totalCount,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

		/// <summary>
		/// 全體會員點數批次異動
		/// </summary>
		public async Task<(bool IsSuccess, string Message, int AffectedCount)> BulkUpdateAllUsersPointsAsync(int changeAmount, string description, string orderNumber)
		{
			using (var transaction = await _context.Database.BeginTransactionAsync())
			{
				try
				{
					var profiles = await _context.MemberProfiles.ToListAsync();
					int affected = 0;
					var now = DateTime.Now;

					foreach (var profile in profiles)
					{
						int currentBalance = profile.PointBalance ?? 0;
						
						// 避免餘額小於 0 (如果是扣點)
						int newBalance = currentBalance + changeAmount;
						if (newBalance < 0) newBalance = 0;

						profile.PointBalance = newBalance;
						profile.UpdatedAt = now;

						var history = new PointHistory
						{
							UserId = profile.UserId,
							OrderNumber = orderNumber,
							ChangeAmount = changeAmount,
							BalanceAfter = newBalance,
							Description = description,
							CreatedAt = now
						};
						_context.PointHistories.Add(history);
						affected++;
					}

					await _context.SaveChangesAsync();
					await transaction.CommitAsync();
					return (true, $"成功為 {affected} 位會員更新點數", affected);
				}
				catch (Exception ex)
				{
					await transaction.RollbackAsync();
					return (false, $"批次更新失敗: {ex.Message}", 0);
				}
			}
		}
	}
