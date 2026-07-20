using System.ComponentModel.DataAnnotations;

namespace SkillBridge.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "الاسم مطلوب")]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "بريد إلكتروني غير صالح")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمة المرور يجب أن تكون 6 أحرف على الأقل")]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; } = "";

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "كلمتا المرور غير متطابقتين")]
        [Display(Name = "تأكيد كلمة المرور")]
        public string ConfirmPassword { get; set; } = "";

        // نوع الحساب: Student أو Company
        [Required(ErrorMessage = "اختر نوع الحساب")]
        [Display(Name = "نوع الحساب")]
        public string Role { get; set; } = "Student";

        // للشركة فقط
        [Display(Name = "اسم الشركة")]
        public string? CompanyName { get; set; }
    }
}
