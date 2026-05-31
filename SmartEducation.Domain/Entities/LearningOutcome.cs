using SmartEducation.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class LearningOutcome : BaseEntity
    {
        public string Description { get; set; }

        public Guid TopicId { get; set; }

        public Topic Topic { get; set; }
    }
}