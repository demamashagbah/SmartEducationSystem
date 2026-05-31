using SmartEducation.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class TeacherProfile : BaseEntity
    {
        public Guid UserId { get; set; }

        public ApplicationUser User { get; set; }

        public string EmployeeNumber { get; set; }
    }
}
