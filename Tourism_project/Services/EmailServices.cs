
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Reflection;
using Tourism_project.Settings;



namespace Tourism_project.Services
{
    public class EmailServices : IEmailServices
    {
        private readonly IOptions<EmailSetting> _emailSetting;

        public EmailServices(IOptions<EmailSetting> emailSetting)
        {
            _emailSetting = emailSetting;
        }


        public async Task SendVerificationEmail(string toEmail, string subject, string textBody, string htmlBody)
        {
            var emailMessage = new MimeMessage();

            // إعداد المرسل
            emailMessage.From.Add(new MailboxAddress(_emailSetting.Value.DisplayName, _emailSetting.Value.Email));

            // إعداد المستقبل
            emailMessage.To.Add(MailboxAddress.Parse(toEmail));
            emailMessage.Subject = subject;


            // إعداد محتوى الرسالة
            var bodyBuilder = new BodyBuilder
            {
                TextBody = textBody,
                HtmlBody = htmlBody
            };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            // إرسال البريد
            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_emailSetting.Value.Host, _emailSetting.Value.port, false);
                await client.AuthenticateAsync(_emailSetting.Value.Email, _emailSetting.Value.Password);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }






    }
}

