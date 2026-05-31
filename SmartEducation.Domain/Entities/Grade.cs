using SmartEducation.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class Grade : BaseEntity
    {
        public string Name { get; set; }

        public ICollection<ClassRoom> ClassRooms { get; set; }
    }
}
