using SmartEducation.Domain.Common;

namespace SmartEducation.Domain.Entities
{
    public class CurriculumPlan : BaseEntity
    {
        public Guid TeacherAssignmentId { get; set; }
        public TeacherAssignment TeacherAssignment { get; set; } = default!;

        public string Title { get; set; } = default!;

        public string PlanType { get; set; } = "Weekly"; // Weekly, Monthly, Semester

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsAiGenerated { get; set; } = true;

        public ICollection<CurriculumPlanItem> Items { get; set; } = new List<CurriculumPlanItem>();
    }
}
