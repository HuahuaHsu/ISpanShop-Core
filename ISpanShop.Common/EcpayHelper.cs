using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Collections.Generic;
using System.Linq;

namespace ISpanShop.Common
{
	public static class EcpayHelper
	{
		private const string HashKey = "pwFHCqoQZGmho4w6";
		private const string HashIV = "EkRm7iFT261dpevs";

		public static string GenerateCheckMacValue(Dictionary<string, string> parameters)
		{
			// 排序 A-Z，排除 CheckMacValue
			var sorted = parameters
				.Where(kv => kv.Key != "CheckMacValue")
				.OrderBy(kv => kv.Key)
				.Select(kv => $"{kv.Key}={kv.Value}");

			string raw = $"HashKey={HashKey}&{string.Join("&", sorted)}&HashIV={HashIV}";

			string encoded = HttpUtility.UrlEncode(raw).ToUpper();

			encoded = encoded
				.Replace("%2D", "-")
				.Replace("%5F", "_")
				.Replace("%2E", ".")
				.Replace("%21", "!")
				.Replace("%2A", "*")
				.Replace("%28", "(")
				.Replace("%29", ")");

			using (var sha256 = SHA256.Create())
			{
				var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(encoded));
				var sb = new StringBuilder();
				foreach (var b in bytes) sb.Append(b.ToString("X2"));
				return sb.ToString();
			}
		}

		// 清理特殊符號，防止 CheckMacValue 出錯
		public static string CleanString(string input, int maxLength = 50)
		{
			if (string.IsNullOrEmpty(input)) return "商品1";

			var cleaned = input.Replace("&", "")
							   .Replace("+", "")
							   .Replace("%", "")
							   .Replace("#", "")
							   .Replace("?", "")
							   .Replace("/", "")
							   .Replace("\\", "")
							   .Replace("\"", "")
							   .Replace("'", "")
							   .Replace("<", "")
							   .Replace(">", "");

			if (cleaned.Length > maxLength)
				cleaned = cleaned.Substring(0, maxLength);

			return cleaned;
		}
	}
}