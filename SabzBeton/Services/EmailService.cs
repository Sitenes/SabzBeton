using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using SabzBeton.Models;

namespace SabzBeton.Services
{
    /// <summary>
    /// قرارداد ارسال ایمیل از طریق فرم تماس.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// اطلاعات فرم تماس را به صورت ایمیل HTML به آدرس تعریف‌شده در تنظیمات ارسال می‌کند.
        /// </summary>
        Task SendContactFormEmailAsync(ContactFormModel model, string userIp);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendContactFormEmailAsync(ContactFormModel model, string userIp)
        {
            try
            {
                using var smtpClient = new SmtpClient(_emailSettings.SmtpHost)
                {
                    Port = _emailSettings.SmtpPort,
                    Credentials = new NetworkCredential(_emailSettings.FromEmail, _emailSettings.FromPassword),
                    EnableSsl = _emailSettings.EnableSsl,
                    Timeout = _emailSettings.Timeout
                };
                var emailBody = $@"
<html dir='rtl'>
<head>
    <style>
        body {{ font-family: Tahoma, Arial, sans-serif; direction: rtl; }}
        .container {{ max-width: 600px; margin: 20px auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px; }}
        .header {{ background-color: #2E8B57; color: white; padding: 10px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .field {{ margin: 10px 0; padding: 10px; background-color: white; border-right: 3px solid #2E8B57; }}
        .label {{ font-weight: bold; color: #333; }}
        .value {{ color: #666; margin-top: 5px; }}
        .footer {{ margin-top: 20px; padding: 10px; text-align: center; font-size: 12px; color: #999; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>فرم تماس وب‌سایت سبز بتن - پیام جدید</h2>
        </div>
        <div class='content'>
            <div class='field'>
                <div class='label'>نام:</div>
                <div class='value'>{model.Name}</div>
            </div>
            <div class='field'>
                <div class='label'>ایمیل:</div>
                <div class='value'>{model.Email}</div>
            </div>
            <div class='field'>
                <div class='label'>شماره تماس:</div>
                <div class='value'>{model.Phone}</div>
            </div>
            <div class='field'>
                <div class='label'>موضوع:</div>
                <div class='value'>{model.Subject}</div>
            </div>
            <div class='field'>
                <div class='label'>پیام:</div>
                <div class='value'>{model.Message}</div>
            </div>
            <div class='field'>
                <div class='label'>آدرس IP کاربر:</div>
                <div class='value'>{userIp}</div>
            </div>
            <div class='field'>
                <div class='label'>تاریخ و زمان:</div>
                <div class='value'>{DateTime.Now:yyyy/MM/dd HH:mm:ss}</div>
            </div>
        </div>
        <div class='footer'>
            این پیام از طریق فرم تماس وب‌سایت <a href='https://sabzbeton.ir'>سبز بتن</a> ارسال شده است.
        </div>
    </div>
</body>
</html>";

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail, "وب‌سایت سبز بتن"),
                    Subject = $"تماس جدید: {model.Subject}",
                    Body = emailBody,
                    IsBodyHtml = true,
                    Priority = MailPriority.Normal
                };

                mailMessage.To.Add(_emailSettings.ToEmail);

                // افزودن ایمیل کاربر به ReplyTo برای پاسخ راحت‌تر
                mailMessage.ReplyToList.Add(new MailAddress(model.Email, model.Name));

                await smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation($"Email sent successfully for contact form from {model.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email: {ex.Message}");
                throw;
            }
        }
    }
}
