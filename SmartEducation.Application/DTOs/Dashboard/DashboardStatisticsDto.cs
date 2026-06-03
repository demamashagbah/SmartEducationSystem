namespace SmartEducation.Application.DTOs.Dashboard
{
    public class DashboardStatisticsDto
    {
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalParents { get; set; }
        public int TotalSubjects { get; set; }
        public int TotalClassRooms { get; set; }
        public int TotalGrades { get; set; }
        public int TotalExams { get; set; }
        public int TotalAssignments { get; set; }
        public int TotalLessonPlans { get; set; }
        public int TotalAnnouncements { get; set; }
        public double AttendanceRate { get; set; }
        public double SuccessRate { get; set; }
        public int AIAlertsCount { get; set; }
    }
}
