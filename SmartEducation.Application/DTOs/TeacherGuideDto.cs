namespace SmartEducation.Application.DTOs
{
    public class TeacherGuideDto
    {
        public Guid Id { get; set; }
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = default!;
        public string FileName { get; set; } = default!;
        public string FilePath { get; set; } = default!;
        public string? Description { get; set; }
        public Guid? AcademicYearId { get; set; }
        public string AcademicYearName { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public bool IsAnalyzed { get; set; }
        public string? AnalysisNotes { get; set; }
    }

    public class SubjectCurriculumDto
    {
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = default!;
        public string? Description { get; set; }
        public Guid ClassRoomId { get; set; }
        public string ClassRoomName { get; set; } = string.Empty;
        public Guid? AcademicYearId { get; set; }
        public string AcademicYearName { get; set; } = string.Empty;
        public List<UnitCurriculumDto> Units { get; set; } = new();
        public List<TeacherGuideDto> TeacherGuides { get; set; } = new();
        public int TotalUnits { get; set; }
        public int TotalLessons { get; set; }
        public int TotalTopics { get; set; }
        public int TotalLearningOutcomes { get; set; }
        public int TotalActivities { get; set; }
    }

    public class UnitCurriculumDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public List<LessonCurriculumDto> Lessons { get; set; } = new();
    }

    public class LessonCurriculumDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public List<TopicCurriculumDto> Topics { get; set; } = new();
    }

    public class TopicCurriculumDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public List<string> LearningOutcomes { get; set; } = new();
        public string? Activities { get; set; }
        public string? TeacherNotes { get; set; }
        public string? TeachingStrategies { get; set; }
        public string? AssessmentSuggestions { get; set; }
        public string? HomeworkSuggestions { get; set; }
    }
}
