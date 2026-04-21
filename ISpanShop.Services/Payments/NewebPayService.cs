using ISpanShop.Models.EfModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Linq;
using System.Web;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace ISpanShop.Services
{
    public class NewebPayService
    {
        private readonly string MerchantID;
        private readonly string HashKey;
        private readonly string HashIV;
        private readonly string ReturnURL;
        private readonly string ClientBackURL;

        public NewebPayService(IConfiguration configuration)
        {
            var settings = configuration.GetSection("NewebPaySettings");
            MerchantID = settings["MerchantID"] ?? "";
            HashKey = settings["HashKey"] ?? "";
            HashIV = settings["HashIV"] ?? "";
            ReturnURL = settings["ReturnURL"] ?? "";
            ClientBackURL = settings["ClientBackURL"] ?? "";
        }

        public string DecryptAES(string source)
        {
            if (string.IsNullOrEmpty(source)) return "";

            // Hex 轉 Byte
            byte[] sourceBytes = new byte[source.Length / 2];
            for (int i = 0; i < source.Length; i += 2)
            {
                sourceBytes[i / 2] = Convert.ToByte(source.Substring(i, 2), 16);
            }

            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(HashKey);
                aes.IV = Encoding.UTF8.GetBytes(HashIV);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None; // 藍新使用手動補位或特定填充

                using (var decryptor = aes.CreateDecryptor())
                {
                    byte[] decrypted = decryptor.TransformFinalBlock(sourceBytes, 0, sourceBytes.Length);
                    string result = Encoding.UTF8.GetString(decrypted);
                    
                    // 移除常見的 PKCS7 或空白補位
                    int lastByte = result[result.Length - 1];
                    if (lastByte > 0 && lastByte <= 32)
                    {
                        // 檢查最後幾個字元是否相同，符合 PKCS7 規範
                        bool isPadding = true;
                        for (int i = 1; i <= lastByte; i++)
                        {
                            if (result[result.Length - i] != lastByte) { isPadding = false; break; }
                        }
                        if (isPadding) result = result.Substring(0, result.Length - lastByte);
                    }
                    return result.Trim();
                }
            }
        }

        public Dictionary<string, string> GetNewebPayParameters(Order order, string merchantTradeNo)
        {
            var tradeParams = new Dictionary<string, string>
            {
                { "MerchantID", MerchantID },
                { "RespondType", "JSON" },
                { "TimeStamp", DateTimeOffset.Now.ToUnixTimeSeconds().ToString() },
                { "Version", "2.0" },
                { "MerchantOrderNo", merchantTradeNo },
                { "Amt", ((int)order.FinalAmount).ToString() },
                { "ItemDesc", "商品購買" },
                { "Email", "test@example.com" },
                { "LoginType", "0" },
                { "ReturnURL", ReturnURL },
                { "ClientBackURL", ClientBackURL }
            };

            string notifyUrl = ReturnURL.Replace("Return", "Notify");
            if (!notifyUrl.Contains("localhost") && !notifyUrl.Contains(":"))
            {
                tradeParams.Add("NotifyURL", notifyUrl);
            }

            string queryString = string.Join("&", tradeParams.Select(kv => $"{kv.Key}={kv.Value}"));
            string tradeInfo = EncryptAES(queryString, HashKey, HashIV);
            string tradeSha = GenerateTradeSha(tradeInfo, HashKey, HashIV);

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
            int blockSize = 32;
            int padding = blockSize - (sourceBytes.Length % blockSize);
            Array.Resize(ref sourceBytes, sourceBytes.Length + padding);
            for (int i = 0; i < padding; i++) sourceBytes[sourceBytes.Length - padding + i] = (byte)padding;

            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Encoding.UTF8.GetBytes(iv);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;
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

        public string GenerateMerchantTradeNo(Order order) => $"N{order.Id:D6}{DateTime.Now:HHmmss}";

        public class NewebPayReturnDTO
        {
            public string Status { get; set; }
            public string Message { get; set; }
            public NewebPayResult Result { get; set; }
        }

        public class NewebPayResult
        {
            public string MerchantOrderNo { get; set; }
            public string TradeNo { get; set; }
        }
    }
}