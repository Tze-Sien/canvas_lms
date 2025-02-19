using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanvasLMS.Migrations
{
    /// <inheritdoc />
    public partial class SemesterFacultyConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Semesters_FacultyId",
                table: "Semesters",
                column: "FacultyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Semesters_Faculties_FacultyId",
                table: "Semesters",
                column: "FacultyId",
                principalTable: "Faculties",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Semesters_Faculties_FacultyId",
                table: "Semesters");

            migrationBuilder.DropIndex(
                name: "IX_Semesters_FacultyId",
                table: "Semesters");
        }
    }
}
