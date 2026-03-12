using Microsoft.AspNetCore.Mvc;
using ISpanShop.Services;
using ISpanShop.MVC.Models.ViewModels;
using System.Threading.Tasks;
using System.Linq;
using ISpanShop.Models.DTOs;

namespace ISpanShop.MVC.Areas.Admin.Controllers.Orders
{
    [Area("Admin")]
    public class OrderReviewsController : Controller
    {
        private readonly IOrderReviewService _service;

        public OrderReviewsController(IOrderReviewService service)
        {
            _service = service;
        }

        // --- 顯示所有評論 ---
        public async Task<IActionResult> Index()
        {
            var dtos = await _service.GetAllAsync();

            var vms = dtos.Select(d => new OrderReviewVm
            {
                Id = d.Id,
                UserId = d.UserId,
                OrderId = d.OrderId,
                Rating = d.Rating,
                Comment = d.Comment,
                SellerReply = d.SellerReply,
                IsHidden = d.IsHidden,
                IsAutoFlagged = d.IsAutoFlagged,
                CreatedAt = d.CreatedAt
            }).ToList();

            return View(vms);
        }

        // --- 切換隱藏狀態 (Soft Delete) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleHidden(int id)
        {
            await _service.ToggleHiddenStatusAsync(id);
            // 切換完畢後，重新導回列表頁面
            return RedirectToAction(nameof(Index));
        }

        // --- 模擬前台使用者送出含有「敏感字」的評價 ---
        public async Task<IActionResult> TestAutoModeration()
        {
            var testReview = new OrderReviewDto
            {
                UserId = 1, // 假設的使用者 ID
                OrderId = 1, // 假設的訂單 ID
                Rating = 1,
                // 這裡刻意寫一段包含潛在敏感詞的句子 (例如您可以去後台設定 "詐騙" 或 "爛" 為敏感詞)
                Comment = "這個商品真的很爛，根本是詐騙集團，退錢啦！", 
                CreatedAt = System.DateTime.Now
            };

            await _service.AddReviewAsync(testReview); 

            // 塞完資料後，自動導回列表頁面看結果
            return RedirectToAction(nameof(Index));
        }
    }
}
