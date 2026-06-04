using System.ComponentModel.DataAnnotations;

namespace SabzBeton.Models
{
    public class ContactFormModel
    {
        [Required(ErrorMessage = "نام الزامی است")]
        [StringLength(100, ErrorMessage = "نام نباید بیشتر از 100 کاراکتر باشد")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "ایمیل الزامی است")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل صحیح نیست")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "شماره تماس الزامی است")]
        [RegularExpression(@"^(\+98|0)?9\d{9}$", ErrorMessage = "فرمت شماره تماس صحیح نیست (مثال: 09123456789)")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "موضوع الزامی است")]
        [StringLength(200, ErrorMessage = "موضوع نباید بیشتر از 200 کاراکتر باشد")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "پیام الزامی است")]
        [StringLength(2000, ErrorMessage = "پیام نباید بیشتر از 2000 کاراکتر باشد")]
        public string Message { get; set; } = string.Empty;
        // توکن reCAPTCHA از فیلد g-recaptcha-response در فرم خوانده می‌شود (مستقیم در ContactController)
    }
}
