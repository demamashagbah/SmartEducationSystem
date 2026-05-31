using SmartEducation.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class AttendanceSession : BaseEntity
    {

        public Guid ClassRoomId { get; set; }

        public DateTime SessionDate { get; set; }
    }
}
