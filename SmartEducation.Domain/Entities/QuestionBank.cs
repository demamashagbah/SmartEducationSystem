using SmartEducation.Domain.Common;
using SmartEducation.Domain.Enums;

namespace SmartEducation.Domain.Entities
{
    public class QuestionBank : BaseEntity
    {
        public string QuestionText { get; set; } = default!;

        public Guid SubjectId { get; set; }
        public Subject Subject { get; set; } = default!;

        public Guid? ExamId { get; set; }
        public Exam? Exam { get; set; }

        public QuestionType QuestionType { get; set; } = QuestionType.MultipleChoice;

        public DifficultyLevel DifficultyLevel { get; set; } = DifficultyLevel.Medium;

        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }

        public string? CorrectAnswer { get; set; }

        public int Marks { get; set; }
    }
}
