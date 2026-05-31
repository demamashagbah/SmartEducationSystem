using SmartEducation.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class QuestionBank : BaseEntity
    {
        public string QuestionText { get; set; }

        public Guid SubjectId { get; set; }

        public int Marks { get; set; }
    }
}
