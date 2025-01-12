using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AuthProject.Data;
using AuthProject.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// افزودن سرویس‌های مربوط به کنترلرها
builder.Services.AddControllers(); // این خط را اضافه کنید

// اضافه کردن سرویس gRPC
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
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// پیکربندی سیاست‌های مجوزدهی
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
});

// افزودن سرویس‌های Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "AuthProject API", Version = "v1" });
});

var app = builder.Build();

// Middlewareها
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// فعال‌سازی Swagger فقط در محیط Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthProject API V1");
    });
}

// نگاشت سرویس gRPC به اپلیکیشن
app.MapGrpcService<AuthService>();

// نگاشت کنترلرها
app.MapControllers(); // این خط را اضافه کنید

app.Run();