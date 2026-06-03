using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartEducation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCurriculumPlanAndTeacherGuide : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CurriculumPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherAssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlanType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsAiGenerated = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumPlans_TeacherAssignments_TeacherAssignmentId",
                        column: x => x.TeacherAssignmentId,
                        principalTable: "TeacherAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeacherGuides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAnalyzed = table.Column<bool>(type: "bit", nullable: false),
                    AnalysisNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherGuides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherGuides_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CurriculumPlanItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurriculumPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LessonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TopicId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WeekNumber = table.Column<int>(type: "int", nullable: false),
                    PlannedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LearningOutcomes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TeachingStrategy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Homework = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuizDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExamDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumPlanItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumPlanItems_CurriculumPlans_CurriculumPlanId",
                        column: x => x.CurriculumPlanId,
                        principalTable: "CurriculumPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CurriculumPlanItems_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CurriculumPlanItems_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumPlanItems_CurriculumPlanId",
                table: "CurriculumPlanItems",
                column: "CurriculumPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumPlanItems_LessonId",
                table: "CurriculumPlanItems",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumPlanItems_TopicId",
                table: "CurriculumPlanItems",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumPlans_TeacherAssignmentId",
                table: "CurriculumPlans",
                column: "TeacherAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherGuides_SubjectId",
                table: "TeacherGuides",
                column: "SubjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurriculumPlanItems");

            migrationBuilder.DropTable(
                name: "TeacherGuides");

            migrationBuilder.DropTable(
                name: "CurriculumPlans");
        }
    }
}
