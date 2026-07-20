namespace SkillBridge.Models
{
    // سؤال اختيار من متعدد تابع لمرحلة
    public class Question
    {
        public int Id { get; set; }

        public string Text { get; set; } = "";

        // الخيارات الأربعة
        public string OptionA { get; set; } = "";
        public string OptionB { get; set; } = "";
        public string OptionC { get; set; } = "";
        public string OptionD { get; set; } = "";

        // الإجابة الصحيحة: "A" أو "B" أو "C" أو "D"
        public string CorrectOption { get; set; } = "A";

        // نقاط السؤال
        public int Points { get; set; } = 10;

        // العلاقة: كل سؤال تابع لمرحلة
        public int StageId { get; set; }
        public Stage? Stage { get; set; }
    }
}
