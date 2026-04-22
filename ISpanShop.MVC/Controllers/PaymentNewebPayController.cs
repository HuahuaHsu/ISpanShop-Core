using ISpanShop.Models.EfModels;
using ISpanShop.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ISpanShop.MVC.Controllers
{
    [Route("[controller]/[action]")]
    public class PaymentNewebPayController : Controller
    {
        private readonly ISpanShopDBContext _context;
        private readonly NewebPayService _newebPayService;

        public PaymentNewebPayController(ISpanShopDBContext context, NewebPayService newebPayService)
        {
            _context = context;
            _newebPayService = newebPayService;
        }

        [HttpGet]
        public async Task<IActionResult> Pay(string orderNumber)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

            // 如果找不到，且 orderNumber 以 "ORD" 開頭，嘗試去過濾掉 "ORD" 再找一次 (相容舊資料)
            if (order == null && !string.IsNullOrEmpty(orderNumber) && orderNumber.StartsWith("ORD"))
            {
                string originalNumber = orderNumber.Substring(3);
                order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.OrderNumber == originalNumber);
            }

            if (order == null) return NotFound("訂單不存在");

            string merchantTradeNo = _newebPayService.GenerateMerchantTradeNo(order);

            _context.PaymentLogs.Add(new PaymentLog
            {
                OrderId = order.Id,
                MerchantTradeNo = merchantTradeNo,
                TradeAmt = order.FinalAmount,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();

            var parameters = _newebPayService.GetNewebPayParameters(order, merchantTradeNo);
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'></head><body>");
            sb.AppendLine("<h4>正在轉向藍新支付...</h4>");
            sb.AppendLine("<form id='payForm' action='https://ccore.newebpay.com/MPG/mpg_gateway' method='POST'>");
            foreach (var p in parameters) sb.AppendLine($"<input type='hidden' name='{p.Key}' value='{p.Value}' />");
            sb.AppendLine("</form><script>document.getElementById('payForm').submit();</script></body></html>");

            return Content(sb.ToString(), "text/html", Encoding.UTF8);
        }

        [HttpPost]
        public async Task<IActionResult> Return()
        {
            var form = Request.Form;
            string status = form["Status"]; 
            string tradeInfo = form["TradeInfo"];
            string merchantOrderNo = "";

            try 
            {
                if (!string.IsNullOrEmpty(tradeInfo))
                {
                    string decryptedJson = _newebPayService.DecryptAES(tradeInfo);
                    
                    // 備援：如果 JSON 解析失敗，用 Regex 抓取 MerchantOrderNo
                    var match = Regex.Match(decryptedJson, "\"MerchantOrderNo\":\"([^\"]+)\"");
                    if (match.Success)
                    {
                        merchantOrderNo = match.Groups[1].Value;
                    }
                    else 
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var returnData = JsonSerializer.Deserialize<NewebPayService.NewebPayReturnDTO>(decryptedJson, options);
                        merchantOrderNo = returnData?.Result?.MerchantOrderNo ?? "";
                    }
                }
            }
            catch { /* 忽略錯誤 */ }

            if (string.IsNullOrEmpty(merchantOrderNo)) merchantOrderNo = form["MerchantOrderNo"].ToString();

            var paymentLog = await _context.PaymentLogs
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.MerchantTradeNo == merchantOrderNo);

            string orderNumber = "";
            if (paymentLog?.Order != null)
            {
                orderNumber = paymentLog.Order.OrderNumber;
                if (status == "SUCCESS")
                {
                    paymentLog.TradeNo = form["TradeNo"];
                    paymentLog.RtnCode = 1;
                    paymentLog.PaymentDate = DateTime.Now;
                    paymentLog.Order.Status = 1;
                    paymentLog.Order.PaymentDate = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
            }

            // 如果還是拿不到真正的 OrderNumber，則反向解析 N000123 類型的 ID
            if (string.IsNullOrEmpty(orderNumber) && merchantOrderNo.StartsWith("N"))
            {
                try {
                    int orderId = int.Parse(merchantOrderNo.Substring(1, 6));
                    var backupOrder = await _context.Orders.FindAsync(orderId);
                    orderNumber = backupOrder?.OrderNumber ?? "";
                } catch {}
            }

            // 最後保險：如果真的都沒有，就傳回原始交易代碼
            if (string.IsNullOrEmpty(orderNumber)) orderNumber = merchantOrderNo;

            string frontendUrl = "http://localhost:5173/member/orders";
            return Redirect(frontendUrl);
        }
    }
}