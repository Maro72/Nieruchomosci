using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mieszkaniec.Migrations
{
    /// <inheritdoc />
    public partial class DodanieUmowIAneksowOrazZalacznikow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UmowyNajmu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NumerUmowy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NajemcaId = table.Column<int>(type: "int", nullable: false),
                    DataOd = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataDo = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CzyAktywna = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UmowyNajmu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UmowyNajmu_Najemcy_NajemcaId",
                        column: x => x.NajemcaId,
                        principalTable: "Najemcy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AneksyUmow",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UmowaNajmuId = table.Column<int>(type: "int", nullable: false),
                    NumerAneksu = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataZawarcia = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    NowaDataDo = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    NowaStawkaCzynszu = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    OpisZmian = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataDodania = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AneksyUmow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AneksyUmow_UmowyNajmu_UmowaNajmuId",
                        column: x => x.UmowaNajmuId,
                        principalTable: "UmowyNajmu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ZalacznikiUmow",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UmowaId = table.Column<int>(type: "int", nullable: false),
                    NazwaPliku = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SciezkaPliku = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataDodania = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UmowaNajmuId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZalacznikiUmow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZalacznikiUmow_UmowyNajmu_UmowaNajmuId",
                        column: x => x.UmowaNajmuId,
                        principalTable: "UmowyNajmu",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AneksyUmow_UmowaNajmuId",
                table: "AneksyUmow",
                column: "UmowaNajmuId");

            migrationBuilder.CreateIndex(
                name: "IX_UmowyNajmu_NajemcaId",
                table: "UmowyNajmu",
                column: "NajemcaId");

            migrationBuilder.CreateIndex(
                name: "IX_ZalacznikiUmow_UmowaNajmuId",
                table: "ZalacznikiUmow",
                column: "UmowaNajmuId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AneksyUmow");

            migrationBuilder.DropTable(
                name: "ZalacznikiUmow");

            migrationBuilder.DropTable(
                name: "UmowyNajmu");
        }
    }
}
