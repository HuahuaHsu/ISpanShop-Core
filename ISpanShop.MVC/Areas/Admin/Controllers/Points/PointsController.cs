using ISpanShop.Models.DTOs.Common;
using ISpanShop.Models.EfModels;
using ISpanShop.MVC.Areas.Admin.Models.Points;
using ISpanShop.Services;
using ISpanShop.Services.Members;
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
        private readonly IMemberService _memberService;
        private readonly ISpanShopDBContext _context;

        public PointsController(PointService pointService, IMemberService memberService, ISpanShopDBContext context)
        {
            _pointService = pointService;
            _memberService = memberService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string keyword = "", int? userId = null, int page = 1, int pageSize = 10)
		{//點數歷史清單 (讀取)
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

        [HttpGet("SearchMembers")]
        public IActionResult SearchMembers(string term)//動態搜尋會員
		{
            var members = _memberService.Search(term, null)
                .Select(m => new { 
                    id = m.Id, 
                    text = $"{m.FullName} ({m.Account}) - 餘額: {m.PointBalance}" 
                })
                .Take(10);
            return Json(members);
        }

        [HttpPost("UpdatePoints")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePoints(PointUpdateVm vm)//單筆點數調整 (寫入)
		{
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "資料格式不正確，請檢查輸入內容。";
                return RedirectToAction(nameof(Index));
            }

            // 根據異動類型調整正負號
            if ((vm.UpdateType == "扣點" || vm.UpdateType == "到期歸零") && vm.ChangeAmount > 0)
            {
                vm.ChangeAmount = -vm.ChangeAmount;
            }
            else if (vm.UpdateType == "加點" && vm.ChangeAmount < 0)
            {
                vm.ChangeAmount = Math.Abs(vm.ChangeAmount);
            }

            // 取得當前操作者 (管理員)
            var operatorName = User.Identity?.Name ?? "System";

            // 格式化描述，包含所有 Audit Trail 欄位
            var formattedDescription = $"[{vm.UpdateType}] 原因: {vm.Reason} | 操作人: {operatorName}";
            if (!string.IsNullOrEmpty(vm.Remark))
            {
                formattedDescription += $" | 備註: {vm.Remark}";
            }

            var dto = new ISpanShop.Models.DTOs.Members.PointUpdateDTO
            {
                UserId = vm.UserId,
                ChangeAmount = vm.ChangeAmount,
                OrderNumber = vm.OrderNumber ?? "MANUAL-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                Description = formattedDescription
            };

            var result = await _pointService.UpdatePointsAsync(dto);

            if (result.IsSuccess)
            {
                TempData["Success"] = "點數更新成功！";
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("BulkUpdatePoints")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkUpdatePoints(BulkPointUpdateVm vm)//全體批次更新
		{
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "資料格式不正確。";
                return RedirectToAction(nameof(Index));
            }

            // 根據異動類型調整正負號
            if ((vm.UpdateType == "扣點" || vm.UpdateType == "到期歸零") && vm.ChangeAmount > 0)
            {
                vm.ChangeAmount = -vm.ChangeAmount;
            }

            var operatorName = User.Identity?.Name ?? "System";
            var formattedDescription = $"[全體-{vm.UpdateType}] 原因: {vm.Reason} | 操作人: {operatorName}";
            if (!string.IsNullOrEmpty(vm.Remark))
            {
                formattedDescription += $" | 備註: {vm.Remark}";
            }

            var orderNumber = vm.OrderNumber ?? "BULK-" + DateTime.Now.ToString("yyyyMMddHHmmss");

            var result = await _pointService.BulkUpdateAllUsersPointsAsync(vm.ChangeAmount, formattedDescription, orderNumber);

            if (result.IsSuccess)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("GenerateMockData")]
        [ValidateAntiForgeryToken] //防止跨站請求偽造攻擊
		public async Task<IActionResult> GenerateMockData()//依訂單產生假資料 (開發專用)
		{
            // 防呆：先確認訂單表有足夠資料
            var orders = await _context.Orders
                .Include(o => o.User)
                    .ThenInclude(u => u.MemberProfile)
                .Where(o => o.User.MemberProfile != null)
                .ToListAsync();

            if (orders.Count < 5)
            {
                TempData["Error"] = $"訂單資料不足（目前僅 {orders.Count} 筆），請先在訂單管理中產生假資料後再操作。";
                return RedirectToAction(nameof(Index));
            }

            var random = new Random();

            // 只取付款中/已付款/已完成/已取消的訂單（排除完全空的狀態）
            var eligibleOrders = orders
                .Where(o => o.Status != null)
                .OrderBy(_ => random.Next())
                .Take(50)
                .ToList();

            int generatedCount = 0;

            foreach (var order in eligibleOrders)
            {
                var profile = order.User?.MemberProfile;
                if (profile == null) continue;

                int changeAmount;
                string description;

                if (order.Status == 4) // 已取消
                {
                    if ((order.PointDiscount ?? 0) > 0)
                    {
                        // 訂單取消時退還原本折抵的點數
                        changeAmount = order.PointDiscount.Value;
                        description = "訂單取消退還折抵點數";
                    }
                    else
                    {
                        continue; // 已取消且無折抵點數，跳過
                    }
                }
                else if (order.Status == 1 || order.Status == 2 || order.Status == 3)
                {
                    // 已付款或已完成：依訂單金額 1% 贈點（最少 10 點）
                    changeAmount = Math.Max(10, (int)(order.FinalAmount * 0.01m));
                    description = order.Status == 3 ? "訂單完成贈點" : "訂單付款贈點";
                }
                else
                {
                    continue; // 待付款等其他狀態不產生點數
                }

                int currentBalance = profile.PointBalance ?? 0;
                if (currentBalance + changeAmount < 0)
                    changeAmount = Math.Abs(changeAmount);

                profile.PointBalance = currentBalance + changeAmount;
                profile.UpdatedAt = DateTime.Now;

                _context.PointHistories.Add(new PointHistory
                {
                    UserId = order.UserId,
                    OrderNumber = order.OrderNumber,
                    ChangeAmount = changeAmount,
                    BalanceAfter = profile.PointBalance.Value,
                    Description = description,
                    CreatedAt = order.CreatedAt ?? DateTime.Now
                });

                generatedCount++;
            }

            if (generatedCount == 0)
            {
                TempData["Error"] = "現有訂單均不符合產生點數紀錄的條件（訂單需為已付款、已完成，或已取消且有使用折抵點數）。";
                return RedirectToAction(nameof(Index));
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"成功依訂單資料產生 {generatedCount} 筆點數紀錄！";

            return RedirectToAction(nameof(Index));
        }
    }
}
