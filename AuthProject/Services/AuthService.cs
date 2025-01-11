using AuthProject.Data; // دسترسی به پایگاه داده پروژه
using AuthProject.Models; // استفاده از مدل‌های مرتبط با کاربران
using Grpc.Core; // استفاده از gRPC برای تعریف سرویس‌های ارتباطی
using Microsoft.IdentityModel.Tokens; // مدیریت توکن‌های JWT
using System.IdentityModel.Tokens.Jwt; // تولید و مدیریت JWT
using System.Security.Claims; // مدیریت ادعاها (Claims) در توکن‌ها
using System.Text; // برای کار با رشته‌ها
using Microsoft.EntityFrameworkCore; // برای ارتباط با پایگاه داده

namespace AuthProject.Services
{
    // تعریف سرویس gRPC برای عملیات احراز هویت
    public class AuthService : AuthProject.AuthService.AuthServiceBase
    {
        private readonly AuthDbContext _context; // کانتکست پایگاه داده
        private readonly IConfiguration _configuration; // تنظیمات پروژه (مانند کلید JWT)

        // سازنده کلاس برای مقداردهی اولیه کانتکست و تنظیمات
        public AuthService(AuthDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // متد Login برای ورود کاربران
        public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
        {
            // بررسی وجود کاربر با نام کاربری مشخص
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            // اگر کاربر یافت نشد یا رمز عبور اشتباه بود
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid credentials"));
            }

            // تولید توکن JWT برای کاربر
            var token = GenerateJwtToken(user);

            // بازگشت توکن به عنوان پاسخ
            return new LoginResponse { Token = token };
        }

        // متد Register برای ثبت‌نام کاربران جدید
        public override async Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            // بررسی تکراری نبودن نام کاربری
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Username already exists"));
            }

            // ایجاد شی جدید از کاربر
            var user = new User
            {
                Username = request.Username, // ذخیره نام کاربری
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password), // هش کردن رمز عبور
                Role = request.Role // نقش کاربر
            };

            // افزودن کاربر به پایگاه داده و ذخیره تغییرات
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // بازگشت پاسخ موفقیت‌آمیز
            return new RegisterResponse { Success = true };
        }

        // متد خصوصی برای تولید توکن JWT
        private string GenerateJwtToken(User user)
        {
            // ایجاد کلید امنیتی با استفاده از تنظیمات پروژه
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            // تعیین الگوریتم امضای توکن
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // تعریف ادعاهای توکن (مانند نام و نقش کاربر)
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username), // نام کاربر
                new Claim(ClaimTypes.Role, user.Role) // نقش کاربر
            };

            // ایجاد شی توکن با مشخصات لازم
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"], // صادرکننده توکن
                audience: _configuration["Jwt:Audience"], // مصرف‌کننده توکن
                claims: claims, // لیست ادعاها
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"])), // زمان انقضا
                signingCredentials: credentials // اطلاعات امضای توکن
            );

            // تبدیل شی توکن به رشته و بازگشت آن
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
