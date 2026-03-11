using ISpanShop.Models.EfModels;
using ISpanShop.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ISpanShop.Services
{
	public class PaymentService
	{
		// 產生交易編號，長度不超過 20
		public string GenerateMerchantTradeNo(Order order)
		{
			return $"O{order.Id:D6}{DateTime.Now:HHmmss}";
		}

		// 取得綠界參數
		public Dictionary<string, string> GetEcpayParameters(Order order, string merchantTradeNo)
		{
			// 將商品名稱串起來，清理特殊符號
			var itemNames = string.Join("#",
				order.OrderDetails.Select(od => EcpayHelper.CleanString(od.ProductName))
			);

			var parameters = new Dictionary<string, string>
			{
				{ "MerchantID", "3002607" },
				{ "MerchantTradeNo", merchantTradeNo },
				{ "MerchantTradeDate", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") },
				{ "PaymentType", "aio" },
				{ "TotalAmount", order.TotalAmount.ToString("F0") },
				{ "TradeDesc", EcpayHelper.CleanString("訂單付款") },
				{ "ItemName", string.IsNullOrEmpty(itemNames) ? "商品1" : itemNames },
				{ "ReturnURL", "https://localhost:7028/api/PaymentCallback" }, // WebAPI 的非同步回傳
				{ "OrderResultURL", "https://localhost:7125/Payment/Return" }, // 使用者瀏覽器跳回的網址
				{ "ChoosePayment", "ALL" }
			};

			parameters.Add("CheckMacValue", EcpayHelper.GenerateCheckMacValue(parameters));
			return parameters;
		}

        // 驗證綠界回傳的 CheckMacValue
        public bool ValidateCheckMacValue(Dictionary<string, string> parameters)
        {
            if (!parameters.ContainsKey("CheckMacValue")) return false;

            string originalCheckMacValue = parameters["CheckMacValue"];
            string calculatedCheckMacValue = EcpayHelper.GenerateCheckMacValue(parameters);

            return string.Equals(originalCheckMacValue, calculatedCheckMacValue, StringComparison.OrdinalIgnoreCase);
        }
	}
}