namespace SmartEducation.Application.DTOs
{
    public class SemesterDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public Guid AcademicYearId { get; set; }
        public string AcademicYearName { get; set; } = default!;
    }
}
