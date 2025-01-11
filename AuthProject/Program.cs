using Microsoft.AspNetCore.Authentication.JwtBearer; // اضافه کردن احراز هویت مبتنی بر JWT
using Microsoft.IdentityModel.Tokens; // مدیریت و اعتبارسنجی توکن‌های JWT
using System.Text; // برای کار با رمزنگاری و کدگذاری رشته‌ها
using AuthProject.Data; // دسترسی به دیتابیس پروژه
using AuthProject.Services; // دسترسی به سرویس‌های پروژه
using Microsoft.EntityFrameworkCore; // برای ارتباط با پایگاه داده

var builder = WebApplication.CreateBuilder(args);

// افزودن سرویس‌ها به کانتینر

// اضافه کردن سرویس gRPC برای مدیریت ارتباطات گرافیکی
builder.Services.AddGrpc();

// پیکربندی دیتابیس با استفاده از Entity Framework Core
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// پیکربندی احراز هویت با استفاده از JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // بررسی معتبر بودن صادرکننده توکن
            ValidateAudience = true, // بررسی معتبر بودن مصرف‌کننده توکن
            ValidateLifetime = true, // بررسی انقضای توکن
            ValidateIssuerSigningKey = true, // بررسی کلید امضای صادرکننده توکن
            ValidIssuer = builder.Configuration["Jwt:Issuer"], // تعریف صادرکننده معتبر
            ValidAudience = builder.Configuration["Jwt:Audience"], // تعریف مصرف‌کننده معتبر
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])) // کلید امضای توکن
        };
    });

// پیکربندی سیاست‌های مجوزدهی
builder.Services.AddAuthorization(options =>
{
    // سیاستی که فقط به کاربران با نقش "Admin" اجازه دسترسی می‌دهد
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

    // سیاستی که فقط به کاربران با نقش "User" اجازه دسترسی می‌دهد
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
});

var app = builder.Build();

// میدلورهای برنامه

// فعال کردن احراز هویت در سطح اپلیکیشن
app.UseAuthentication();

// فعال کردن مجوزدهی برای دسترسی‌ها
app.UseAuthorization();

// نگاشت سرویس gRPC به اپلیکیشن
app.MapGrpcService<AuthService>();

// شروع به اجرای اپلیکیشن
app.Run();
