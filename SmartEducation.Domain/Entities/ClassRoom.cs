using SmartEducation.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class ClassRoom : BaseEntity
    {
        public string Name { get; set; }

        public Guid GradeId { get; set; }

        public Grade Grade { get; set; }
    }
}
