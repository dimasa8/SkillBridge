using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SkillBridge.Data;
using SkillBridge.Models;

var builder = WebApplication.CreateBuilder(args);

// 1) قاعدة البيانات SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2) نظام Identity مع الأدوار
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // إعدادات كلمة السر (خفّفناها شوي للتجربة)
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// 3) مسارات صفحات الدخول
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// تشغيل الـ Seeder عند الإقلاع (ينشئ الأدوار + الأدمن + بيانات تجريبية)
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();
