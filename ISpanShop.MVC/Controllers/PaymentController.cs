using ISpanShop.Models.EfModels;
using ISpanShop.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.MVC.Controllers
{
	public class PaymentController : Controller
	{
		private readonly ISpanShopDBContext _context;
		private readonly PaymentService _paymentService;

		public PaymentController(ISpanShopDBContext context, PaymentService paymentService)
		{
			_context = context;
			_paymentService = paymentService;
		}

		/// <summary>
		/// 點擊付款 → 跳轉綠界測試頁面
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> Pay(string orderNumber)
		{
			var order = await _context.Orders
				.Include(o => o.OrderDetails)
				.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

			if (order == null) return NotFound("訂單不存在");

			// 產生交易編號
			string merchantTradeNo = _paymentService.GenerateMerchantTradeNo(order);

			// 取得綠界參數
			var parameters = _paymentService.GetEcpayParameters(order, merchantTradeNo);

			// 組成自動送出表單（UTF-8，中文正常）
			var sb = new StringBuilder();
			sb.AppendLine("<!DOCTYPE html>");
			sb.AppendLine("<html lang='zh-Hant'>");
			sb.AppendLine("<head>");
			sb.AppendLine("<meta charset='UTF-8'>");
			sb.AppendLine("<title>轉向綠界支付</title>");
			sb.AppendLine("</head>");
			sb.AppendLine("<body>");
			sb.AppendLine("<h4>正在轉向綠界支付...</h4>");
			sb.AppendLine($"<form id='payForm' action='https://payment-stage.ecpay.com.tw/Cashier/AioCheckOut/V5' method='POST'>");

			foreach (var p in parameters)
				sb.AppendLine($"<input type='hidden' name='{p.Key}' value='{p.Value}' />");

			sb.AppendLine("</form>");
			sb.AppendLine("<script>document.getElementById('payForm').submit();</script>");
			sb.AppendLine("</body></html>");

			return Content(sb.ToString(), "text/html; charset=utf-8");
		}

		/// <summary>
		/// 綠界回傳網址（模擬交易成功，專題用）
		/// </summary>
		[HttpPost]
		public IActionResult Return()
		{
			var form = Request.Form;

			// 專題展示用，直接當作交易成功
			ViewBag.Message = "交易成功（模擬結果）";
			ViewBag.MerchantTradeNo = form["MerchantTradeNo"];
			ViewBag.Amount = form["TradeAmt"];
			ViewBag.TradeDate = form["TradeDate"];
			ViewBag.PaymentType = form["PaymentType"];

			return View();
		}
	}
}