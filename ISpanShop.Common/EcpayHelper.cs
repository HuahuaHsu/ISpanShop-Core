using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ISpanShop.Common
{
    public static class EcpayHelper
    {
        // 綠界測試帳號金鑰（正式環境請更換）
        private const string HashKey = "pwFHCqoQZGmho4w6";
        private const string HashIV = "EkRm7iFT261dpevs";

        public static string GenerateCheckMacValue(Dictionary<string, string> parameters)
        {
            // 1. 排序：依照字母 A-Z 排序，排除 CheckMacValue
            var sorted = parameters
                .Where(kv => kv.Key != "CheckMacValue")
                .OrderBy(kv => kv.Key)
                .Select(kv => $"{kv.Key}={kv.Value}");

            // 2. 串接：前後加上 HashKey 和 HashIV
            string raw = $"HashKey={HashKey}&{string.Join("&", sorted)}&HashIV={HashIV}";

            // 3. URL Encode：綠界要求的特殊編碼處理
            string encoded = CustomUrlEncode(raw);

            // 4. SHA256 加密並轉大寫
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(encoded));
                return BitConverter.ToString(bytes).Replace("-", "").ToUpper();
            }
        }

        private static string CustomUrlEncode(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return string.Empty;

            // 使用 HttpUtility.UrlEncode 並轉小寫
            string result = HttpUtility.UrlEncode(raw).ToLower();

            // 綠界官方指定的替換規則
            result = result
                .Replace("%2d", "-")
                .Replace("%5f", "_")
                .Replace("%2e", ".")
                .Replace("%21", "!")
                .Replace("%2a", "*")
                .Replace("%28", "(")
                .Replace("%29", ")");

            return result;
        }

        // 清理特殊符號，防止 CheckMacValue 出錯
        public static string CleanString(string input, int maxLength = 50)
        {
            if (string.IsNullOrEmpty(input)) return "商品";

            // 移除可能干擾 QueryString 的符號
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