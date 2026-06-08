using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartEducation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UIRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_ClassRooms_ClassRoomId",
                table: "Subjects");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClassRoomId",
                table: "Subjects",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_ClassRooms_ClassRoomId",
                table: "Subjects",
                column: "ClassRoomId",
                principalTable: "ClassRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_ClassRooms_ClassRoomId",
                table: "Subjects");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClassRoomId",
                table: "Subjects",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_ClassRooms_ClassRoomId",
                table: "Subjects",
                column: "ClassRoomId",
                principalTable: "ClassRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
