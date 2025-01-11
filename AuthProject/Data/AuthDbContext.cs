using Microsoft.EntityFrameworkCore; // کتابخانه مربوط به Entity Framework Core برای مدیریت ارتباطات با دیتابیس
using AuthProject.Models; // فضای نام مربوط به مدل‌های پروژه

namespace AuthProject.Data // فضای نام برای دسترسی به داده‌ها و مدیریت دیتابیس
{
    // تعریف کلاس AuthDbContext برای مدیریت کانتکست دیتابیس
    public class AuthDbContext : DbContext
    {
        // سازنده کلاس که تنظیمات مربوط به DbContext را از بیرون دریافت می‌کند
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        // تعریف DbSet برای جدول Users که به مدل User نگاشت شده است
        public DbSet<User> Users { get; set; }

        // متد OnModelCreating برای تعریف تنظیمات و قوانین خاص مربوط به مدل‌ها
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // تنظیمات مربوط به مدل User:
            // ایجاد یک ایندکس منحصربه‌فرد روی فیلد Username
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        }
    }
}
