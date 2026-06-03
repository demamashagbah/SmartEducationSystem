using SmartEducation.Domain.Common;
using SmartEducation.Domain.Enums;

namespace SmartEducation.Domain.Entities
{
    public class Exam : BaseEntity
    {
        public string Title { get; set; } = default!;

        public string? Description { get; set; }

        public Guid SubjectId { get; set; }
        public Subject Subject { get; set; } = default!;

        public Guid? ClassRoomId { get; set; }
        public ClassRoom? ClassRoom { get; set; }

        public ExamType ExamType { get; set; } = ExamType.Quiz;

        public DateTime ExamDate { get; set; }

        public int TotalMarks { get; set; }

        public int DurationMinutes { get; set; } = 60;

        public ICollection<StudentExam> StudentExams { get; set; } = new List<StudentExam>();
        public ICollection<QuestionBank> Questions { get; set; } = new List<QuestionBank>();
    }
}
