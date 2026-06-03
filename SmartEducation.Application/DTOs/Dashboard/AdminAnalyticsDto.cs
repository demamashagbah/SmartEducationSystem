namespace SmartEducation.Application.DTOs.Dashboard
{
    public class AdminAnalyticsDto
    {
        public DashboardStatisticsDto KPIs { get; set; } = new();
        public List<TeacherAnalyticsDto> TeacherAnalytics { get; set; } = new();
        public List<ClassAnalyticsDto> ClassAnalytics { get; set; } = new();
        public List<SubjectAnalyticsDto> SubjectAnalytics { get; set; } = new();
        public List<LearningOutcomeAnalyticsDto> LearningOutcomeAnalytics { get; set; } = new();
        public List<AtRiskStudentDto> AtRiskStudents { get; set; } = new();
        public double OverallAttendanceRate { get; set; }
        public double OverallSuccessRate { get; set; }
        public double CurriculumCoverage { get; set; }
    }

    public class TeacherAnalyticsDto
    {
        public Guid TeacherId { get; set; }
        public string TeacherName { get; set; } = default!;
        public string EmployeeNumber { get; set; } = default!;
        public int AssignedSubjects { get; set; }
        public int AssignedClasses { get; set; }
        public int TotalStudents { get; set; }
        public double CurriculumCoverage { get; set; }
        public double LessonPlanCompletionRate { get; set; }
        public double AttendanceRecordingRate { get; set; }
        public double AssignmentUsageRate { get; set; }
        public double AssessmentUsageRate { get; set; }
        public double AverageStudentPerformance { get; set; }
        public double StudentSuccessRate { get; set; }
        public double StudentFailureRate { get; set; }
        public List<string> SubjectNames { get; set; } = new();
        public List<string> ClassNames { get; set; } = new();
    }

    public class ClassAnalyticsDto
    {
        public Guid ClassRoomId { get; set; }
        public string ClassName { get; set; } = default!;
        public string GradeName { get; set; } = default!;
        public int TotalStudents { get; set; }
        public double AverageGrade { get; set; }
        public double AttendanceRate { get; set; }
        public double AssignmentCompletionRate { get; set; }
        public double ExamSuccessRate { get; set; }
        public double LearningOutcomeMastery { get; set; }
        public List<string> SubjectNames { get; set; } = new();
    }

    public class SubjectAnalyticsDto
    {
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = default!;
        public int TotalUnits { get; set; }
        public int TotalLessons { get; set; }
        public int TotalTopics { get; set; }
        public int TotalLearningOutcomes { get; set; }
        public double AveragePerformance { get; set; }
        public double LearningOutcomeCoverage { get; set; }
        public double MasteryPercentage { get; set; }
        public int WeakOutcomes { get; set; }
        public int StrongOutcomes { get; set; }
    }

    public class LearningOutcomeAnalyticsDto
    {
        public Guid OutcomeId { get; set; }
        public string OutcomeDescription { get; set; } = default!;
        public string TopicName { get; set; } = default!;
        public string SubjectName { get; set; } = default!;
        public int StudentsTotal { get; set; }
        public int StudentsMastered { get; set; }
        public int StudentsNotMastered { get; set; }
        public double MasteryPercentage { get; set; }
        public string MasteryLevel { get; set; } = default!; // "Low" | "Medium" | "High"
    }

    public class AtRiskStudentDto
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = default!;
        public string ClassName { get; set; } = default!;
        public double RiskScore { get; set; }
        public List<string> RiskFactors { get; set; } = new();
        public string SuggestedIntervention { get; set; } = default!;
        public double AttendanceRate { get; set; }
        public double AverageScore { get; set; }
    }
}
