namespace SmartEducation.Application.DTOs
{
    public class AcademicYearDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
