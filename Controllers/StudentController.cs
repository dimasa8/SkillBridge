using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillBridge.Data;
using SkillBridge.Models;

namespace SkillBridge.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // لوحة الطالب: لو ما اختار تخصص يوجهه لاختيار تخصص، غير هيك يعرض المراحل
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (user.SpecializationId == null)
                return RedirectToAction(nameof(ChooseSpecialization));

            var specialization = await _context.Specializations
                .Include(s => s.Stages.OrderBy(st => st.Order))
                    .ThenInclude(st => st.Resources)
                .Include(s => s.Stages)
                    .ThenInclude(st => st.Questions)
                .FirstOrDefaultAsync(s => s.Id == user.SpecializationId);

            // المراحل اللي خلّصها
            var completedStageIds = await _context.StudentProgress
                .Where(p => p.UserId == user.Id)
                .Select(p => p.StageId)
                .ToListAsync();

            ViewBag.CompletedStageIds = completedStageIds;
            ViewBag.Score = user.Score;
            ViewBag.FullName = user.FullName;

            return View(specialization);
        }

        // اختيار التخصص
        [HttpGet]
        public async Task<IActionResult> ChooseSpecialization()
        {
            var specializations = await _context.Specializations
                .Include(s => s.Stages)
                .ToListAsync();
            return View(specializations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChooseSpecialization(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            user.SpecializationId = id;
            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }

        // صفحة المرحلة: تعرض الفيديوهات والمصادر + زر بدء الاختبار
        public async Task<IActionResult> Stage(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var stage = await _context.Stages
                .Include(s => s.Resources)
                .Include(s => s.Questions)
                .Include(s => s.Specialization)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (stage == null) return NotFound();

            // تأكد إنه المرحلة من تخصص الطالب
            if (stage.SpecializationId != user.SpecializationId)
                return Forbid();

            bool completed = await _context.StudentProgress
                .AnyAsync(p => p.UserId == user.Id && p.StageId == id);

            ViewBag.Completed = completed;

            return View(stage);
        }

        // بدء الاختبار
        public async Task<IActionResult> Quiz(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var stage = await _context.Stages
                .Include(s => s.Questions)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (stage == null) return NotFound();
            if (stage.SpecializationId != user.SpecializationId) return Forbid();

            if (!stage.Questions.Any())
            {
                TempData["Message"] = "لا توجد أسئلة لهذه المرحلة بعد.";
                return RedirectToAction(nameof(Stage), new { id });
            }

            return View(stage);
        }

        // تصحيح الاختبار وحساب النقاط
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitQuiz(int stageId, Dictionary<int, string> answers)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var stage = await _context.Stages
                .Include(s => s.Questions)
                .FirstOrDefaultAsync(s => s.Id == stageId);

            if (stage == null) return NotFound();
            if (stage.SpecializationId != user.SpecializationId) return Forbid();

            int earned = 0;
            int total = 0;
            int correctCount = 0;

            foreach (var q in stage.Questions)
            {
                total += q.Points;
                if (answers != null && answers.TryGetValue(q.Id, out var chosen) && chosen == q.CorrectOption)
                {
                    earned += q.Points;
                    correctCount++;
                }
            }

            // هل خلّص المرحلة من قبل؟
            bool already = await _context.StudentProgress
                .AnyAsync(p => p.UserId == user.Id && p.StageId == stageId);

            if (!already)
            {
                _context.StudentProgress.Add(new StudentProgress
                {
                    UserId = user.Id,
                    StageId = stageId,
                    PointsEarned = earned,
                    CompletedAt = DateTime.UtcNow
                });

                user.Score += earned;
                await _userManager.UpdateAsync(user);
                await _context.SaveChangesAsync();
            }

            ViewBag.Earned = earned;
            ViewBag.Total = total;
            ViewBag.CorrectCount = correctCount;
            ViewBag.QuestionCount = stage.Questions.Count;
            ViewBag.StageId = stageId;
            ViewBag.AlreadyCompleted = already;

            return View("QuizResult");
        }

        // الملف الشخصي والتقدّم
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var progress = await _context.StudentProgress
                .Include(p => p.Stage)
                .Where(p => p.UserId == user.Id)
                .OrderByDescending(p => p.CompletedAt)
                .ToListAsync();

            var specialization = user.SpecializationId != null
                ? await _context.Specializations.FindAsync(user.SpecializationId)
                : null;

            ViewBag.Specialization = specialization;
            ViewBag.Score = user.Score;
            ViewBag.FullName = user.FullName;
            ViewBag.Email = user.Email;

            return View(progress);
        }
    }
}
