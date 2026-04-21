using System.Threading.Tasks;

namespace ISpanShop.Services.Communication
{
    public interface IBotService
    {
        // 取得機器人的回覆文字
        Task<string> GetResponseAsync(string userMessage);
    }

    public class MockBotService : IBotService
    {
        public Task<string> GetResponseAsync(string userMessage)
        {
            string response = "🤖 [自動回覆] 您好！我是好聊 AI 助理。您的訊息店主已收到，會盡快親自回覆您！";

            // 簡單的關鍵字比對邏輯
            if (userMessage.Contains("現貨"))
            {
                response = "🛍️ 您好！本賣場商品 90% 以上皆為現貨，下單後 24-48 小時內會為您安排出貨喔！";
            }
            else if (userMessage.Contains("打折") || userMessage.Contains("便宜") || userMessage.Contains("優惠"))
            {
                response = "💰 您好！目前全店滿千免運，關注賣場還可以領取隱藏版折價券喔，快去首頁看看！";
            }
            else if (userMessage.Contains("你好") || userMessage.Contains("在嗎"))
            {
                response = "😊 您好！很高興為您服務！請問有什麼我可以幫您的？或是您可以先留下您的問題。";
            }
            else if (userMessage.Contains("運費"))
            {
                response = "🚚 目前本賣場配合 HowBuy 全站活動，滿 $999 享免運優惠！";
            }

            return Task.FromResult(response);
        }
    }
}
