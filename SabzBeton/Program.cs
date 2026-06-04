using SabzBeton.Models;
using SabzBeton.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Email Settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Configure ReCaptcha Settings
builder.Services.Configure<ReCaptchaSettings>(builder.Configuration.GetSection("ReCaptcha"));

// Register services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IReCaptchaService, ReCaptchaService>();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

// ✅ تنظیم درست فایل‌های استاتیک با پشتیبانی از Range
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // اجازه کش کردن فایل‌های ویدئو برای 7 روز
        var headers = ctx.Context.Response.Headers;
        var contentType = headers["Content-Type"].ToString();
        if (contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
        {
            headers["Cache-Control"] = "public, max-age=604800";
        }
    }
});

// اگر فایل‌های استاتیک رو از مسیر پیش‌فرض (wwwroot) سرویس می‌کنید
// مطمئن بشید فایل assets داخل پوشه wwwroot هست

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();