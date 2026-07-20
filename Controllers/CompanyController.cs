using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillBridge.Data;
using SkillBridge.Models;

namespace SkillBridge.Controllers
{
    [Authorize(Roles = "Company")]
    public class CompanyController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CompanyController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // لوحة الشركة: التخصصات + أفضل الطلاب
        public async Task<IActionResult> Index()
        {
            var specializations = await _context.Specializations
                .Include(s => s.Stages)
                .ToListAsync();

            // أفضل الطلاب حسب السكور
            var topStudents = await GetStudentsQuery()
                .OrderByDescending(u => u.Score)
                .Take(5)
                .ToListAsync();

            ViewBag.TopStudents = topStudents;
            return View(specializations);
        }

        // تصفّح الطلاب مع فلترة بالتخصص
        public async Task<IActionResult> Students(int? specializationId)
        {
            var query = GetStudentsQuery();

            if (specializationId != null)
                query = query.Where(u => u.SpecializationId == specializationId);

            var students = await query
                .Include(u => u.Specialization)
                .OrderByDescending(u => u.Score)
                .ToListAsync();

            ViewBag.Specializations = await _context.Specializations.ToListAsync();
            ViewBag.SelectedSpecialization = specializationId;

            return View(students);
        }

        // ملف طالب مفصّل (للتواصل)
        public async Task<IActionResult> StudentProfile(string id)
        {
            var student = await _context.Users
                .Include(u => u.Specialization)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (student == null) return NotFound();

            // تأكد إنه فعلاً طالب
            if (!await _userManager.IsInRoleAsync(student, "Student"))
                return NotFound();

            var progress = await _context.StudentProgress
                .Include(p => p.Stage)
                .Where(p => p.UserId == id)
                .OrderByDescending(p => p.CompletedAt)
                .ToListAsync();

            ViewBag.Progress = progress;

            return View(student);
        }

        // استعلام يجيب الطلاب فقط
        private IQueryable<ApplicationUser> GetStudentsQuery()
        {
            var studentRoleId = _context.Roles
                .Where(r => r.Name == "Student")
                .Select(r => r.Id)
                .FirstOrDefault();

            var studentIds = _context.UserRoles
                .Where(ur => ur.RoleId == studentRoleId)
                .Select(ur => ur.UserId);

            return _context.Users.Where(u => studentIds.Contains(u.Id));
        }
    }
}
