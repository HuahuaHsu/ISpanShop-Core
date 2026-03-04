using ISpanShop.Models.EfModels;
using ISpanShop.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.MVC.Controllers
{
	public class PaymentController : Controller
	{
		private readonly ISpanShopDBContext _context;
		private readonly PaymentService _paymentService;

		public PaymentController(
			ISpanShopDBContext context,
			PaymentService paymentService)
		{
			_context = context;
			_paymentService = paymentService;
		}

		/// <summary>
		/// 前往綠界付款（統一使用 PaymentService 產生參數）
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> Pay(string orderNumber)
		{
			// 1. 找訂單
			var order = await _context.Orders
				.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

			if (order == null)
				return NotFound("訂單不存在");

			// 2. 產生交易編號
			string merchantTradeNo = _paymentService.GenerateMerchantTradeNo();

			// 3. 取得綠界參數（包含 CheckMacValue）
			var parameters = _paymentService.GetEcpayParameters(order, merchantTradeNo);

			// 4. 組成自動送出表單
			var sb = new System.Text.StringBuilder();
			sb.AppendLine("<h4>正在轉向綠界支付...</h4>");
			sb.AppendLine("<form id='payForm' " +
						   "action='https://payment-stage.ecpay.com.tw/Cashier/AioCheckOut/V5' " +
						   "method='POST'>");

			foreach (var p in parameters)
			{
				// 移除特殊符號，避免 CheckMacValue 計算錯誤
				string key = p.Key;
				string value = p.Value.Replace("&", "").Replace("+", "").Replace("%", "").Replace("#", "");
				sb.AppendLine($"<input type='hidden' name='{key}' value='{value}' />");
			}

			sb.AppendLine("</form>");
			sb.AppendLine("<script>document.getElementById('payForm').submit();</script>");

			return Content(sb.ToString(), "text/html");
		}
	}
}