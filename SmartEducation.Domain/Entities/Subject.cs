using SmartEducation.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class Subject : BaseEntity
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<Unit> Units { get; set; }
    }
}
