namespace AuthProject.Models // فضای نام مربوط به مدل‌های پروژه
{
    // تعریف کلاس User برای نمایش یک کاربر در سیستم
    public class User
    {
        // شناسه منحصر به‌فرد کاربر
        public int Id { get; set; }

        // نام کاربری که به صورت یکتا برای هر کاربر تعریف می‌شود
        public string Username { get; set; }

        // هش رمز عبور کاربر برای ذخیره‌سازی امن در دیتابیس
        public string PasswordHash { get; set; }

        // نقش کاربر (مانند مدیر، کاربر عادی و غیره) برای مدیریت سطح دسترسی
        public string Role { get; set; }
    }
}
