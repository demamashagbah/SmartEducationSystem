using SmartEducation.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class StudentPerformance : BaseEntity
    {
        public Guid StudentId { get; set; }

        public Guid SubjectId { get; set; }

        public double AverageScore { get; set; }

        public double AttendanceRate { get; set; }
    }
}
