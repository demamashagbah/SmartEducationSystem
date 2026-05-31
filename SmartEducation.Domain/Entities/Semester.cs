using SmartEducation.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class Semester : BaseEntity
    {
        public string Name { get; set; }

        public Guid AcademicYearId { get; set; }

        public AcademicYear AcademicYear { get; set; }
    }
}
