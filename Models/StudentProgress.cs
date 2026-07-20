namespace SkillBridge.Models
{
    // سجل: أي طالب خلّص أي مرحلة وكم نقطة أخذ فيها
    public class StudentProgress
    {
        public int Id { get; set; }

        // أي طالب
        public string UserId { get; set; } = "";
        public ApplicationUser? User { get; set; }

        // أي مرحلة خلّصها
        public int StageId { get; set; }
        public Stage? Stage { get; set; }

        // كم نقطة أخذ بهالمرحلة
        public int PointsEarned { get; set; }

        // تاريخ إكمال المرحلة
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }
}
