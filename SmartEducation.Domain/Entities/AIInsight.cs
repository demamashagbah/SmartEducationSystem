using SmartEducation.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class AIInsight : BaseEntity
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string Severity { get; set; }
    }
}
