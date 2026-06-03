using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartEducation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FullSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassRoom_Grade_GradeId",
                table: "ClassRoom");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningOutcomes_Topics_TopicId",
                table: "LearningOutcomes");

            migrationBuilder.DropForeignKey(
                name: "FK_Lesson_Units_UnitId",
                table: "Lesson");

            migrationBuilder.DropForeignKey(
                name: "FK_Semesters_AcademicYear_AcademicYearId",
                table: "Semesters");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentProfiles_ClassRoom_ClassRoomId",
                table: "StudentProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherAssignments_ClassRoom_ClassRoomId",
                table: "TeacherAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherAssignments_Subjects_SubjectId",
                table: "TeacherAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherAssignments_TeacherProfiles_TeacherId",
                table: "TeacherAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_Topics_Lesson_LessonId",
                table: "Topics");

            migrationBuilder.DropForeignKey(
                name: "FK_Units_Subjects_SubjectId",
                table: "Units");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Lesson",
                table: "Lesson");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Grade",
                table: "Grade");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClassRoom",
                table: "ClassRoom");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AcademicYear",
                table: "AcademicYear");

            migrationBuilder.RenameTable(
                name: "Lesson",
                newName: "Lessons");

            migrationBuilder.RenameTable(
                name: "Grade",
                newName: "Grades");

            migrationBuilder.RenameTable(
                name: "ClassRoom",
                newName: "ClassRooms");

            migrationBuilder.RenameTable(
                name: "AcademicYear",
                newName: "AcademicYears");

            migrationBuilder.RenameIndex(
                name: "IX_Lesson_UnitId",
                table: "Lessons",
                newName: "IX_Lessons_UnitId");

            migrationBuilder.RenameIndex(
                name: "IX_ClassRoom_GradeId",
                table: "ClassRooms",
                newName: "IX_ClassRooms_GradeId");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "TeacherEvaluations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "EvaluationDate",
                table: "TeacherEvaluations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "EvaluatorId",
                table: "TeacherEvaluations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "FinishedAt",
                table: "StudentExams",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSubmitted",
                table: "StudentExams",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "StudentExams",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrectAnswer",
                table: "QuestionBanks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DifficultyLevel",
                table: "QuestionBanks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ExamId",
                table: "QuestionBanks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OptionA",
                table: "QuestionBanks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OptionB",
                table: "QuestionBanks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OptionC",
                table: "QuestionBanks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OptionD",
                table: "QuestionBanks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuestionType",
                table: "QuestionBanks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Lessons",
                table: "Lessons",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Grades",
                table: "Grades",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClassRooms",
                table: "ClassRooms",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AcademicYears",
                table: "AcademicYears",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AIInsights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIInsights", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AIRecommendations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Recommendation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActedOn = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIRecommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIRecommendations_StudentProfiles_StudentId",
                        column: x => x.StudentId,
                        principalTable: "StudentProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Announcements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetRole = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPinned = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassRoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaxScore = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assignments_ClassRooms_ClassRoomId",
                        column: x => x.ClassRoomId,
                        principalTable: "ClassRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assignments_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assignments_TeacherProfiles_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "TeacherProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassRoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceSessions_ClassRooms_ClassRoomId",
                        column: x => x.ClassRoomId,
                        principalTable: "ClassRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttendanceSessions_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttendanceSessions_TeacherProfiles_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "TeacherProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Exams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassRoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExamType = table.Column<int>(type: "int", nullable: false),
                    ExamDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalMarks = table.Column<int>(type: "int", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exams_ClassRooms_ClassRoomId",
                        column: x => x.ClassRoomId,
                        principalTable: "ClassRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Exams_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LessonPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherAssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LessonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LessonDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Objectives = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Introduction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activities = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TeachingStrategies = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssessmentMethods = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Homework = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    IsAiGenerated = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonPlans_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LessonPlans_TeacherAssignments_TeacherAssignmentId",
                        column: x => x.TeacherAssignmentId,
                        principalTable: "TeacherAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Grade = table.Column<double>(type: "float", nullable: true),
                    Feedback = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentSubmissions_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssignmentSubmissions_StudentProfiles_StudentId",
                        column: x => x.StudentId,
                        principalTable: "StudentProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttendanceSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsPresent = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_AttendanceSessions_AttendanceSessionId",
                        column: x => x.AttendanceSessionId,
                        principalTable: "AttendanceSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_StudentProfiles_StudentId",
                        column: x => x.StudentId,
                        principalTable: "StudentProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluations_TeacherId",
                table: "TeacherEvaluations",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentExams_ExamId",
                table: "StudentExams",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentExams_StudentId",
                table: "StudentExams",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionBanks_ExamId",
                table: "QuestionBanks",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionBanks_SubjectId",
                table: "QuestionBanks",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AIRecommendations_StudentId",
                table: "AIRecommendations",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_ClassRoomId",
                table: "Assignments",
                column: "ClassRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_SubjectId",
                table: "Assignments",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_TeacherId",
                table: "Assignments",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentSubmissions_AssignmentId",
                table: "AssignmentSubmissions",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentSubmissions_StudentId",
                table: "AssignmentSubmissions",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_AttendanceSessionId",
                table: "AttendanceRecords",
                column: "AttendanceSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_StudentId",
                table: "AttendanceRecords",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceSessions_ClassRoomId",
                table: "AttendanceSessions",
                column: "ClassRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceSessions_SubjectId",
                table: "AttendanceSessions",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceSessions_TeacherId",
                table: "AttendanceSessions",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_ClassRoomId",
                table: "Exams",
                column: "ClassRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_SubjectId",
                table: "Exams",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlans_LessonId",
                table: "LessonPlans",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlans_TeacherAssignmentId",
                table: "LessonPlans",
                column: "TeacherAssignmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassRooms_Grades_GradeId",
                table: "ClassRooms",
                column: "GradeId",
                principalTable: "Grades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningOutcomes_Topics_TopicId",
                table: "LearningOutcomes",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_Units_UnitId",
                table: "Lessons",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionBanks_Exams_ExamId",
                table: "QuestionBanks",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionBanks_Subjects_SubjectId",
                table: "QuestionBanks",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Semesters_AcademicYears_AcademicYearId",
                table: "Semesters",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentExams_Exams_ExamId",
                table: "StudentExams",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentExams_StudentProfiles_StudentId",
                table: "StudentExams",
                column: "StudentId",
                principalTable: "StudentProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentProfiles_ClassRooms_ClassRoomId",
                table: "StudentProfiles",
                column: "ClassRoomId",
                principalTable: "ClassRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherAssignments_ClassRooms_ClassRoomId",
                table: "TeacherAssignments",
                column: "ClassRoomId",
                principalTable: "ClassRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherAssignments_Subjects_SubjectId",
                table: "TeacherAssignments",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherAssignments_TeacherProfiles_TeacherId",
                table: "TeacherAssignments",
                column: "TeacherId",
                principalTable: "TeacherProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherEvaluations_TeacherProfiles_TeacherId",
                table: "TeacherEvaluations",
                column: "TeacherId",
                principalTable: "TeacherProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Topics_Lessons_LessonId",
                table: "Topics",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Units_Subjects_SubjectId",
                table: "Units",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassRooms_Grades_GradeId",
                table: "ClassRooms");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningOutcomes_Topics_TopicId",
                table: "LearningOutcomes");

            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_Units_UnitId",
                table: "Lessons");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionBanks_Exams_ExamId",
                table: "QuestionBanks");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionBanks_Subjects_SubjectId",
                table: "QuestionBanks");

            migrationBuilder.DropForeignKey(
                name: "FK_Semesters_AcademicYears_AcademicYearId",
                table: "Semesters");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentExams_Exams_ExamId",
                table: "StudentExams");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentExams_StudentProfiles_StudentId",
                table: "StudentExams");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentProfiles_ClassRooms_ClassRoomId",
                table: "StudentProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherAssignments_ClassRooms_ClassRoomId",
                table: "TeacherAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherAssignments_Subjects_SubjectId",
                table: "TeacherAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherAssignments_TeacherProfiles_TeacherId",
                table: "TeacherAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherEvaluations_TeacherProfiles_TeacherId",
                table: "TeacherEvaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_Topics_Lessons_LessonId",
                table: "Topics");

            migrationBuilder.DropForeignKey(
                name: "FK_Units_Subjects_SubjectId",
                table: "Units");

            migrationBuilder.DropTable(
                name: "AIInsights");

            migrationBuilder.DropTable(
                name: "AIRecommendations");

            migrationBuilder.DropTable(
                name: "Announcements");

            migrationBuilder.DropTable(
                name: "AssignmentSubmissions");

            migrationBuilder.DropTable(
                name: "AttendanceRecords");

            migrationBuilder.DropTable(
                name: "Exams");

            migrationBuilder.DropTable(
                name: "LessonPlans");

            migrationBuilder.DropTable(
                name: "Assignments");

            migrationBuilder.DropTable(
                name: "AttendanceSessions");

            migrationBuilder.DropIndex(
                name: "IX_TeacherEvaluations_TeacherId",
                table: "TeacherEvaluations");

            migrationBuilder.DropIndex(
                name: "IX_StudentExams_ExamId",
                table: "StudentExams");

            migrationBuilder.DropIndex(
                name: "IX_StudentExams_StudentId",
                table: "StudentExams");

            migrationBuilder.DropIndex(
                name: "IX_QuestionBanks_ExamId",
                table: "QuestionBanks");

            migrationBuilder.DropIndex(
                name: "IX_QuestionBanks_SubjectId",
                table: "QuestionBanks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Lessons",
                table: "Lessons");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Grades",
                table: "Grades");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClassRooms",
                table: "ClassRooms");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AcademicYears",
                table: "AcademicYears");

            migrationBuilder.DropColumn(
                name: "EvaluationDate",
                table: "TeacherEvaluations");

            migrationBuilder.DropColumn(
                name: "EvaluatorId",
                table: "TeacherEvaluations");

            migrationBuilder.DropColumn(
                name: "FinishedAt",
                table: "StudentExams");

            migrationBuilder.DropColumn(
                name: "IsSubmitted",
                table: "StudentExams");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "StudentExams");

            migrationBuilder.DropColumn(
                name: "CorrectAnswer",
                table: "QuestionBanks");

            migrationBuilder.DropColumn(
                name: "DifficultyLevel",
                table: "QuestionBanks");

            migrationBuilder.DropColumn(
                name: "ExamId",
                table: "QuestionBanks");

            migrationBuilder.DropColumn(
                name: "OptionA",
                table: "QuestionBanks");

            migrationBuilder.DropColumn(
                name: "OptionB",
                table: "QuestionBanks");

            migrationBuilder.DropColumn(
                name: "OptionC",
                table: "QuestionBanks");

            migrationBuilder.DropColumn(
                name: "OptionD",
                table: "QuestionBanks");

            migrationBuilder.DropColumn(
                name: "QuestionType",
                table: "QuestionBanks");

            migrationBuilder.RenameTable(
                name: "Lessons",
                newName: "Lesson");

            migrationBuilder.RenameTable(
                name: "Grades",
                newName: "Grade");

            migrationBuilder.RenameTable(
                name: "ClassRooms",
                newName: "ClassRoom");

            migrationBuilder.RenameTable(
                name: "AcademicYears",
                newName: "AcademicYear");

            migrationBuilder.RenameIndex(
                name: "IX_Lessons_UnitId",
                table: "Lesson",
                newName: "IX_Lesson_UnitId");

            migrationBuilder.RenameIndex(
                name: "IX_ClassRooms_GradeId",
                table: "ClassRoom",
                newName: "IX_ClassRoom_GradeId");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "TeacherEvaluations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Lesson",
                table: "Lesson",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Grade",
                table: "Grade",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClassRoom",
                table: "ClassRoom",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AcademicYear",
                table: "AcademicYear",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassRoom_Grade_GradeId",
                table: "ClassRoom",
                column: "GradeId",
                principalTable: "Grade",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningOutcomes_Topics_TopicId",
                table: "LearningOutcomes",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Lesson_Units_UnitId",
                table: "Lesson",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Semesters_AcademicYear_AcademicYearId",
                table: "Semesters",
                column: "AcademicYearId",
                principalTable: "AcademicYear",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentProfiles_ClassRoom_ClassRoomId",
                table: "StudentProfiles",
                column: "ClassRoomId",
                principalTable: "ClassRoom",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherAssignments_ClassRoom_ClassRoomId",
                table: "TeacherAssignments",
                column: "ClassRoomId",
                principalTable: "ClassRoom",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherAssignments_Subjects_SubjectId",
                table: "TeacherAssignments",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherAssignments_TeacherProfiles_TeacherId",
                table: "TeacherAssignments",
                column: "TeacherId",
                principalTable: "TeacherProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Topics_Lesson_LessonId",
                table: "Topics",
                column: "LessonId",
                principalTable: "Lesson",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Units_Subjects_SubjectId",
                table: "Units",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
