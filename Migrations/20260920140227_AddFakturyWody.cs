using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mieszkaniec.Migrations
{
    /// <inheritdoc />
    public partial class AddFakturyWody : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "faktury_wody",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NumerFaktury = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Rok = table.Column<int>(type: "int", nullable: false),
                    Miesiac = table.Column<int>(type: "int", nullable: false),
                    DataWystawienia = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ZuzycieFakturowane_m3 = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    WartoscNetto = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    WartoscBrutto = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    DataDodania = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_faktury_wody", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "faktury_wody");
        }
    }
}
