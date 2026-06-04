namespace SabzBeton.Models
{
    public class ReCaptchaSettings
    {
        /// <summary>کلید مخفیانه reCAPTCHA برای اعتبارسنجی سمت سرور از طریق Google siteverify API.</summary>
        public string SecretKey { get; set; } = string.Empty;
    }
}
