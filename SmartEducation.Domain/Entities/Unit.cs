using SmartEducation.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class Unit : BaseEntity
    {
        public string Name { get; set; }

        public Guid SubjectId { get; set; }

        public Subject Subject { get; set; }

        public ICollection<Lesson> Lessons { get; set; }
    }
}
