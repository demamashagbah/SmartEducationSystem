namespace SmartEducation.Application.DTOs
{
    public class ConversationDto
    {
        public Guid OtherUserId { get; set; }
        public string OtherUserName { get; set; } = default!;
        public string OtherUserRole { get; set; } = default!;
        public string OtherUserInitials { get; set; } = default!;
        public string LastMessage { get; set; } = default!;
        public DateTime LastMessageAt { get; set; }
        public bool LastMessageIsFromMe { get; set; }
        public int UnreadCount { get; set; }
        public Guid? LinkedStudentProfileId { get; set; }
        public string LinkedStudentName { get; set; } = string.Empty;
        public string LinkedStudentNumber { get; set; } = string.Empty;
        public string LinkedStudentClass { get; set; } = string.Empty;
        public string AvatarColor { get; set; } = "#696cff";
    }

    public class ConversationMessageDto
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; } = default!;
        public string SenderInitials { get; set; } = default!;
        public string Body { get; set; } = default!;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public bool IsFromMe { get; set; }
        public string FormattedTime { get; set; } = default!;
    }

    public class StudentContextDto
    {
        public Guid ProfileId { get; set; }
        public string FullName { get; set; } = default!;
        public string StudentNumber { get; set; } = default!;
        public string ClassName { get; set; } = default!;
        public double AttendanceRate { get; set; }
        public int AbsenceCount { get; set; }
        public double LatestExamGrade { get; set; }
        public int LatestExamTotal { get; set; }
        public string LatestExamTitle { get; set; } = string.Empty;
        public double AverageGrade { get; set; }
        public double AssignmentCompletionRate { get; set; }
        public int PendingAssignments { get; set; }
        public string PerformanceLevel { get; set; } = "Unknown";
        public string PerformanceColor { get; set; } = "#6c757d";
        public string PerformanceEmoji { get; set; } = "⚪";
        public List<string> Alerts { get; set; } = new();
    }

    public class ChatContactDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = default!;
        public string Role { get; set; } = default!;
        public string Initials { get; set; } = default!;
        public Guid? StudentProfileId { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
    }
}
