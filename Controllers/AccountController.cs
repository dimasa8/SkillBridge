using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SkillBridge.Models;
using SkillBridge.ViewModels;

namespace SkillBridge.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // ===== التسجيل =====
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // لو شركة لازم اسم الشركة
            if (model.Role == "Company" && string.IsNullOrWhiteSpace(model.CompanyName))
            {
                ModelState.AddModelError("CompanyName", "اسم الشركة مطلوب");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                CompanyName = model.Role == "Company" ? model.CompanyName : null,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // إعطاء الدور المناسب
                await _userManager.AddToRoleAsync(user, model.Role);
                await _signInManager.SignInAsync(user, isPersistent: false);

                // توجيه حسب الدور
                if (model.Role == "Company")
                    return RedirectToAction("Index", "Company");
                return RedirectToAction("Index", "Student");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, TranslateError(error.Code, error.Description));

            return View(model);
        }

        // ===== الدخول =====
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "البريد الإلكتروني أو كلمة المرور غير صحيحة");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // توجيه حسب الدور
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    return RedirectToAction("Index", "Admin");
                if (await _userManager.IsInRoleAsync(user, "Company"))
                    return RedirectToAction("Index", "Company");
                return RedirectToAction("Index", "Student");
            }

            ModelState.AddModelError(string.Empty, "البريد الإلكتروني أو كلمة المرور غير صحيحة");
            return View(model);
        }

        // ===== الخروج =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // ترجمة أخطاء Identity الشائعة للعربي
        private string TranslateError(string code, string fallback)
        {
            return code switch
            {
                "DuplicateUserName" => "هذا البريد الإلكتروني مسجّل مسبقاً",
                "DuplicateEmail" => "هذا البريد الإلكتروني مسجّل مسبقاً",
                "PasswordTooShort" => "كلمة المرور قصيرة جداً",
                _ => fallback
            };
        }
    }
}
