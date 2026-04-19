using System.Threading.Tasks;

namespace ISpanShop.Services.Communication
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
    }
}
