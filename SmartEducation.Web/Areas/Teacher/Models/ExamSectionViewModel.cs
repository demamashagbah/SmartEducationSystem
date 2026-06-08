namespace SmartEducation.Web.Areas.Teacher.Models
{
    public class ExamSectionViewModel
    {
        public string SectionName { get; set; } = string.Empty;
        public string QuestionType { get; set; } = "MultipleChoice";
        public int QuestionCount { get; set; } = 5;
        public int MarksEach { get; set; } = 2;
        public int SectionTotal => QuestionCount * MarksEach;
    }

    public class ExamCreateViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid SubjectId { get; set; }
        public Guid? ClassRoomId { get; set; }
        public string ExamType { get; set; } = "Quiz";
        public DateTime ExamDate { get; set; } = DateTime.Today.AddDays(7);
        public int DurationMinutes { get; set; } = 60;

        public List<ExamSectionViewModel> Sections { get; set; } = new()
        {
            new ExamSectionViewModel { SectionName = "Section A", QuestionType = "MultipleChoice", QuestionCount = 10, MarksEach = 1 },
            new ExamSectionViewModel { SectionName = "Section B", QuestionType = "Essay", QuestionCount = 2, MarksEach = 5 }
        };

        public int TotalMarks => Sections.Sum(s => s.SectionTotal);
    }
}
