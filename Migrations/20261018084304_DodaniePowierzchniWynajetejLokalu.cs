using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mieszkaniec.Migrations
{
    /// <inheritdoc />
    public partial class DodaniePowierzchniWynajetejLokalu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PowierzchniaWynajetaM2",
                table: "UmowaLokal",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(
                "UPDATE `UmowaLokal` ul " +
                "INNER JOIN `LokaleWynajem` l ON l.`Id` = ul.`LokalWynajemId` " +
                "SET ul.`PowierzchniaWynajetaM2` = l.`PowierzchniaM2`;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PowierzchniaWynajetaM2",
                table: "UmowaLokal");
        }
    }
}
