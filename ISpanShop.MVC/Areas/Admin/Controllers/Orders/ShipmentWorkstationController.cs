using ISpanShop.Common.Enums;
using ISpanShop.Models.DTOs.Orders;
using ISpanShop.MVC.Areas.Admin.Controllers;
using ISpanShop.MVC.Areas.Admin.Models.Orders;
using ISpanShop.Services.Orders;
using Microsoft.AspNetCore.Mvc;

namespace ISpanShop.MVC.Areas.Admin.Controllers.Orders
{
    public class ShipmentWorkstationController : AdminBaseController
    {
        private readonly IOrderService _orderService;

        public ShipmentWorkstationController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var counts = await _orderService.GetOrderStatusCountsAsync();
            var totalPending = counts.TryGetValue(1, out int c1) ? c1 : 0;

            // 取得緊急處理 (超過 48 小時未出貨)
            var urgentResult = await _orderService.GetFilteredOrdersAsync(new OrderSearchDto
            {
                Statuses = new List<int> { 1 },
                EndDate = DateTime.Now.AddDays(-2),
                PageSize = 1
            });

            // 取得庫存緊缺總數
            var lowStockResult = await _orderService.GetFilteredOrdersAsync(new OrderSearchDto
            {
                Statuses = new List<int> { 1 },
                StockStatus = 2,
                PageSize = 1
            });

            var vm = new OrderIndexVm
            {
                CountTotal = totalPending,
                CountUrgentShipment = urgentResult.TotalCount,
                CountLowStock = lowStockResult.TotalCount,
                DateDimensionOptions = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
                {
                    new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Text = "下單時間", Value = "1" },
                    new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Text = "付款時間", Value = "2" }
                }
            };
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetStatsAjax()
        {
            var counts = await _orderService.GetOrderStatusCountsAsync();
            var totalPending = counts.TryGetValue(1, out int c1) ? c1 : 0;

            var urgentResult = await _orderService.GetFilteredOrdersAsync(new OrderSearchDto
            {
                Statuses = new List<int> { 1 },
                EndDate = DateTime.Now.AddDays(-2),
                PageSize = 1
            });

            var lowStockResult = await _orderService.GetFilteredOrdersAsync(new OrderSearchDto
            {
                Statuses = new List<int> { 1 },
                StockStatus = 2,
                PageSize = 1
            });

            return Json(new { 
                success = true, 
                totalPending = totalPending, 
                totalUrgent = urgentResult.TotalCount,
                totalLowStock = lowStockResult.TotalCount
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderDetailAjax(long id)
        {
            var order = await _orderService.GetOrderDetailAsync(id);
            if (order == null) return Json(new { success = false, message = "找不到訂單" });
            return Json(new { success = true, data = order });
        }

        [HttpPost]
        public async Task<IActionResult> GetShipmentListAjax([FromBody] OrderSearchDto searchParams)
        {
            // 強制設定狀態為「待出貨」
            searchParams.Statuses = new List<int> { 1 };
            // 預設按付款時間排序（最久未出貨的在最前面）
            if (string.IsNullOrEmpty(searchParams.SortBy))
            {
                searchParams.SortBy = "OrderDate"; // 這裡 Repository 預設會用 CreatedAt，如有需要可再微調
                searchParams.IsDescending = false; 
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

        [HttpPost]
        public async Task<IActionResult> BatchUpdateStatus([FromBody] BatchUpdateStatusDto model)
        {
            if (model.OrderIds == null || !model.OrderIds.Any()) 
                return Json(new { success = false, message = "請選擇訂單" });

            try
            {
                foreach (var id in model.OrderIds)
                {
                    await _orderService.UpdateStatusAsync(id, model.NewStatus);
                }
                return Json(new { success = true, message = $"已成功更新 {model.OrderIds.Count} 筆訂單狀態" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class BatchUpdateStatusDto
        {
            public List<long> OrderIds { get; set; }
            public OrderStatus NewStatus { get; set; }
        }
    }
}
