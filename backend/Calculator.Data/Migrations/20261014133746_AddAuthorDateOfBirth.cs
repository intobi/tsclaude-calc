using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calculator.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthorDateOfBirth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The date of birth is required, so authors that already exist get a placeholder
            // (1 January 1900) that can be corrected on the Authors page.
            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                table: "Authors",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateOnly(1900, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Authors");
        }
    }
}
