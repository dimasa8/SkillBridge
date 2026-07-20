using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SkillBridge.Data;
using SkillBridge.Models;

namespace SkillBridge.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ===== لوحة التحكم =====
        public async Task<IActionResult> Index()
        {
            ViewBag.SpecializationCount = await _context.Specializations.CountAsync();
            ViewBag.StageCount = await _context.Stages.CountAsync();
            ViewBag.ResourceCount = await _context.Resources.CountAsync();
            ViewBag.QuestionCount = await _context.Questions.CountAsync();

            var studentIds = await GetUserIdsInRole("Student");
            var companyIds = await GetUserIdsInRole("Company");
            ViewBag.StudentCount = studentIds.Count;
            ViewBag.CompanyCount = companyIds.Count;

            return View();
        }

        // ==================== التخصصات ====================
        public async Task<IActionResult> Specializations()
        {
            var specs = await _context.Specializations
                .Include(s => s.Stages)
                .ToListAsync();
            return View(specs);
        }

        [HttpGet]
        public IActionResult CreateSpecialization() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSpecialization(Specialization model)
        {
            if (!ModelState.IsValid) return View(model);
            _context.Specializations.Add(model);
            await _context.SaveChangesAsync();
            TempData["Message"] = "تمت إضافة التخصص بنجاح.";
            return RedirectToAction(nameof(Specializations));
        }

        [HttpGet]
        public async Task<IActionResult> EditSpecialization(int id)
        {
            var spec = await _context.Specializations.FindAsync(id);
            if (spec == null) return NotFound();
            return View(spec);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSpecialization(Specialization model)
        {
            if (!ModelState.IsValid) return View(model);
            _context.Specializations.Update(model);
            await _context.SaveChangesAsync();
            TempData["Message"] = "تم تحديث التخصص.";
            return RedirectToAction(nameof(Specializations));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSpecialization(int id)
        {
            var spec = await _context.Specializations.FindAsync(id);
            if (spec != null)
            {
                _context.Specializations.Remove(spec);
                await _context.SaveChangesAsync();
                TempData["Message"] = "تم حذف التخصص.";
            }
            return RedirectToAction(nameof(Specializations));
        }

        // ==================== المراحل ====================
        public async Task<IActionResult> Stages(int? specializationId)
        {
            var query = _context.Stages
                .Include(s => s.Specialization)
                .Include(s => s.Resources)
                .Include(s => s.Questions)
                .AsQueryable();

            if (specializationId != null)
                query = query.Where(s => s.SpecializationId == specializationId);

            var stages = await query.OrderBy(s => s.SpecializationId).ThenBy(s => s.Order).ToListAsync();

            ViewBag.Specializations = await _context.Specializations.ToListAsync();
            ViewBag.SelectedSpecialization = specializationId;
            return View(stages);
        }

        [HttpGet]
        public async Task<IActionResult> CreateStage(int? specializationId)
        {
            ViewBag.Specializations = new SelectList(await _context.Specializations.ToListAsync(), "Id", "Name", specializationId);
            return View(new Stage { SpecializationId = specializationId ?? 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStage(Stage model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Specializations = new SelectList(await _context.Specializations.ToListAsync(), "Id", "Name", model.SpecializationId);
                return View(model);
            }
            _context.Stages.Add(model);
            await _context.SaveChangesAsync();
            TempData["Message"] = "تمت إضافة المرحلة.";
            return RedirectToAction(nameof(Stages), new { specializationId = model.SpecializationId });
        }

        [HttpGet]
        public async Task<IActionResult> EditStage(int id)
        {
            var stage = await _context.Stages.FindAsync(id);
            if (stage == null) return NotFound();
            ViewBag.Specializations = new SelectList(await _context.Specializations.ToListAsync(), "Id", "Name", stage.SpecializationId);
            return View(stage);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStage(Stage model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Specializations = new SelectList(await _context.Specializations.ToListAsync(), "Id", "Name", model.SpecializationId);
                return View(model);
            }
            _context.Stages.Update(model);
            await _context.SaveChangesAsync();
            TempData["Message"] = "تم تحديث المرحلة.";
            return RedirectToAction(nameof(Stages), new { specializationId = model.SpecializationId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStage(int id)
        {
            var stage = await _context.Stages.FindAsync(id);
            if (stage != null)
            {
                int specId = stage.SpecializationId;
                _context.Stages.Remove(stage);
                await _context.SaveChangesAsync();
                TempData["Message"] = "تم حذف المرحلة.";
                return RedirectToAction(nameof(Stages), new { specializationId = specId });
            }
            return RedirectToAction(nameof(Stages));
        }

        // ==================== المصادر (روابط/فيديوهات) ====================
        public async Task<IActionResult> Resources(int stageId)
        {
            var stage = await _context.Stages
                .Include(s => s.Resources)
                .Include(s => s.Specialization)
                .FirstOrDefaultAsync(s => s.Id == stageId);
            if (stage == null) return NotFound();
            return View(stage);
        }

        [HttpGet]
        public async Task<IActionResult> CreateResource(int stageId)
        {
            var stage = await _context.Stages.FindAsync(stageId);
            if (stage == null) return NotFound();
            ViewBag.StageTitle = stage.Title;
            return View(new Resource { StageId = stageId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateResource(Resource model)
        {
            if (!ModelState.IsValid)
            {
                var st = await _context.Stages.FindAsync(model.StageId);
                ViewBag.StageTitle = st?.Title;
                return View(model);
            }
            _context.Resources.Add(model);
            await _context.SaveChangesAsync();
            TempData["Message"] = "تمت إضافة المصدر.";
            return RedirectToAction(nameof(Resources), new { stageId = model.StageId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteResource(int id)
        {
            var res = await _context.Resources.FindAsync(id);
            if (res != null)
            {
                int stageId = res.StageId;
                _context.Resources.Remove(res);
                await _context.SaveChangesAsync();
                TempData["Message"] = "تم حذف المصدر.";
                return RedirectToAction(nameof(Resources), new { stageId });
            }
            return RedirectToAction(nameof(Stages));
        }

        // ==================== الأسئلة ====================
        public async Task<IActionResult> Questions(int stageId)
        {
            var stage = await _context.Stages
                .Include(s => s.Questions)
                .Include(s => s.Specialization)
                .FirstOrDefaultAsync(s => s.Id == stageId);
            if (stage == null) return NotFound();
            return View(stage);
        }

        [HttpGet]
        public async Task<IActionResult> CreateQuestion(int stageId)
        {
            var stage = await _context.Stages.FindAsync(stageId);
            if (stage == null) return NotFound();
            ViewBag.StageTitle = stage.Title;
            return View(new Question { StageId = stageId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuestion(Question model)
        {
            if (!ModelState.IsValid)
            {
                var st = await _context.Stages.FindAsync(model.StageId);
                ViewBag.StageTitle = st?.Title;
                return View(model);
            }
            _context.Questions.Add(model);
            await _context.SaveChangesAsync();
            TempData["Message"] = "تمت إضافة السؤال.";
            return RedirectToAction(nameof(Questions), new { stageId = model.StageId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var q = await _context.Questions.FindAsync(id);
            if (q != null)
            {
                int stageId = q.StageId;
                _context.Questions.Remove(q);
                await _context.SaveChangesAsync();
                TempData["Message"] = "تم حذف السؤال.";
                return RedirectToAction(nameof(Questions), new { stageId });
            }
            return RedirectToAction(nameof(Stages));
        }

        // ==================== المستخدمون ====================
        public async Task<IActionResult> Students()
        {
            var ids = await GetUserIdsInRole("Student");
            var students = await _context.Users
                .Include(u => u.Specialization)
                .Where(u => ids.Contains(u.Id))
                .OrderByDescending(u => u.Score)
                .ToListAsync();
            return View(students);
        }

        public async Task<IActionResult> Companies()
        {
            var ids = await GetUserIdsInRole("Company");
            var companies = await _context.Users
                .Where(u => ids.Contains(u.Id))
                .OrderBy(u => u.CompanyName)
                .ToListAsync();
            return View(companies);
        }

        // ==================== مساعدات ====================
        private async Task<List<string>> GetUserIdsInRole(string roleName)
        {
            var roleId = await _context.Roles
                .Where(r => r.Name == roleName)
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            return await _context.UserRoles
                .Where(ur => ur.RoleId == roleId)
                .Select(ur => ur.UserId)
                .ToListAsync();
        }
    }
}
