using ISpanShop.Common;
using ISpanShop.Models.EfModels;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.Services
{
	public class PaymentService
	{
		private readonly ISpanShopDBContext _context;

		public PaymentService(ISpanShopDBContext context)
		{
			_context = context;
		}

		/// <summary>
		/// 產生綠界專用交易單號 (20碼內)
		/// 格式：TS + 月日時分秒 + 4位隨機數
		/// </summary>
		public string GenerateMerchantTradeNo()
		{
			var timestamp = DateTime.Now.ToString("MMddHHmmss");
			var random = new Random().Next(1000, 9999).ToString();
			return $"TS{timestamp}{random}";
		}

		/// <summary>
		/// 產生綠界所需參數
		/// </summary>
		public Dictionary<string, string> GetEcpayParameters(Order order, string merchantTradeNo)
		{
			var dict = new Dictionary<string, string>
			{
				{ "MerchantID", "2000132" }, // 測試特店編號
                { "MerchantTradeNo", merchantTradeNo },
				{ "MerchantTradeDate", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") },
				{ "PaymentType", "aio" },
				{ "TotalAmount", ((int)order.FinalAmount).ToString() },
				{ "TradeDesc", "ISpanShop訂單付款" },
				{ "ItemName", "ISpanShop商品一批" },
				{ "ReturnURL", "https://你的網址/api/PaymentCallback" },
				{ "OrderResultURL", "https://你的網址/Payment/Result" },
				{ "ChoosePayment", "ALL" },
				{ "EncryptType", "1" }
			};

			// 產生 CheckMacValue
			dict.Add("CheckMacValue", EcpayHelper.GenerateCheckMacValue(dict));

			return dict;
		}

		/// <summary>
		/// 驗證綠界回傳 CheckMacValue
		/// </summary>
		public bool ValidateCheckMacValue(Dictionary<string, string> ecpayReturnData)
		{
			var received = ecpayReturnData["CheckMacValue"];
			ecpayReturnData.Remove("CheckMacValue");

			string calculated = EcpayHelper.GenerateCheckMacValue(ecpayReturnData);

			return received == calculated;
		}

		/// <summary>
		/// 處理付款成功後邏輯
		/// </summary>
		public async Task<bool> ProcessPaymentCallbackAsync(PaymentLog log, long orderId)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();

			try
			{
				_context.PaymentLogs.Add(log);

				var order = await _context.Orders.FindAsync(orderId);
				if (order != null)
				{
					order.Status = 1; // 已付款
					order.PaymentDate = DateTime.Now;
				}

				await _context.SaveChangesAsync();
				await transaction.CommitAsync();
				return true;
			}
			catch
			{
				await transaction.RollbackAsync();
				return false;
			}
		}
	}
}