using Microsoft.AspNetCore.Mvc;
using ISpanShop.Services;
using ISpanShop.Services.Coupons;
using ISpanShop.Services.Communication;
using ISpanShop.Models.EfModels;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.WebAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class PaymentCallbackController : ControllerBase
	{
		private readonly ISpanShopDBContext _context;
		private readonly PaymentService _paymentService;
		private readonly ICouponService _couponService;
		private readonly IEmailService _emailService;

		public PaymentCallbackController(
			ISpanShopDBContext context, 
			PaymentService paymentService,
			ICouponService couponService,
			IEmailService emailService)
		{
			_context = context;
			_paymentService = paymentService;
			_couponService = couponService;
			_emailService = emailService;
		}

		// 接收綠界付款結果通知
		// 綠界是用 Form 表單 POST 過來的，所以用 [FromForm]
		[HttpPost]
		public async Task<IActionResult> EcpayReturn([FromForm] IFormCollection collection)
		{
			// 1. 將回傳資料轉為 Dictionary，以便驗證加密碼
			var dict = collection.ToDictionary(k => k.Key, v => v.Value.ToString());
			dict.Remove("CheckMacValue");

			// 2. 驗證 CheckMacValue (確保這封信真的是綠界寄的，不是駭客偽造)
			if (!_paymentService.ValidateCheckMacValue(dict))
			{
				return BadRequest("Checksum failed");
			}

			// 3. 檢查 RtnCode (1 代表付款成功)
			if (dict["RtnCode"] == "1")
			{
				// 取得綠界的交易編號
				string merchantTradeNo = dict["MerchantTradeNo"];

				// 這裡你需要根據 MerchantTradeNo 去 PaymentLogs 找到對應的 OrderId
				var paymentLog = await _context.PaymentLogs
					.Include(p => p.Order)
					.ThenInclude(o => o.User)
					.FirstOrDefaultAsync(p => p.MerchantTradeNo == merchantTradeNo);

				if (paymentLog != null)
				{
					// 呼叫你在 PaymentService 寫好的更新邏輯
					// 將 RtnMsg, TradeNo 等資訊補進 PaymentLog，並把訂單狀態改為「已付款」
					paymentLog.TradeNo = dict["TradeNo"];
					paymentLog.RtnCode = 1;
					paymentLog.RtnMsg = dict["RtnMsg"];
					paymentLog.PaymentDate = DateTime.Parse(dict["PaymentDate"]);

					// 找出訂單並更新狀態
					var order = paymentLog.Order;
					if (order != null)
					{
						order.Status = 1; // 假設 1 代表已付款
						order.PaymentDate = paymentLog.PaymentDate;

						// 更新優惠券狀態為已使用
						await _couponService.MarkAsUsedAsync(order.Id);

						// 發送扣款自動通知 (Email)
						try
						{
							string userEmail = order.User?.Email;
							if (!string.IsNullOrEmpty(userEmail))
							{
								string subject = $"【HowBuy】訂單付款成功通知 - 訂單編號：{order.OrderNumber}";
								string body = $@"
									<h3>親愛的會員您好：</h3>
									<p>您的訂單 <strong>{order.OrderNumber}</strong> 已成功完成付款。</p>
									<p>付款金額：NT$ {order.FinalAmount:N0}</p>
									<p>付款時間：{paymentLog.PaymentDate:yyyy/MM/dd HH:mm:ss}</p>
									<br/>
									<p>感謝您的購買！</p>
									<p>HowBuy 團隊 敬上</p>";
								
								await _emailService.SendEmailAsync(userEmail, subject, body);
							}
						}
						catch (Exception ex)
						{
							// 通知失敗不應導致交易回滾，記錄錯誤即可
							Console.WriteLine($"[Payment Notification Error] {ex.Message}");
						}
					}

					await _context.SaveChangesAsync();
				}
			}

			// 4. 重要：綠界規定收到通知後，必須回傳 "1|OK" 給他們，否則他們會一直重寄通知
			return Content("1|OK");
		}
	}
}