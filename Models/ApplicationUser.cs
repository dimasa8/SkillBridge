using Microsoft.AspNetCore.Identity;

namespace SkillBridge.Models
{
    // المستخدم الأساسي: بيصير طالب أو شركة أو أدمن حسب الـ Role
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = "";

        // للطالب: التخصص اللي اختاره
        public int? SpecializationId { get; set; }
        public Specialization? Specialization { get; set; }

        // للطالب: مجموع نقاطه
        public int Score { get; set; }

        // للشركة: اسم الشركة
        public string? CompanyName { get; set; }

        // تاريخ التسجيل
        public DateTime JoinedAt { get; set; } = DateTime.Now;

        // العلاقة: تقدّم الطالب بالمراحل
        public List<StudentProgress> ProgressList { get; set; } = new List<StudentProgress>();
    }
}
