using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class ParentStudent
    {
        public Guid ParentId { get; set; }

        public ParentProfile Parent { get; set; }

        public Guid StudentId { get; set; }

        public StudentProfile Student { get; set; }
    }
}
