using SmartEducation.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class Exam : BaseEntity
    {
        public string Title { get; set; }

        public Guid SubjectId { get; set; }

        public DateTime ExamDate { get; set; }

        public int TotalMarks { get; set; }
    }
}
