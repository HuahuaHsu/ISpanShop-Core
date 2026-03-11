using ISpanShop.Common.Enums;
using ISpanShop.Models.EfModels;
using ISpanShop.Services.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.MVC.Controllers
{
    public class OrderTrackingController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ISpanShopDBContext _db;

        public OrderTrackingController(IOrderService orderService, ISpanShopDBContext db)
        {
            _orderService = orderService;
            _db = db;
        }

        // 訂單追蹤頁面 (目前抓取一筆「已完成」的訂單作為測試)
        public async Task<IActionResult> Index(long? id)
        {
            long orderId = 0;

            if (id.HasValue)
            {
                orderId = id.Value;
            }
            else
            {
                // 優先抓取一筆「已完成」的訂單 (OrderStatus.Completed = 3)，方便測試
                orderId = await _db.Orders
                    .Where(o => o.Status == (byte)OrderStatus.Completed)
                    .OrderByDescending(o => o.Id)
                    .Select(o => o.Id)
                    .FirstOrDefaultAsync();

                // 如果沒有已完成的，抓取最新的一筆
                if (orderId == 0)
                {
                    orderId = await _db.Orders
                        .OrderByDescending(o => o.Id)
                        .Select(o => o.Id)
                        .FirstOrDefaultAsync();
                }
            }
            
            if (orderId == 0) return Content("資料庫中目前沒有任何訂單記錄，請先產生訂單再進行測試。");

            var order = await _orderService.GetOrderDetailAsync(orderId);
            
            if (order == null) return Content($"找不到 ID 為 {orderId} 的訂單明細。");

            return View("Tracking", order); // 指向原本的 Tracking.cshtml
        }

        // 處理退款申請
        [HttpPost]
        public async Task<IActionResult> ApplyReturn(long id, string reason, IFormFile evidence)
        {
            var order = await _db.Orders.Include(o => o.ReturnRequests).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            // 1. 更新訂單狀態
            order.Status = (byte)OrderStatus.Returning;
            order.Note = $"【買家退貨申請】原因：{reason}";

            // 2. 建立退貨紀錄 (ReturnRequest)
            var returnRequest = new ReturnRequest
            {
                OrderId = id,
                ReasonCategory = "一般退貨", // 暫時固定分類
                ReasonDescription = reason,
                RefundAmount = order.FinalAmount,
                Status = 0, // 0: 待審核
                CreatedAt = DateTime.Now
            };

            // 3. 處理圖片憑證
            if (evidence != null && evidence.Length > 0)
            {
                order.Note += " (附圖片憑證)";
                
                // 確保目錄存在
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "returns");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                // 產生唯一檔名
                var fileName = $"{id}_{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(evidence.FileName)}";
                var filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await evidence.CopyToAsync(stream);
                }

                // 儲存圖片紀錄
                returnRequest.ReturnRequestImages.Add(new ReturnRequestImage
                {
                    ImageUrl = $"/uploads/returns/{fileName}",
                    CreatedAt = DateTime.Now
                });
            }

            _db.ReturnRequests.Add(returnRequest);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "退貨申請已成功送出！";
            return RedirectToAction(nameof(Index), new { id });
        }
    }
}
