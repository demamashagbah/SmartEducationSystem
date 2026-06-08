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

        public string? Activities { get; set; }
        public string? TeacherNotes { get; set; }
        public string? TeachingStrategies { get; set; }
        public string? AssessmentSuggestions { get; set; }
        public string? HomeworkSuggestions { get; set; }
    }
}
