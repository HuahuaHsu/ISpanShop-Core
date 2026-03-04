using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ISpanShop.Common
{
	public static class EcpayHelper
	{
		// 綠界測試用 HashKey 與 HashIV
		private const string HashKey = "5294y06JbISpM5x9";
		private const string HashIV = "v77hoKGq4kWxJvU5";

		public static string GenerateCheckMacValue(Dictionary<string, string> parameters)
		{
			// 1. 依照參數名稱字典排序 (A-Z)
			var sortedParams = parameters.OrderBy(x => x.Key)
				.Select(x => $"{x.Key}={x.Value}");

			// 2. 組合字串，前後加上 HashKey 與 HashIV
			string rawData = $"HashKey={HashKey}&{string.Join("&", sortedParams)}&HashIV={HashIV}";

			// 3. URL Encode
			string encodedData = HttpUtility.UrlEncode(rawData).ToLower();

			// 4. SHA256 加密
			using (var sha256 = SHA256.Create())
			{
				byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(encodedData));
				StringBuilder sb = new StringBuilder();
				foreach (var b in hashBytes) sb.Append(b.ToString("X2"));
				return sb.ToString(); // 轉大寫回傳
			}
		}
	}
}