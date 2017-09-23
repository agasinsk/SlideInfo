using System.Threading.Tasks;

namespace SlideInfo.App.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
