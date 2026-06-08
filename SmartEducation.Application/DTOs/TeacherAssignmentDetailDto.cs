namespace SmartEducation.Application.DTOs
{
    public class TeacherAssignmentDetailDto
    {
        public Guid Id { get; set; }
        public Guid TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public string TeacherEmail { get; set; } = string.Empty;
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public Guid ClassRoomId { get; set; }
        public string ClassRoomName { get; set; } = string.Empty;
        public Guid? AcademicYearId { get; set; }
        public string AcademicYearName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
