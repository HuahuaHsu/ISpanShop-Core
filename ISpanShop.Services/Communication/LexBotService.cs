using Amazon.LexRuntimeV2;
using Amazon.LexRuntimeV2.Model;
using ISpanShop.Services.Communication;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace ISpanShop.Services.Communication
{
    public class LexBotService : IBotService
    {
        private readonly IAmazonLexRuntimeV2 _lexClient;
        private readonly IConfiguration _config;

        public LexBotService(IConfiguration config)
        {
            _config = config;
            var awsConfig = _config.GetSection("AWS");
            
            // 初始化 AWS 客戶端
            var credentials = new Amazon.Runtime.BasicAWSCredentials(
                awsConfig["AccessKey"], 
                awsConfig["SecretKey"]
            );
            var region = Amazon.RegionEndpoint.GetBySystemName(awsConfig["Region"]);
            
            _lexClient = new AmazonLexRuntimeV2Client(credentials, region);
        }

        public async Task<string> GetResponseAsync(string userMessage, string sessionId = null)
        {
            var awsConfig = _config.GetSection("AWS");

            try
            {
                var request = new RecognizeTextRequest
                {
                    BotId = awsConfig["BotId"],
                    BotAliasId = awsConfig["BotAliasId"],
                    LocaleId = awsConfig["LocaleId"],
                    // 如果有提供 sessionId 就用提供的（通常是 MemberId），否則才用隨機 ID
                    SessionId = !string.IsNullOrEmpty(sessionId) ? $"HowBuy_{sessionId}" : "HowBuySession_" + System.Guid.NewGuid().ToString("N"),
                    Text = userMessage
                };

                var response = await _lexClient.RecognizeTextAsync(request);

                // 取得 Lex 的回覆訊息
                if (response.Messages != null && response.Messages.Count > 0)
                {
                    return response.Messages[0].Content;
                }

                return "🤖 [自動回覆] 抱歉，我暫時無法理解您的意思，請稍候由真人為您服務。";
            }
            catch (Amazon.LexRuntimeV2.AmazonLexRuntimeV2Exception ex)
            {
                string errorMsg = $"[AWS Lex Error] Code: {ex.ErrorCode}, Message: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(errorMsg);
                System.Console.WriteLine(errorMsg);
                return $"🤖 [系統訊息] 機器人服務連線失敗 (AWS: {ex.ErrorCode})，請檢查 BotId 或 BotAliasId 是否正確。";
            }
            catch (System.Exception ex)
            {
                string errorMsg = $"[General Bot Error] {ex.Message}";
                System.Diagnostics.Debug.WriteLine(errorMsg);
                System.Console.WriteLine(errorMsg);
                return $"🤖 [系統訊息] 機器人服務發生非預期錯誤 ({ex.Message})，請稍後再試。";
            }
        }
    }
}
