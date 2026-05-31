using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Application.DTOs.Dashboard
{
    public class DashboardStatisticsDto
    {
        public int TotalStudents { get; set; }

        public int TotalTeachers { get; set; }

        public int TotalSubjects { get; set; }

        public int TotalClasses { get; set; }

        public double AttendanceRate { get; set; }

        public double SuccessRate { get; set; }

        public int AIAlertsCount { get; set; }
    }
}
