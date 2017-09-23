using System.Threading.Tasks;

namespace SlideInfo.App.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
