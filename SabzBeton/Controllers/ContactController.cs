using Microsoft.AspNetCore.Mvc;
using SabzBeton.Models;
using SabzBeton.Services;

namespace SabzBeton.Controllers
{
    public class ContactController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly IReCaptchaService _reCaptchaService;
        private readonly ILogger<ContactController> _logger;

        // حافظه موقت برای ذخیره اطلاعات IP و زمان ارسال
        private static Dictionary<string, DateTime> ipSubmissionTimes = new Dictionary<string, DateTime>();

        public ContactController(
            IEmailService emailService,
            IReCaptchaService reCaptchaService,
            ILogger<ContactController> logger)
        {
            _emailService = emailService;
            _reCaptchaService = reCaptchaService;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitForm([FromForm] ContactFormModel model)
        {
            // گرفتن IP کاربر
            var userIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            try
            {
                // بررسی اعتبار مدل
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    _logger.LogWarning($"Invalid model state: {errors}");
                    return BadRequest(new { success = false, message = $"اطلاعات فرم نامعتبر است: {errors}" });
                }

                // بررسی reCAPTCHA — توکن را از فیلد g-recaptcha-response که widget به صورت خودکار اضافه می‌کند می‌خوانیم
                var captchaToken = Request.Form["g-recaptcha-response"].ToString();
                var isCaptchaValid = await _reCaptchaService.VerifyTokenAsync(captchaToken);
                if (!isCaptchaValid)
                {
                    _logger.LogWarning($"reCAPTCHA verification failed for IP: {userIp}");
                    return BadRequest(new { success = false, message = "تایید CAPTCHA ناموفق بود. لطفاً دوباره تلاش کنید." });
                }

                // بررسی محدودیت زمانی (هر 12 ساعت یک بار)
                if (ipSubmissionTimes.ContainsKey(userIp))
                {
                    var lastSubmissionTime = ipSubmissionTimes[userIp];
                    if (DateTime.Now < lastSubmissionTime.AddHours(12))
                    {
                        var remainingTime = lastSubmissionTime.AddHours(12) - DateTime.Now;
                        _logger.LogWarning($"Rate limit exceeded for IP: {userIp}");
                        return BadRequest(new
                        {
                            success = false,
                            message = $"شما قبلاً فرم را ارسال کرده‌اید. لطفاً {remainingTime.Hours} ساعت و {remainingTime.Minutes} دقیقه دیگر تلاش کنید."
                        });
                    }
                }

                // ارسال ایمیل
                await _emailService.SendContactFormEmailAsync(model, userIp);

                // ذخیره زمان ارسال فرم برای IP
                ipSubmissionTimes[userIp] = DateTime.Now;

                _logger.LogInformation($"Contact form submitted successfully from IP: {userIp}, Email: {model.Email}");

                return Ok(new { success = true, message = "پیام شما با موفقیت ارسال شد. به زودی با شما تماس خواهیم گرفت." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing contact form from IP {userIp}: {ex.Message}");
                return StatusCode(500, new { success = false, message = "خطا در ارسال پیام. لطفاً بعداً تلاش کنید." });
            }
        }
    }
}