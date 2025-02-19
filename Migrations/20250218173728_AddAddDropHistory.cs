using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanvasLMS.Migrations
{
    /// <inheritdoc />
    public partial class AddAddDropHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Approved",
                table: "CourseEnrollments");

            migrationBuilder.CreateTable(
                name: "AddDropHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SemesterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActionedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActionedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddDropHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AddDropHistories_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AddDropHistories_Lecturers_ActionedById",
                        column: x => x.ActionedById,
                        principalTable: "Lecturers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AddDropHistories_Semesters_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semesters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AddDropHistories_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddDropHistories_ActionedById",
                table: "AddDropHistories",
                column: "ActionedById");

            migrationBuilder.CreateIndex(
                name: "IX_AddDropHistories_CourseId",
                table: "AddDropHistories",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_AddDropHistories_SemesterId_StudentId",
                table: "AddDropHistories",
                columns: new[] { "SemesterId", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_AddDropHistories_StudentId",
                table: "AddDropHistories",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddDropHistories");

            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "CourseEnrollments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
