using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartEducation.Persistence.Migrations
{
    public partial class AddTopicCurriculumFieldsAndGuideYear : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Topic curriculum fields
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Topics') AND name = 'Activities')
    ALTER TABLE [Topics] ADD [Activities] nvarchar(max) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Topics') AND name = 'AssessmentSuggestions')
    ALTER TABLE [Topics] ADD [AssessmentSuggestions] nvarchar(max) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Topics') AND name = 'HomeworkSuggestions')
    ALTER TABLE [Topics] ADD [HomeworkSuggestions] nvarchar(max) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Topics') AND name = 'TeacherNotes')
    ALTER TABLE [Topics] ADD [TeacherNotes] nvarchar(max) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Topics') AND name = 'TeachingStrategies')
    ALTER TABLE [Topics] ADD [TeachingStrategies] nvarchar(max) NULL;
");

            // TeacherGuide AcademicYearId
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeacherGuides') AND name = 'AcademicYearId')
BEGIN
    ALTER TABLE [TeacherGuides] ADD [AcademicYearId] uniqueidentifier NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('TeacherGuides') AND name = 'IX_TeacherGuides_AcademicYearId')
        CREATE INDEX [IX_TeacherGuides_AcademicYearId] ON [TeacherGuides] ([AcademicYearId]);
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('TeacherGuides') AND name = 'FK_TeacherGuides_AcademicYears_AcademicYearId')
        ALTER TABLE [TeacherGuides] ADD CONSTRAINT [FK_TeacherGuides_AcademicYears_AcademicYearId]
            FOREIGN KEY ([AcademicYearId]) REFERENCES [AcademicYears] ([Id]);
END
");

            // TeacherProfile extra fields (from earlier pending migration)
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeacherProfiles') AND name = 'Qualification')
    ALTER TABLE [TeacherProfiles] ADD [Qualification] nvarchar(max) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeacherProfiles') AND name = 'Specialization')
    ALTER TABLE [TeacherProfiles] ADD [Specialization] nvarchar(max) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeacherProfiles') AND name = 'YearsOfExperience')
    ALTER TABLE [TeacherProfiles] ADD [YearsOfExperience] int NULL;
");

            // ParentProfile extra fields
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ParentProfiles') AND name = 'EmergencyContact')
    ALTER TABLE [ParentProfiles] ADD [EmergencyContact] nvarchar(max) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ParentProfiles') AND name = 'Occupation')
    ALTER TABLE [ParentProfiles] ADD [Occupation] nvarchar(max) NULL;
");

            // AspNetUsers extra fields
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'DateOfBirth')
    ALTER TABLE [AspNetUsers] ADD [DateOfBirth] datetime2 NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'Department')
    ALTER TABLE [AspNetUsers] ADD [Department] nvarchar(max) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'Gender')
    ALTER TABLE [AspNetUsers] ADD [Gender] nvarchar(max) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'Position')
    ALTER TABLE [AspNetUsers] ADD [Position] nvarchar(max) NULL;
");

            // StudentProfiles ClassRoomId nullable (safe — only if column constraint allows)
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('StudentProfiles') AND name = 'ClassRoomId' AND is_nullable = 0)
BEGIN
    -- Drop existing FK first
    DECLARE @fkName nvarchar(255);
    SELECT @fkName = fk.name FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
        INNER JOIN sys.columns c ON fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
        WHERE fk.parent_object_id = OBJECT_ID('StudentProfiles') AND c.name = 'ClassRoomId';
    IF @fkName IS NOT NULL
        EXEC('ALTER TABLE [StudentProfiles] DROP CONSTRAINT [' + @fkName + ']');
    ALTER TABLE [StudentProfiles] ALTER COLUMN [ClassRoomId] uniqueidentifier NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('StudentProfiles') AND name = 'FK_StudentProfiles_ClassRooms_ClassRoomId')
        ALTER TABLE [StudentProfiles] ADD CONSTRAINT [FK_StudentProfiles_ClassRooms_ClassRoomId]
            FOREIGN KEY ([ClassRoomId]) REFERENCES [ClassRooms]([Id]) ON DELETE SET NULL;
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Topics') AND name = 'Activities')
    ALTER TABLE [Topics] DROP COLUMN [Activities];
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Topics') AND name = 'AssessmentSuggestions')
    ALTER TABLE [Topics] DROP COLUMN [AssessmentSuggestions];
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Topics') AND name = 'HomeworkSuggestions')
    ALTER TABLE [Topics] DROP COLUMN [HomeworkSuggestions];
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Topics') AND name = 'TeacherNotes')
    ALTER TABLE [Topics] DROP COLUMN [TeacherNotes];
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Topics') AND name = 'TeachingStrategies')
    ALTER TABLE [Topics] DROP COLUMN [TeachingStrategies];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('TeacherGuides') AND name = 'FK_TeacherGuides_AcademicYears_AcademicYearId')
    ALTER TABLE [TeacherGuides] DROP CONSTRAINT [FK_TeacherGuides_AcademicYears_AcademicYearId];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('TeacherGuides') AND name = 'IX_TeacherGuides_AcademicYearId')
    DROP INDEX [IX_TeacherGuides_AcademicYearId] ON [TeacherGuides];
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeacherGuides') AND name = 'AcademicYearId')
    ALTER TABLE [TeacherGuides] DROP COLUMN [AcademicYearId];
");
        }
    }
}
