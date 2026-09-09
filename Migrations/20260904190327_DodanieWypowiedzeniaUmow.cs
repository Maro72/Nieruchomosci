using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mieszkaniec.Migrations
{
    /// <inheritdoc />
    public partial class DodanieWypowiedzeniaUmow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataFaktycznegoZakonczenia",
                table: "UmowyNajmu",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataPlanowanegoZakonczenia",
                table: "UmowyNajmu",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataWypowiedzenia",
                table: "UmowyNajmu",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OkresWypowiedzeniaDni",
                table: "UmowyNajmu",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PowodWypowiedzenia",
                table: "UmowyNajmu",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "UmowyNajmu",
                type: "longtext",
                nullable: false,
                defaultValue: "Aktywna")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataFaktycznegoZakonczenia",
                table: "UmowyNajmu");

            migrationBuilder.DropColumn(
                name: "DataPlanowanegoZakonczenia",
                table: "UmowyNajmu");

            migrationBuilder.DropColumn(
                name: "DataWypowiedzenia",
                table: "UmowyNajmu");

            migrationBuilder.DropColumn(
                name: "OkresWypowiedzeniaDni",
                table: "UmowyNajmu");

            migrationBuilder.DropColumn(
                name: "PowodWypowiedzenia",
                table: "UmowyNajmu");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "UmowyNajmu");
        }
    }
}
