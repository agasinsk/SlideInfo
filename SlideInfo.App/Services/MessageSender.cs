using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace SlideInfo.App.Services
{
    public class MessageSender : IEmailSender, ISmsSender
    {
        public MessageSender(IOptions<MessageSenderOptions> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }

        public MessageSenderOptions Options { get; } //set only via Secret Manager
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var msg = EmailSender.CreateEmailMessage(email, subject, message);
            return EmailSender.SendEmailAsync(msg);
        }

        public Task SendSmsAsync(string number, string message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }
}
