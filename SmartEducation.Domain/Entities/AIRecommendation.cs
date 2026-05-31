using SmartEducation.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class AIRecommendation : BaseEntity
    {
        public string Recommendation { get; set; }

        public string Category { get; set; }
    }
}
