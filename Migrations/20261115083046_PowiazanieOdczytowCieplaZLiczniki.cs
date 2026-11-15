using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mieszkaniec.Migrations
{
    /// <inheritdoc />
    public partial class PowiazanieOdczytowCieplaZLiczniki : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LicznikId",
                table: "odczyty_ciepla",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                INSERT INTO liczniki (NumerLicznika, Typ, ObiektId, DataDodania, CzyAktywny)
                SELECT odczyt.NumerLicznika, 'Ciepło', odczyt.ObiektId, CURRENT_TIMESTAMP(6), 1
                FROM (
                    SELECT
                        NumerLicznika,
                        ObiektId,
                        ROW_NUMBER() OVER (
                            PARTITION BY UPPER(TRIM(NumerLicznika))
                            ORDER BY Rok DESC, Miesiac DESC, DataOdczytu DESC, Id DESC
                        ) AS NumerWiersza
                    FROM odczyty_ciepla
                ) AS odczyt
                WHERE odczyt.NumerWiersza = 1
                    AND NOT EXISTS (
                        SELECT 1
                        FROM liczniki
                        WHERE UPPER(TRIM(NumerLicznika)) = UPPER(TRIM(odczyt.NumerLicznika))
                            AND (
                                LOWER(Typ) LIKE '%ciep%'
                                OR LOWER(Typ) LIKE '%ogrz%'
                                OR LOWER(Typ) LIKE '%heat%'
                                OR UPPER(TRIM(Typ)) = 'CO'
                            )
                    );
                """);

            migrationBuilder.Sql(
                """
                UPDATE odczyty_ciepla AS odczyt
                JOIN (
                    SELECT UPPER(TRIM(NumerLicznika)) AS NumerNormalizowany, MIN(Id) AS LicznikId
                    FROM liczniki
                    WHERE LOWER(Typ) LIKE '%ciep%'
                        OR LOWER(Typ) LIKE '%ogrz%'
                        OR LOWER(Typ) LIKE '%heat%'
                        OR UPPER(TRIM(Typ)) = 'CO'
                    GROUP BY UPPER(TRIM(NumerLicznika))
                    HAVING COUNT(*) = 1
                ) AS licznik
                    ON licznik.NumerNormalizowany = UPPER(TRIM(odczyt.NumerLicznika))
                SET odczyt.LicznikId = licznik.LicznikId
                WHERE odczyt.LicznikId IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_odczyty_ciepla_LicznikId",
                table: "odczyty_ciepla",
                column: "LicznikId");

            migrationBuilder.AddForeignKey(
                name: "FK_odczyty_ciepla_liczniki_LicznikId",
                table: "odczyty_ciepla",
                column: "LicznikId",
                principalTable: "liczniki",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_odczyty_ciepla_liczniki_LicznikId",
                table: "odczyty_ciepla");

            migrationBuilder.DropIndex(
                name: "IX_odczyty_ciepla_LicznikId",
                table: "odczyty_ciepla");

            migrationBuilder.DropColumn(
                name: "LicznikId",
                table: "odczyty_ciepla");
        }
    }
}
