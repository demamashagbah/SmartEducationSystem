using SmartEducation.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class TeacherAssignment : BaseEntity
    {
        public Guid TeacherId { get; set; }

        public TeacherProfile Teacher { get; set; }

        public Guid SubjectId { get; set; }

        public Subject Subject { get; set; }

        public Guid ClassRoomId { get; set; }

        public ClassRoom ClassRoom { get; set; }
    }
}
