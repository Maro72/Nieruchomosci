using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mieszkaniec.Migrations
{
    /// <inheritdoc />
    public partial class AddUwagiDoLicznika : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Uwagi",
                table: "liczniki",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Uwagi",
                table: "liczniki");
        }
    }
}
