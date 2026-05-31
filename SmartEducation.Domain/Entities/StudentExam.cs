using SmartEducation.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class StudentExam : BaseEntity
    {
        public Guid StudentId { get; set; }

        public Guid ExamId { get; set; }

        public double Score { get; set; }
    }
}
