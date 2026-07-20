namespace SkillBridge.Models
{
    // التخصص: مثل تطوير الويب، الذكاء الاصطناعي، الشبكات...
    public class Specialization
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";

        // اسم أيقونة (bootstrap icon) مثل: bi-code-slash
        public string Icon { get; set; } = "";

        public string Description { get; set; } = "";

        // العلاقة: كل تخصص إله مراحل كثيرة
        public List<Stage> Stages { get; set; } = new List<Stage>();
    }
}
