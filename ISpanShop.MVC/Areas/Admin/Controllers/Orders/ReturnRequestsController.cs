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
                }
            };
            return View(vm);
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
