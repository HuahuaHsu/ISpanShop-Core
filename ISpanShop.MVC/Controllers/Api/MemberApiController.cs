using Microsoft.AspNetCore.Mvc;
using ISpanShop.Services.Payments;
using ISpanShop.Models.EfModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace ISpanShop.WebAPI.Controllers
{
    [ApiController]
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

        [HttpGet("wallet-balance")]
        public async Task<IActionResult> GetWalletBalance()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value; // 嘗試抓取名稱
            
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized(new { message = "未登入或 Token 已失效" });
            int userId = int.Parse(userIdStr);

            int balance = await _pointService.GetBalanceAsync(userId);
            
            return Ok(new { 
                userId,
                loginAccount = userName,
                balance, 
                pointBalance = balance
            });
        }

        [HttpGet("add-test-points")]
        public async Task<IActionResult> AddTestPoints()
        {
            var account = "a992006";
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Account == account);
            if (user == null) return NotFound("找不到測試帳號 a992006");

            var profile = await _context.MemberProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (profile == null) return NotFound("找不到該帳號的會員設定檔");

            profile.PointBalance = (profile.PointBalance ?? 0) + 1000;

            _context.PointHistories.Add(new PointHistory {
                UserId = user.Id,
                ChangeAmount = 1000,
                BalanceAfter = profile.PointBalance.Value,
                Description = "系統測試儲值",
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return Ok($"已成功為 {account} 儲值 1000 點蝦幣。目前餘額：{profile.PointBalance}");
        }

        [HttpGet("add-points-to-me")]
        public async Task<IActionResult> AddPointsToMe()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized("請先登入");
            int userId = int.Parse(userIdStr);

            var profile = await _context.MemberProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return NotFound("找不到您的會員設定檔");

            profile.PointBalance = (profile.PointBalance ?? 0) + 1000;

            _context.PointHistories.Add(new PointHistory {
                UserId = userId,
                ChangeAmount = 1000,
                BalanceAfter = profile.PointBalance.Value,
                Description = "開發者自我儲值測試",
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return Ok($"儲值成功！已為您的帳號增加 1000 點。目前餘額：{profile.PointBalance}");
        }
        }
        }
