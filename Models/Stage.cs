namespace SkillBridge.Models
{
    // المرحلة (المستوى) داخل التخصص
    public class Stage
    {
        public int Id { get; set; }

        public string Title { get; set; } = "";

        public string Description { get; set; } = "";

        // ترتيب المرحلة داخل التخصص
        public int Order { get; set; }

        // نقاط الطالب بياخدهم لما يخلّص المرحلة (بيحسبها من الأسئلة عادة)
        public int Points { get; set; }

        // العلاقة: كل مرحلة تابعة لتخصص
        public int SpecializationId { get; set; }
        public Specialization? Specialization { get; set; }

        // العلاقة: كل مرحلة إلها روابط تعليمية
        public List<Resource> Resources { get; set; } = new List<Resource>();

        // العلاقة: كل مرحلة إلها أسئلة
        public List<Question> Questions { get; set; } = new List<Question>();
    }
}
