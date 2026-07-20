using System.ComponentModel.DataAnnotations;

namespace SkillBridge.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "بريد إلكتروني غير صالح")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; } = "";

        [Display(Name = "تذكّرني")]
        public bool RememberMe { get; set; }
    }
}
