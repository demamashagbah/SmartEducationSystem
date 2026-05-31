using SmartEducation.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class TeacherEvaluation : BaseEntity
    {
        public Guid TeacherId { get; set; }

        public double Score { get; set; }

        public string Notes { get; set; }
    }
}
