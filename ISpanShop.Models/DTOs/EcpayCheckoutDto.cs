using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Models.DTOs
{
	public class EcpayCheckoutDTO
	{
		public string MerchantTradeNo { get; set; } // 你產生的 20 字內單號
		public string OrderNumber { get; set; }     // 原始訂單編號
		public int TotalAmount { get; set; }        // 最終扣掉點數後的金額
		public string ItemName { get; set; }        // 商品名稱 (多樣商品用 # 隔開)
		public DateTime OrderDate { get; set; }
	}
}
