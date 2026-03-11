using ISpanShop.Models.EfModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Linq;
using System.Web;

namespace ISpanShop.Services
{
    public class NewebPayService
    {
        // 藍新測試帳號（官方提供，正式上線請換成自己的）
        private const string MerchantID = "MS151740924"; // 這是藍新官方提供的測試 ID 範例，請確認你的 ID
        private const string HashKey = "m0GIn6VjL0zE8yJ1hA6IuU8Hw3hRz8Jm"; // 測試用的 Key
        private const string HashIV = "fD3rY6uG8jI1kO3p"; // 測試用的 IV

        public string GenerateMerchantTradeNo(Order order)
        {
            // 產生唯一交易編號，藍新通常要求 20 字以內
            return $"N{order.Id:D6}{DateTime.Now:HHmmss}";
        }

        public Dictionary<string, string> GetNewebPayParameters(Order order, string merchantTradeNo)
        {
            // 1. 準備交易參數 (TradeInfo 原型)
            var tradeParams = new Dictionary<string, string>
            {
                { "MerchantID", MerchantID },
                { "RespondType", "JSON" },
                { "TimeStamp", DateTimeOffset.Now.ToUnixTimeSeconds().ToString() },
                { "Version", "2.0" },
                { "MerchantOrderNo", merchantTradeNo },
                { "Amt", ((int)order.TotalAmount).ToString() },
                { "ItemDesc", "商品購買" },
                { "Email", "test@example.com" }, // 建議傳入使用者的 Email
                { "LoginType", "0" },
                { "ReturnURL", "https://localhost:7230/PaymentNewebPay/Return" }, // 依照你的環境修改
                { "NotifyURL", "https://your-domain.com/api/Payment/NewebPayNotify" },
                { "ClientBackURL", "https://localhost:7230/" },
                { "OrderComment", "測試訂單" }
            };

            // 2. 將參數串接成 QueryString
            string queryString = string.Join("&", tradeParams.Select(kv => $"{kv.Key}={kv.Value}"));

            // 3. 進行 AES 加密 (TradeInfo)
            string tradeInfo = EncryptAES(queryString, HashKey, HashIV);

            // 4. 進行 SHA256 加密 (TradeSha)
            string tradeSha = GenerateTradeSha(tradeInfo, HashKey, HashIV);

            // 5. 回傳給 Controller 使用的最終參數
            return new Dictionary<string, string>
            {
                { "MerchantID", MerchantID },
                { "TradeInfo", tradeInfo },
                { "TradeSha", tradeSha },
                { "Version", "2.0" }
            };
        }

        private string EncryptAES(string source, string key, string iv)
        {
            byte[] sourceBytes = Encoding.UTF8.GetBytes(source);
            
            // 補位 (PKCS7)
            int blockSize = 32;
            int padding = blockSize - (sourceBytes.Length % blockSize);
            Array.Resize(ref sourceBytes, sourceBytes.Length + padding);
            for (int i = 0; i < padding; i++)
            {
                sourceBytes[sourceBytes.Length - padding + i] = (byte)padding;
            }

            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Encoding.UTF8.GetBytes(iv);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None; // 手動補位

                using (var encryptor = aes.CreateEncryptor())
                {
                    byte[] encrypted = encryptor.TransformFinalBlock(sourceBytes, 0, sourceBytes.Length);
                    return BitConverter.ToString(encrypted).Replace("-", "").ToLower();
                }
            }
        }

        private string GenerateTradeSha(string tradeInfo, string key, string iv)
        {
            string raw = $"HashKey={key}&{tradeInfo}&HashIV={iv}";
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(raw));
                return BitConverter.ToString(bytes).Replace("-", "").ToUpper();
            }
        }
    }
}