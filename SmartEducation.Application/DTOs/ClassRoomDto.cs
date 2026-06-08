namespace SmartEducation.Application.DTOs
{
    public class ClassRoomDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public Guid GradeId { get; set; }
        public string GradeName { get; set; } = default!;
        public int StudentCount { get; set; }
        public int SubjectCount { get; set; }
        public int TeacherAssignmentCount { get; set; }
    }
}
