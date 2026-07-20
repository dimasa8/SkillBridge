using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SkillBridge.Models;

namespace SkillBridge.Data
{
    public static class DbSeeder
    {
        // بينده مرة وحدة عند تشغيل التطبيق: بيعمل الأدوار + الأدمن + بيانات تجريبية
        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<AppDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // تأكد إنه قاعدة البيانات موجودة (بتنشئها من الموديلات مباشرة بدون migration)
            await context.Database.EnsureCreatedAsync();

            // 1) الأدوار
            string[] roles = { "Admin", "Student", "Company" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // 2) حساب الأدمن (إنتي)
            string adminEmail = "admin@skillbridge.com";
            string adminPassword = "Admin@123";

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "مدير المنصة",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }

            // 3) بيانات تجريبية (تخصصات + مراحل + مصادر + أسئلة) لو القاعدة فاضية
            if (!await context.Specializations.AnyAsync())
            {
                var webDev = new Specialization
                {
                    Name = "تطوير الويب",
                    Icon = "bi-code-slash",
                    Description = "تعلّم بناء مواقع وتطبيقات ويب من الصفر للاحتراف.",
                    Stages = new List<Stage>
                    {
                        new Stage
                        {
                            Title = "أساسيات HTML و CSS",
                            Description = "بناء صفحات ويب وتنسيقها.",
                            Order = 1,
                            Points = 20,
                            Resources = new List<Resource>
                            {
                                new Resource { Title = "مقدمة في HTML", Url = "https://www.youtube.com/watch?v=qz0aGYrrlhU", Type = "video", Description = "شرح أساسيات HTML" },
                                new Resource { Title = "دليل CSS", Url = "https://developer.mozilla.org/en-US/docs/Web/CSS", Type = "article", Description = "توثيق CSS الرسمي" }
                            },
                            Questions = new List<Question>
                            {
                                new Question { Text = "أي وسم يُستخدم لأكبر عنوان في HTML؟", OptionA = "<h1>", OptionB = "<h6>", OptionC = "<head>", OptionD = "<title>", CorrectOption = "A", Points = 10 },
                                new Question { Text = "أي خاصية CSS تُغيّر لون النص؟", OptionA = "background", OptionB = "color", OptionC = "font", OptionD = "text", CorrectOption = "B", Points = 10 }
                            }
                        },
                        new Stage
                        {
                            Title = "أساسيات JavaScript",
                            Description = "إضافة التفاعل للصفحات.",
                            Order = 2,
                            Points = 30,
                            Resources = new List<Resource>
                            {
                                new Resource { Title = "دورة JavaScript للمبتدئين", Url = "https://www.youtube.com/watch?v=W6NZfCO5SIk", Type = "video", Description = "مقدمة شاملة" }
                            },
                            Questions = new List<Question>
                            {
                                new Question { Text = "أي كلمة تُستخدم للتصريح عن متغير ثابت؟", OptionA = "var", OptionB = "let", OptionC = "const", OptionD = "static", CorrectOption = "C", Points = 15 }
                            }
                        }
                    }
                };

                var ai = new Specialization
                {
                    Name = "الذكاء الاصطناعي",
                    Icon = "bi-robot",
                    Description = "تعلّم أساسيات الذكاء الاصطناعي وتعلّم الآلة.",
                    Stages = new List<Stage>
                    {
                        new Stage
                        {
                            Title = "مقدمة في تعلّم الآلة",
                            Description = "المفاهيم الأساسية.",
                            Order = 1,
                            Points = 25,
                            Resources = new List<Resource>
                            {
                                new Resource { Title = "ما هو تعلّم الآلة؟", Url = "https://www.youtube.com/watch?v=ukzFI9rgwfU", Type = "video", Description = "شرح مبسّط" }
                            },
                            Questions = new List<Question>
                            {
                                new Question { Text = "أي نوع تعلّم يعتمد على بيانات مُصنّفة؟", OptionA = "غير موجّه", OptionB = "موجّه", OptionC = "تعزيزي", OptionD = "عميق", CorrectOption = "B", Points = 15 }
                            }
                        }
                    }
                };

                context.Specializations.AddRange(webDev, ai);
                await context.SaveChangesAsync();
            }
        }
    }
}
