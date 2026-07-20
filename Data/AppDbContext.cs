using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SkillBridge.Models;

namespace SkillBridge.Data
{
    // بيورث IdentityDbContext عشان يجيب جداول المستخدمين والأدوار جاهزة
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Specialization> Specializations { get; set; }
        public DbSet<Stage> Stages { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<StudentProgress> StudentProgress { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // لما ينحذف تخصص تنحذف مراحله
            builder.Entity<Stage>()
                .HasOne(s => s.Specialization)
                .WithMany(sp => sp.Stages)
                .HasForeignKey(s => s.SpecializationId)
                .OnDelete(DeleteBehavior.Cascade);

            // لما تنحذف مرحلة تنحذف مصادرها
            builder.Entity<Resource>()
                .HasOne(r => r.Stage)
                .WithMany(s => s.Resources)
                .HasForeignKey(r => r.StageId)
                .OnDelete(DeleteBehavior.Cascade);

            // لما تنحذف مرحلة تنحذف أسئلتها
            builder.Entity<Question>()
                .HasOne(q => q.Stage)
                .WithMany(s => s.Questions)
                .HasForeignKey(q => q.StageId)
                .OnDelete(DeleteBehavior.Cascade);

            // علاقة تقدّم الطالب
            builder.Entity<StudentProgress>()
                .HasOne(p => p.User)
                .WithMany(u => u.ProgressList)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentProgress>()
                .HasOne(p => p.Stage)
                .WithMany()
                .HasForeignKey(p => p.StageId)
                .OnDelete(DeleteBehavior.Restrict);

            // علاقة الطالب بالتخصص بدون حذف متسلسل
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Specialization)
                .WithMany()
                .HasForeignKey(u => u.SpecializationId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
