using ISpanShop.Models.DTOs.Common;
using ISpanShop.Models.EfModels;
using ISpanShop.MVC.Areas.Admin.Models.Points;
using ISpanShop.Services.Payments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISpanShop.MVC.Areas.Admin.Controllers.Points
{
    [Area("Admin")]
    [Route("Admin/Points")]
    public class PointsController : AdminBaseController
    {
        private readonly PointService _pointService;
        private readonly ISpanShopDBContext _context;

        public PointsController(PointService pointService, ISpanShopDBContext context)
        {
            _pointService = pointService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string keyword = "", int? userId = null, int page = 1, int pageSize = 10)
        {
            var pagedResult = await _pointService.GetPagedHistoryAsync(keyword, userId, page, pageSize);

            var viewModel = new PointHistoryIndexVm
            {
                Keyword = keyword,
                UserId = userId,
                PagedResult = new PagedResult<PointHistoryItemVm>
                {
                    CurrentPage = pagedResult.CurrentPage,
                    PageSize = pagedResult.PageSize,
                    TotalCount = pagedResult.TotalCount,
                    TotalPages = pagedResult.TotalPages,
                    Data = pagedResult.Data.Select(ph => new PointHistoryItemVm
                    {
                        Id = ph.Id,
                        UserId = ph.UserId,
                        Account = ph.User?.Account ?? "未知",
                        FullName = ph.User?.MemberProfile?.FullName ?? "未設定",
                        OrderNumber = ph.OrderNumber,
                        ChangeAmount = ph.ChangeAmount,
                        BalanceAfter = ph.BalanceAfter,
                        Description = ph.Description,
                        CreatedAt = ph.CreatedAt
                    }).ToList()
                }
            };

            return View("~/Areas/Admin/Views/Points/Index.cshtml", viewModel);
        }

        [HttpPost("GenerateMockData")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateMockData()
        {
            var random = new Random();
            var users = await _context.Users
                .Include(u => u.MemberProfile)
                .Where(u => u.MemberProfile != null)
                .ToListAsync();

            if (!users.Any())
            {
                TempData["Error"] = "資料庫中沒有會員資料，無法產生假資料。";
                return RedirectToAction(nameof(Index));
            }

            var descriptions = new[] { "訂單完成贈點", "每日登入獎勵", "活動補點", "退貨點數扣除", "商品評論獎勵", "首購禮包" };
            
            for (int i = 0; i < 50; i++)
            {
                var user = users[random.Next(users.Count)];
                var changeAmount = random.Next(-100, 500);
                if (changeAmount == 0) changeAmount = 50;

                // 避免餘額變成負數
                int currentBalance = user.MemberProfile.PointBalance ?? 0;
                if (currentBalance + changeAmount < 0)
                {
                    changeAmount = Math.Abs(changeAmount); // 轉正值
                }

                user.MemberProfile.PointBalance = currentBalance + changeAmount;
                user.MemberProfile.UpdatedAt = DateTime.Now;

                var history = new PointHistory
                {
                    UserId = user.Id,
                    OrderNumber = changeAmount > 0 && random.Next(2) == 0 ? null : "ORD" + DateTime.Now.ToString("yyyyMMdd") + random.Next(1000, 9999),
                    ChangeAmount = changeAmount,
                    BalanceAfter = user.MemberProfile.PointBalance ?? 0,
                    Description = descriptions[random.Next(descriptions.Length)],
                    CreatedAt = DateTime.Now.AddMinutes(-random.Next(1, 10000))
                };

                _context.PointHistories.Add(history);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "成功產生 50 筆點數紀錄假資料！";

            return RedirectToAction(nameof(Index));
        }
    }
}
