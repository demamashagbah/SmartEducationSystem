namespace SmartEducation.Application.DTOs
{
    public class StudentDashboardDto
    {
        public string StudentName { get; set; } = default!;
        public string ClassName { get; set; } = default!;
        public string GradeName { get; set; } = default!;
        public int TotalSubjects { get; set; }
        public int PendingAssignments { get; set; }
        public int UpcomingExams { get; set; }
        public double AttendanceRate { get; set; }
        public double GPA { get; set; }
        public int CompletedTasks { get; set; }
        public double LearningProgress { get; set; }
        public int UnreadMessages { get; set; }
        public List<StudentSubjectSummaryDto> Subjects { get; set; } = new();
        public List<string> WeakTopics { get; set; } = new();
        public List<string> StrongTopics { get; set; } = new();
        public List<AIRecommendationItemDto> AIRecommendations { get; set; } = new();
        public List<UpcomingItemDto> UpcomingItems { get; set; } = new();
    }

    public class StudentSubjectSummaryDto
    {
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = default!;
        public double AverageScore { get; set; }
        public int TotalLessons { get; set; }
        public string TeacherName { get; set; } = default!;
    }

    public class AIRecommendationItemDto
    {
        public string Category { get; set; } = default!;
        public string Topic { get; set; } = default!;
        public string Recommendation { get; set; } = default!;
        public string Priority { get; set; } = "Medium"; // Low, Medium, High
        public string Icon { get; set; } = "bx-bulb";
        public string Color { get; set; } = "#696cff";
    }

    public class UpcomingItemDto
    {
        public string Title { get; set; } = default!;
        public string Type { get; set; } = default!; // Exam, Assignment
        public DateTime DueDate { get; set; }
        public string SubjectName { get; set; } = default!;
        public string UrgencyLevel { get; set; } = "Normal"; // Urgent, Normal, Upcoming
    }
}
