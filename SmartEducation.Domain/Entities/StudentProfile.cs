using SmartEducation.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class StudentProfile : BaseEntity
    {
        public Guid UserId { get; set; }

        public ApplicationUser User { get; set; }

        public Guid ClassRoomId { get; set; }

        public ClassRoom ClassRoom { get; set; }

        public ICollection<ParentStudent> ParentStudents { get; set; } = new List<ParentStudent>();
    }
}
