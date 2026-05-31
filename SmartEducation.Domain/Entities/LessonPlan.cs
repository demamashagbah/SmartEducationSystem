using SmartEducation.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class LessonPlan : BaseEntity
    {
        public Guid TeacherAssignmentId { get; set; }

        public TeacherAssignment TeacherAssignment { get; set; }

        public DateTime LessonDate { get; set; }

        public string Objectives { get; set; }

        public string Activities { get; set; }
    }
}
