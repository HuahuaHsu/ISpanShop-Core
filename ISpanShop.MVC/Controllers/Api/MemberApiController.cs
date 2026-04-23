using Microsoft.AspNetCore.Mvc;
using ISpanShop.Services.Payments;
using ISpanShop.Models.EfModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;

namespace ISpanShop.MVC.Controllers.Api
{
    [ApiController]
    [Authorize(AuthenticationSchemes = "FrontendJwt")]
    [Route("api/member")]
    public class MemberApiController : ControllerBase
    {
        private readonly PointService _pointService;
        private readonly ISpanShopDBContext _context;

        public MemberApiController(PointService pointService, ISpanShopDBContext context)
        {
            _pointService = pointService;
            _context = context;
        }

        [HttpGet("level-info")]
        public async Task<IActionResult> GetLevelInfo()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized(new { message = "未登入或 Token 已失效" });
            int userId = int.Parse(userIdStr);

            var user = await _context.Users
                .Include(u => u.MemberProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound(new { message = "找不到該使用者" });

            // 獲取所有等級規則
            var levels = await _context.MembershipLevels
                .OrderBy(l => l.MinSpending)
                .Select(l => new {
                    l.Id,
                    l.LevelName,
                    l.MinSpending,
                    l.DiscountRate
                })
                .ToListAsync();

            return Ok(new {
                userId = user.Id,
                totalSpending = user.MemberProfile?.TotalSpending ?? 0,
                currentLevelName = user.MemberProfile?.LevelId != null 
                    ? levels.FirstOrDefault(l => l.Id == user.MemberProfile.LevelId)?.LevelName 
                    : "一般會員",
                levels = levels
            });
        }

        [HttpGet("wallet-balance")]
        public async Task<IActionResult> GetWalletBalance()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized(new { message = "未登入或 Token 已失效" });
            int userId = int.Parse(userIdStr);

            // 診斷：抓取 User 資訊
            var user = await _context.Users.Include(u => u.MemberProfile).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound(new { message = "找不到該使用者" });

            int balance = user.MemberProfile?.PointBalance ?? 0;
            
            return Ok(new { 
                userId = user.Id,
                account = user.Account,
                fullNameInDb = user.MemberProfile?.FullName,
                balance = balance, 
                pointBalance = balance,
                source = "Database (MemberProfiles)"
            });
        }

        // 新增：開發者專用免登入診斷工具
        [AllowAnonymous]
        [HttpGet("debug-check")]
        public async Task<IActionResult> DebugCheck(int userId)
        {
            var user = await _context.Users
                .Include(u => u.MemberProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound($"資料庫找不到 ID 為 {userId} 的使用者");

            return Ok(new {
                提示 = "這是直接讀取資料庫的結果",
                資料庫ID = user.Id,
                帳號 = user.Account,
                姓名 = user.MemberProfile?.FullName ?? "未設定",
                點數餘額 = user.MemberProfile?.PointBalance ?? 0,
                是否有設定檔 = user.MemberProfile != null
            });
        }

        [HttpGet("fix-my-profile")]
        public async Task<IActionResult> FixMyProfile()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized("請先登入");
            int userId = int.Parse(userIdStr);

            var user = await _context.Users.Include(u => u.MemberProfile).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound("找不到使用者");

            var profile = user.MemberProfile;
            
            if (profile == null)
            {
                profile = new MemberProfile
                {
                    UserId = userId,
                    LevelId = 1,
                    PointBalance = 0,
                    FullName = user.Account, // 使用帳號當預設姓名
                    IsSeller = false,
                    UpdatedAt = DateTime.Now
                };
                _context.MemberProfiles.Add(profile);
            }
            else
            {
                // 如果姓名是 "管理員" 且帳號不是 admin，就修正它
                if (profile.FullName == "管理員" && user.Account != "admin")
                {
                    profile.FullName = user.Account;
                }
                profile.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "個人資料已修復", account = user.Account, fullName = profile.FullName });
        }

        [HttpGet("point-history")]
        public async Task<IActionResult> GetMyPointHistory()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized(new { message = "未登入" });
            int userId = int.Parse(userIdStr);

            var history = await _context.PointHistories
                .Where(ph => ph.UserId == userId)
                .OrderByDescending(ph => ph.CreatedAt)
                .Select(ph => new {
                    ph.Id,
                    ph.ChangeAmount,
                    ph.BalanceAfter,
                    ph.Description,
                    ph.CreatedAt
                })
                .ToListAsync();

            return Ok(history);
        }

        [HttpGet("add-points-to-me")]
        public async Task<IActionResult> AddPointsToMe()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized("請先登入");
            int userId = int.Parse(userIdStr);

            var user = await _context.Users.Include(u => u.MemberProfile).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound("找不到使用者");

            var profile = user.MemberProfile;
            
            if (profile == null)
            {
                profile = new MemberProfile
                {
                    UserId = userId,
                    LevelId = 1,
                    PointBalance = 0,
                    FullName = user.Account,
                    IsSeller = false
                };
                _context.MemberProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            int amount = 1000;
            profile.PointBalance = (profile.PointBalance ?? 0) + amount;

            _context.PointHistories.Add(new PointHistory {
                UserId = userId,
                ChangeAmount = amount,
                BalanceAfter = profile.PointBalance.Value,
                Description = "使用者手動觸發儲值 (測試用)",
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return Ok(new { message = $"成功增加 {amount} 點", currentBalance = profile.PointBalance });
        }
    }
}
