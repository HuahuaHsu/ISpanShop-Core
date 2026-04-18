using System;
using Microsoft.AspNetCore.Mvc;
using ISpanShop.Services;
using ISpanShop.MVC.Models.ViewModels;
using System.Threading.Tasks;
using System.Linq;
using ISpanShop.Models.DTOs;
using ISpanShop.Models.Seeding;
using ISpanShop.Models.EfModels;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.MVC.Areas.Admin.Controllers.Orders
{
    [Area("Admin")]
    public class OrderReviewsController : Controller
    {
        private readonly IOrderReviewService _service;
        private readonly ISpanShopDBContext _context;

        public OrderReviewsController(IOrderReviewService service, ISpanShopDBContext context)
        {
            _service = service;
            _context = context;
        }

        // --- 顯示所有評論 (支援分頁) ---
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 10;
            var dtos = await _service.GetAllAsync();

            // 計算分頁資訊
            int totalCount = dtos.Count;
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // 確保當前頁數在合理範圍內
            page = page < 1 ? 1 : page;
            if (totalPages > 0 && page > totalPages) page = totalPages;

            // 進行分頁切割 (依照時間由新到舊排序)
            var pagedDtos = dtos.OrderByDescending(d => d.CreatedAt)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToList();

            var vms = pagedDtos.Select(d => new OrderReviewVm
            {
                Id = d.Id,
                UserId = d.UserId,
                OrderId = d.OrderId,
                Rating = d.Rating,
                Comment = d.Comment,
                StoreReply = d.StoreReply,
                IsHidden = d.IsHidden,
                IsAutoFlagged = d.IsAutoFlagged,
                CreatedAt = d.CreatedAt,
                ImageUrls = d.ImageUrls,
                ProductMainImage = d.ProductMainImage
            }).ToList();

            // 將分頁資訊存入 ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(vms);
        }

        // --- 4. 展示與測試工具 Action ---
        [HttpPost]
        public async Task<IActionResult> GenerateTestReviews()
        {
            // 強制補充 30 筆評論
            await DataSeeder.GenerateOrderReviewsAsync(_context, 30);
            TempData["SuccessMessage"] = "已成功生成 30 筆測試用商品評論！";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ClearAllReviews()
        {
            // 清空所有評論資料
            var allReviews = await _context.OrderReviews.ToListAsync();
            _context.OrderReviews.RemoveRange(allReviews);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "已成功清除所有商品評論！";
            return RedirectToAction(nameof(Index));
        }

        // --- 切換隱藏狀態 (Soft Delete) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleHidden(int id)
        {
            await _service.ToggleHiddenStatusAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // --- 模擬前台使用者送出含有「敏感字」的評價 ---
        public async Task<IActionResult> TestAutoModeration()
        {
            var testReview = new OrderReviewDto
            {
                UserId = 1,
                OrderId = 1,
                Rating = 1,
                Comment = "這個商品真的很爛，根本是詐騙集團，退錢啦！", 
                CreatedAt = System.DateTime.Now
            };

            await _service.AddReviewAsync(testReview); 
            return RedirectToAction(nameof(Index));
        }
    }
}