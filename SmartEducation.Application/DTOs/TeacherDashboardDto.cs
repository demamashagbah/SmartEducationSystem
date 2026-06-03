namespace SmartEducation.Application.DTOs
{
    public class TeacherDashboardDto
    {
        public string TeacherName { get; set; } = default!;
        public string EmployeeNumber { get; set; } = default!;
        public int AssignedClasses { get; set; }
        public int AssignedSubjects { get; set; }
        public int TotalStudents { get; set; }
        public int LessonPlansCount { get; set; }
        public int UpcomingExams { get; set; }
        public int PendingAssignments { get; set; }
        public double AttendanceRate { get; set; }
        public double CurriculumCoverage { get; set; }
        public double LearningOutcomeMastery { get; set; }
        public int AtRiskStudents { get; set; }
        public int UnreadMessages { get; set; }
        public List<TeacherClassSummaryDto> Classes { get; set; } = new();
        public List<TeacherSubjectCoverageDto> SubjectCoverage { get; set; } = new();
        public List<StudentSummaryDto> RecentStudentActivity { get; set; } = new();
    }

    public class TeacherClassSummaryDto
    {
        public Guid ClassRoomId { get; set; }
        public string ClassName { get; set; } = default!;
        public string GradeName { get; set; } = default!;
        public int StudentCount { get; set; }
        public List<string> Subjects { get; set; } = new();
        public double AveragePerformance { get; set; }
        public double AttendanceRate { get; set; }
    }

    public class TeacherSubjectCoverageDto
    {
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = default!;
        public int TotalLessons { get; set; }
        public int PlannedLessons { get; set; }
        public double CoveragePercent { get; set; }
    }

    public class StudentSummaryDto
    {
        public Guid StudentProfileId { get; set; }
        public string StudentName { get; set; } = default!;
        public string ClassName { get; set; } = default!;
        public double AverageScore { get; set; }
        public double AttendanceRate { get; set; }
        public string RiskLevel { get; set; } = "Low"; // Low, Medium, High
        public List<string> WeakAreas { get; set; } = new();
        public List<string> StrongAreas { get; set; } = new();
    }
}
