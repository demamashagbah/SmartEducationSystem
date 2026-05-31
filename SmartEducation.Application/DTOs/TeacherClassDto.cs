using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Application.DTOs
{
    public class TeacherClassDto
    {
        public Guid TeacherSubjectId { get; set; }
        public Guid ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string GradeName { get; set; } = string.Empty;
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int WeeklyLessonsCount { get; set; }
        public Guid CurriculumId { get; set; }
    }
}
