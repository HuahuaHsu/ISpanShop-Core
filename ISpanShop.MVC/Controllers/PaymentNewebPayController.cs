using ISpanShop.Models.EfModels;
using ISpanShop.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.MVC.Controllers
{
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

			if (order == null)
				return NotFound("訂單不存在");

			string merchantTradeNo = _newebPayService.GenerateMerchantTradeNo(order);

			// 建立付款日誌 (PaymentLog)
			var paymentLog = new PaymentLog
			{
				OrderId = order.Id,
				MerchantTradeNo = merchantTradeNo,
				TradeAmt = order.FinalAmount,
				CreatedAt = DateTime.Now
			};
			_context.PaymentLogs.Add(paymentLog);
			await _context.SaveChangesAsync();

			var parameters = _newebPayService.GetNewebPayParameters(order, merchantTradeNo);

			// 生成自動送出表單
			var sb = new StringBuilder();
			sb.AppendLine("<h4>正在轉向藍新支付...</h4>");
			sb.AppendLine("<form id='payForm' " +
						  "action='https://ccore.newebpay.com/MPG/mpg_gateway' " +
						  "method='POST'>");

			foreach (var p in parameters)
			{
				sb.AppendLine($"<input type='hidden' name='{p.Key}' value='{p.Value}' />");
			}

			sb.AppendLine("</form>");
			sb.AppendLine("<script>document.getElementById('payForm').submit();</script>");

			return Content(sb.ToString(), "text/html");
		}

		[HttpPost]
		public async Task<IActionResult> Return()
		{
			var form = Request.Form;
			// 藍新回傳的編號通常在 MerchantOrderNo (如果沒有則從 Form 找)
			string merchantOrderNo = form["MerchantOrderNo"];
			
			// 交易回傳模擬成功
			var paymentLog = await _context.PaymentLogs
				.FirstOrDefaultAsync(p => p.MerchantTradeNo == merchantOrderNo);

			if (paymentLog != null)
			{
				paymentLog.TradeNo = "NEWB_" + DateTime.Now.ToString("HHmmss");
				paymentLog.RtnCode = 1;
				paymentLog.RtnMsg = "交易成功 (專題模擬)";
				paymentLog.PaymentDate = DateTime.Now;

				var order = await _context.Orders.FindAsync(paymentLog.OrderId);
				if (order != null)
				{
					order.Status = 1; // 已付款
					order.PaymentDate = paymentLog.PaymentDate;
					await _context.SaveChangesAsync();
				}
			}

			ViewBag.Message = "交易成功（模擬）";
			ViewBag.MerchantTradeNo = merchantOrderNo;
			ViewBag.Amount = form["Amt"];
			ViewBag.PaymentType = string.IsNullOrEmpty(form["PaymentType"]) ? "CreditCard" : form["PaymentType"].ToString();
			return View();
		}
	}
}