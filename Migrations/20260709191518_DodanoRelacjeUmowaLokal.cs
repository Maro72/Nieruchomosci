using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mieszkaniec.Migrations
{
    /// <inheritdoc />
    public partial class DodanoRelacjeUmowaLokal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UmowaLokal",
                columns: table => new
                {
                    UmowaNajmuId = table.Column<int>(type: "int", nullable: false),
                    LokalWynajemId = table.Column<int>(type: "int", nullable: false),
                    WynegocjowanaCenaZaM2 = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CzyRyczalt = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UmowaLokal", x => new { x.UmowaNajmuId, x.LokalWynajemId });
                    table.ForeignKey(
                        name: "FK_UmowaLokal_LokaleWynajem_LokalWynajemId",
                        column: x => x.LokalWynajemId,
                        principalTable: "LokaleWynajem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UmowaLokal_UmowyNajmu_UmowaNajmuId",
                        column: x => x.UmowaNajmuId,
                        principalTable: "UmowyNajmu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_UmowaLokal_LokalWynajemId",
                table: "UmowaLokal",
                column: "LokalWynajemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UmowaLokal");
        }
    }
}
