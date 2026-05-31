using SmartEducation.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class AssignmentSubmission : BaseEntity
    {
        public Guid AssignmentId { get; set; }

        public Guid StudentId { get; set; }

        public string FileUrl { get; set; }

        public double? Grade { get; set; }
    }
}
