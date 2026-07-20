namespace SkillBridge.Models
{
    // المصدر التعليمي: رابط أو فيديو تابع لمرحلة
    public class Resource
    {
        public int Id { get; set; }

        public string Title { get; set; } = "";

        public string Url { get; set; } = "";

        // النوع: "video" أو "article" أو "link"
        public string Type { get; set; } = "video";

        public string Description { get; set; } = "";

        // العلاقة: كل رابط تابع لمرحلة
        public int StageId { get; set; }
        public Stage? Stage { get; set; }
    }
}
