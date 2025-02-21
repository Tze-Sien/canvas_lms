﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanvasLMS.Migrations
{
    /// <inheritdoc />
    public partial class addFees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Fee",
                table: "Courses",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fee",
                table: "Courses");
        }
    }
}
