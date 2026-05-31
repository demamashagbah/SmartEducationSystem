using SmartEducation.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public bool IsRead { get; set; }
    }
}
