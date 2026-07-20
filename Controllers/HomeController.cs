using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillBridge.Data;
using SkillBridge.Models;

namespace SkillBridge.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            AppDbContext context,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // لو مسجّل دخول وجّهه للوحته
            if (_signInManager.IsSignedIn(User))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                        return RedirectToAction("Index", "Admin");
                    if (await _userManager.IsInRoleAsync(user, "Company"))
                        return RedirectToAction("Index", "Company");
                    return RedirectToAction("Index", "Student");
                }
            }

            // للزوّار: نعرض التخصصات المتاحة
            var specializations = await _context.Specializations
                .Include(s => s.Stages)
                .ToListAsync();

            return View(specializations);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
