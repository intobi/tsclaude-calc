using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calculator.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBookUpdatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Books",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // Books that already exist get the migration time instead of 0001-01-01.
            migrationBuilder.Sql("UPDATE \"Books\" SET \"UpdatedAtUtc\" = strftime('%Y-%m-%d %H:%M:%f', 'now');");

            migrationBuilder.CreateIndex(
                name: "IX_Books_UpdatedAtUtc",
                table: "Books",
                column: "UpdatedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Books_UpdatedAtUtc",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Books");
        }
    }
}
