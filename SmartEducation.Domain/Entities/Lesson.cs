using SmartEducation.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class Lesson : BaseEntity
    {
        public string Name { get; set; }

        public Guid UnitId { get; set; }

        public Unit Unit { get; set; }

        public ICollection<Topic> Topics { get; set; }
    }
}
