using SmartEducation.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class AttendanceRecord : BaseEntity
    {
        public Guid AttendanceSessionId { get; set; }

        public Guid StudentId { get; set; }

        public bool IsPresent { get; set; }
    }
}
