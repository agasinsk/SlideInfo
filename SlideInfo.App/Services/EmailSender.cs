using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace SlideInfo.App.Services
{
    public class EmailSender
    {
        static readonly SecureSocketOptions secureSocketOptions = SecureSocketOptions.SslOnConnect;

        public static async void SendEmailAsync(string fromEmailAddress, string toEmailAddress, string subject, string bodyText, int retryCount = 4)
        {
            var message = CreateEmailMessage(fromEmailAddress, toEmailAddress, subject, bodyText);

            for (var count = 1; count <= retryCount; count++)
            {
                try
                {
                    using (var client = new SmtpClient())
                    {
                        await client.ConnectAsync(MessageConstants.EMAIL_HOST, MessageConstants.EMAIL_PORT,
                            secureSocketOptions).ConfigureAwait(false);
                        await client.AuthenticateAsync(MessageConstants.EMAIL_USERNAME, MessageConstants.EMAIL_PASSWORD);
                        await client.SendAsync(message).ConfigureAwait(false);
                        await client.DisconnectAsync(true).ConfigureAwait(false);
                        return;
                    }
                }
                catch (Exception)
                {
                    if (retryCount >= 0)
                    {
                        throw;
                    }
                    await Task.Delay(count * 1000);
                }
            }
        }

        private static MimeMessage CreateEmailMessage(string fromEmailAddress, string toEmailAddress, string subject, string bodyText)
        {
            var message = new MimeMessage();

            var fromMailboxAddress = new MailboxAddress(fromEmailAddress);
            message.From.Add(fromMailboxAddress);

            var toMailboxAdress = new MailboxAddress(toEmailAddress);
            message.To.Add(toMailboxAdress);

            message.Subject = subject;
            var builder = new BodyBuilder
            {
                TextBody = bodyText,
            };
            message.Body = builder.ToMessageBody();
            return message;
        }

        public static async void SendEmailAsync(string toEmailAddress, string subject, string bodyText, int retryCount = 4)
        {
            var message = CreateEmailMessage(toEmailAddress, subject, bodyText);

            for (var count = 1; count <= retryCount; count++)
            {
                try
                {
                    using (var client = new SmtpClient())
                    {
                        await client.ConnectAsync(MessageConstants.EMAIL_HOST, MessageConstants.EMAIL_PORT,
                                secureSocketOptions).ConfigureAwait(false);
                        await client.AuthenticateAsync(MessageConstants.EMAIL_USERNAME, MessageConstants.EMAIL_PASSWORD);
                        await client.SendAsync(message).ConfigureAwait(false);
                        await client.DisconnectAsync(true).ConfigureAwait(false);
                        return;
                    }
                }
                catch (Exception)
                {
                    if (retryCount >= 0)
                    {
                        throw;
                    }
                    await Task.Delay(count * 1000);
                }
            }
        }

        public static MimeMessage CreateEmailMessage(string toName, string toEmailAddress, string subject, string bodyHtml,
            string bodyText)
        {
            var message = new MimeMessage();

            var fromMailboxAddress = new MailboxAddress(MessageConstants.APP_NAME,
                MessageConstants.APP_EMAIL);
            message.From.Add(fromMailboxAddress);

            var toMailboxAdress = new MailboxAddress(toName, toEmailAddress);
            message.To.Add(toMailboxAdress);

            message.Subject = subject;
            var builder = new BodyBuilder
            {
                TextBody = bodyText,
                HtmlBody = bodyHtml
            };
            message.Body = builder.ToMessageBody();
            return message;
        }

        public static async Task SendEmailAsync(MimeMessage message, int retryCount = 4)
        {
           for (var count = 1; count <= retryCount; count++)
            {
                try
                {
                    using (var client = new SmtpClient())
                    {
                        await client.ConnectAsync(MessageConstants.EMAIL_HOST, MessageConstants.EMAIL_PORT,
                                secureSocketOptions)
                            .ConfigureAwait(false);
                        await client.AuthenticateAsync(MessageConstants.EMAIL_USERNAME, MessageConstants.EMAIL_PASSWORD);
                        await client.SendAsync(message).ConfigureAwait(false);
                        await client.DisconnectAsync(true).ConfigureAwait(false);
                        return;
                    }
                }
                catch (Exception)
                {
                    if (retryCount >= 0)
                    {
                        throw;
                    }
                    await Task.Delay(count * 1000);
                }
            }
        }

        public static MimeMessage CreateEmailMessage(string toEmailAddress, string subject, string bodyText)
        {
            var message = new MimeMessage();

            var fromMailboxAddress = new MailboxAddress(MessageConstants.APP_NAME,
                MessageConstants.APP_EMAIL);
            message.From.Add(fromMailboxAddress);

            var toMailboxAdress = new MailboxAddress(toEmailAddress);
            message.To.Add(toMailboxAdress);

            message.Subject = subject;
            var builder = new BodyBuilder
            {
                HtmlBody = bodyText
            };
            message.Body = builder.ToMessageBody();
            return message;
        }
    }
}
