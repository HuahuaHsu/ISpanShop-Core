using ISpanShop.Common.Enums;
using ISpanShop.Models.DTOs.Orders;
using ISpanShop.MVC.Areas.Admin.Controllers;
using ISpanShop.MVC.Areas.Admin.Models.Orders;
using ISpanShop.Services.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ISpanShop.MVC.Areas.Admin.Controllers.Orders
{
    public class ReturnRequestsController : AdminBaseController
    {
        private readonly IOrderService _orderService;

        public ReturnRequestsController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var counts = await _orderService.GetOrderStatusCountsAsync();

            var vm = new OrderIndexVm
            {
                CountTotal = (counts.TryGetValue(5, out int r) ? r : 0) + (counts.TryGetValue(6, out int f) ? f : 0),
                CountPendingPayment = counts.TryGetValue(5, out int c5) ? c5 : 0, // 借用欄位顯示「待處理退貨」
                CountCompleted = counts.TryGetValue(6, out int c6) ? c6 : 0,      // 借用欄位顯示「已退款結案」
                StatusOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "5", Text = "退貨/款中" },
                    new SelectListItem { Value = "6", Text = "已退款" }
                },
                DateDimensionOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "1", Text = "下單日期" },
                    new SelectListItem { Value = "2", Text = "付款日期" },
                    new SelectListItem { Value = "3", Text = "完成日期" },
                    new SelectListItem { Value = "4", Text = "申請退款日期" },
                    new SelectListItem { Value = "5", Text = "結案日期" }
                }
            };
            return View(vm);
        }

        public async Task<IActionResult> Review(long id)
        {
            var order = await _orderService.GetOrderDetailAsync(id);
            if (order == null || (order.Status != OrderStatus.Returning && order.Status != OrderStatus.Refunded))
            {
                return NotFound();
            }
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(long id)
        {
            try
            {
                await _orderService.UpdateStatusAsync(id, OrderStatus.Refunded);
                return Json(new { success = true, message = "已核准退款" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Reject(long id)
        {
            try
            {
                // 拒絕退款，暫時恢復為已完成
                await _orderService.UpdateStatusAsync(id, OrderStatus.Completed);
                return Json(new { success = true, message = "已拒絕退款申請" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetReturnListAjax([FromBody] OrderSearchDto searchParams)
        {
            // 強制只搜尋退貨相關狀態
            if (searchParams.Statuses == null || !searchParams.Statuses.Any())
            {
                searchParams.Statuses = new List<int> { 5, 6 };
            }
            
            try
            {
                var result = await _orderService.GetFilteredOrdersAsync(searchParams);
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
