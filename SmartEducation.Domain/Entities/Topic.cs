using SmartEducation.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class Topic : BaseEntity
    {
        public string Name { get; set; }

        public Guid LessonId { get; set; }

        public Lesson Lesson { get; set; }

        public ICollection<LearningOutcome> LearningOutcomes { get; set; }
    }
}
