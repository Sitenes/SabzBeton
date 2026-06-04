using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using SabzBeton.Models;

namespace SabzBeton.Services
{
    /// <summary>
    /// قرارداد اعتبارسنجی توکن reCAPTCHA از طریق Google siteverify API.
    /// </summary>
    public interface IReCaptchaService
    {
        /// <summary>
        /// توکن دریافت‌شده از فرم (g-recaptcha-response) را با استفاده از SecretKey اعتبارسنجی می‌کند.
        /// </summary>
        Task<bool> VerifyTokenAsync(string token);
    }

    public class ReCaptchaService : IReCaptchaService
    {
        private const string SiteVerifyUrl = "https://www.google.com/recaptcha/api/siteverify";

        private readonly ReCaptchaSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ReCaptchaService> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ReCaptchaService(
            IOptions<ReCaptchaSettings> settings,
            IHttpClientFactory httpClientFactory,
            ILogger<ReCaptchaService> logger)
        {
            _settings = settings.Value;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<bool> VerifyTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("reCAPTCHA: توکن خالی یا null دریافت شد.");
                return false;
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient();

                var formContent = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["secret"] = _settings.SecretKey,
                    ["response"] = token
                });

                var response = await httpClient.PostAsync(SiteVerifyUrl, formContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("reCAPTCHA siteverify: HTTP {StatusCode}.", (int)response.StatusCode);
                    return false;
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<SiteVerifyResponse>(json, _jsonOptions);

                if (result?.Success != true)
                {
                    _logger.LogWarning("reCAPTCHA siteverify: توکن نامعتبر. ErrorCodes: {Codes}",
                        string.Join(", ", result?.ErrorCodes ?? Array.Empty<string>()));
                    return false;
                }

                _logger.LogInformation("reCAPTCHA: اعتبارسنجی موفق. Score={Score}", result.Score);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "reCAPTCHA: خطای غیرمنتظره در حین اعتبارسنجی.");
                return false;
            }
        }

        // ─── Response DTO ─────────────────────────────────────────────────────────────

        private sealed class SiteVerifyResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("score")]
            public float Score { get; set; }

            [JsonPropertyName("error-codes")]
            public string[]? ErrorCodes { get; set; }
        }
    }
}
