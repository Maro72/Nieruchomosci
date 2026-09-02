using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mieszkaniec.Migrations
{
    /// <inheritdoc />
    public partial class PoprawaKluczaZalacznikiUmowy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ZalacznikiUmow_UmowyNajmu_UmowaNajmuId",
                table: "ZalacznikiUmow");

            migrationBuilder.DropIndex(
                name: "IX_ZalacznikiUmow_UmowaNajmuId",
                table: "ZalacznikiUmow");

            migrationBuilder.DropColumn(
                name: "UmowaNajmuId",
                table: "ZalacznikiUmow");

            migrationBuilder.CreateIndex(
                name: "IX_ZalacznikiUmow_UmowaId",
                table: "ZalacznikiUmow",
                column: "UmowaId");

            migrationBuilder.AddForeignKey(
                name: "FK_ZalacznikiUmow_UmowyNajmu_UmowaId",
                table: "ZalacznikiUmow",
                column: "UmowaId",
                principalTable: "UmowyNajmu",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ZalacznikiUmow_UmowyNajmu_UmowaId",
                table: "ZalacznikiUmow");

            migrationBuilder.DropIndex(
                name: "IX_ZalacznikiUmow_UmowaId",
                table: "ZalacznikiUmow");

            migrationBuilder.AddColumn<int>(
                name: "UmowaNajmuId",
                table: "ZalacznikiUmow",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ZalacznikiUmow_UmowaNajmuId",
                table: "ZalacznikiUmow",
                column: "UmowaNajmuId");

            migrationBuilder.AddForeignKey(
                name: "FK_ZalacznikiUmow_UmowyNajmu_UmowaNajmuId",
                table: "ZalacznikiUmow",
                column: "UmowaNajmuId",
                principalTable: "UmowyNajmu",
                principalColumn: "Id");
        }
    }
}
