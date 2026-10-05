using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mieszkaniec.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Najemcy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NazwaFirmyOsoby = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nip = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    REGON = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Adres = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefon = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OsobaKontaktowa = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CzyArchiwalny = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataArchiwizacji = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Uwagi = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Najemcy", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "obiekty",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nazwa = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumerEwidencyjny = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Adres = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RokBudowy = table.Column<int>(type: "int", nullable: true),
                    LiczbaKondygnacji = table.Column<int>(type: "int", nullable: true),
                    Wysokosc = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    Kubatura = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    PowUzytkowa = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Opis = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Wyposazenie = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CzyArchiwum = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataUtworzenia = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_obiekty", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PriorytetyUsterek",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nazwa = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Poziom = table.Column<int>(type: "int", nullable: false),
                    KodKoloru = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MaksCzasReakcjiGodziny = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriorytetyUsterek", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RodzajeUsterek",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nazwa = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    KlasaIkony = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CzyWymagaUprawnien = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RodzajeUsterek", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nazwa = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "terminy_definicje",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NazwaTypu = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CzestoscMiesiace = table.Column<int>(type: "int", nullable: false),
                    DniPowiadomienia = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_terminy_definicje", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Uprawnienia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NazwaSystemowa = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Opis = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Uprawnienia", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Uzytkownicy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Login = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HasloHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Imie = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nazwisko = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CzyAktywny = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Uzytkownicy", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
                name: "LokaleWynajem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ObiektId = table.Column<int>(type: "int", nullable: false),
                    NumerLokalu = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TypLokalu = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PowierzchniaM2 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CenaZaM2 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SvgElementId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NajemcaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LokaleWynajem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LokaleWynajem_Najemcy_NajemcaId",
                        column: x => x.NajemcaId,
                        principalTable: "Najemcy",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LokaleWynajem_obiekty_ObiektId",
                        column: x => x.ObiektId,
                        principalTable: "obiekty",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PraceRemontowe",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ObiektId = table.Column<int>(type: "int", nullable: false),
                    UsterkaId = table.Column<int>(type: "int", nullable: true),
                    DataZgloszeniaUsterki = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    OsobaZglaszajaca = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nazwa = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Opis = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RodzajUsterkiId = table.Column<int>(type: "int", nullable: false),
                    PriorytetUsterkiId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataRozpoczeciaPlanowana = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataZakonczeniaPlanowana = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataRozpoczeciaFaktyczna = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DataZakonczeniaFaktyczna = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    KosztSzacowany = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KosztFaktyczny = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WykonawcaNazwa = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RobociznaStawkaGodzinowa = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LiczbaPracownikow = table.Column<int>(type: "int", nullable: false),
                    SzacowanaLiczbaDni = table.Column<int>(type: "int", nullable: false),
                    GodzinyDziennie = table.Column<int>(type: "int", nullable: false),
                    KosztCalkowityRobocizny = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PraceRemontowe", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PraceRemontowe_PriorytetyUsterek_PriorytetUsterkiId",
                        column: x => x.PriorytetUsterkiId,
                        principalTable: "PriorytetyUsterek",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PraceRemontowe_RodzajeUsterek_RodzajUsterkiId",
                        column: x => x.RodzajUsterkiId,
                        principalTable: "RodzajeUsterek",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PraceRemontowe_obiekty_ObiektId",
                        column: x => x.ObiektId,
                        principalTable: "obiekty",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UsterkiBud",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ObiektId = table.Column<int>(type: "int", nullable: false),
                    OsobaZglaszajaca = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataZgloszenia = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    OpisZgłoszenia = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RodzajUsterkiId = table.Column<int>(type: "int", nullable: false),
                    PriorytetUsterkiId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataZakonczeniaNaprawy = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UwagiKonserwatora = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CzyArchiwum = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsterkiBud", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsterkiBud_PriorytetyUsterek_PriorytetUsterkiId",
                        column: x => x.PriorytetUsterkiId,
                        principalTable: "PriorytetyUsterek",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsterkiBud_RodzajeUsterek_RodzajUsterkiId",
                        column: x => x.RodzajUsterkiId,
                        principalTable: "RodzajeUsterek",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsterkiBud_obiekty_ObiektId",
                        column: x => x.ObiektId,
                        principalTable: "obiekty",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "przeglady",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ObiektId = table.Column<int>(type: "int", nullable: false),
                    TerminDefinicjaId = table.Column<int>(type: "int", nullable: false),
                    DataWykonania = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataNastepnego = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    OsobaWykonujaca = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WynikOcena = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_przeglady", x => x.Id);
                    table.ForeignKey(
                        name: "FK_przeglady_obiekty_ObiektId",
                        column: x => x.ObiektId,
                        principalTable: "obiekty",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_przeglady_terminy_definicje_TerminDefinicjaId",
                        column: x => x.TerminDefinicjaId,
                        principalTable: "terminy_definicje",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UzytkownikRola",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    UzytkownicyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UzytkownikRola", x => new { x.RoleId, x.UzytkownicyId });
                    table.ForeignKey(
                        name: "FK_UzytkownikRola_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UzytkownikRola_Uzytkownicy_UzytkownicyId",
                        column: x => x.UzytkownicyId,
                        principalTable: "Uzytkownicy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UzytkownikUprawnienie",
                columns: table => new
                {
                    UprawnieniaId = table.Column<int>(type: "int", nullable: false),
                    UzytkownicyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UzytkownikUprawnienie", x => new { x.UprawnieniaId, x.UzytkownicyId });
                    table.ForeignKey(
                        name: "FK_UzytkownikUprawnienie_Uprawnienia_UprawnieniaId",
                        column: x => x.UprawnieniaId,
                        principalTable: "Uprawnienia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UzytkownikUprawnienie_Uzytkownicy_UzytkownicyId",
                        column: x => x.UzytkownicyId,
                        principalTable: "Uzytkownicy",
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
                    NowaStawkaCzynszu = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
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
                    DataDodania = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZalacznikiUmow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZalacznikiUmow_UmowyNajmu_UmowaId",
                        column: x => x.UmowaId,
                        principalTable: "UmowyNajmu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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

            migrationBuilder.CreateTable(
                name: "KosztorysMaterial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PraceRemontoweId = table.Column<int>(type: "int", nullable: false),
                    NazwaMaterialu = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Jm = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ilosc = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CenaJednostkowa = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WartoscCalkowita = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KosztorysMaterial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KosztorysMaterial_PraceRemontowe_PraceRemontoweId",
                        column: x => x.PraceRemontoweId,
                        principalTable: "PraceRemontowe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "zalaczniki",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PrzegladId = table.Column<int>(type: "int", nullable: true),
                    UsterkiBudId = table.Column<int>(type: "int", nullable: true),
                    NazwaPliku = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SciezkaMagazyn = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RozmiarKB = table.Column<int>(type: "int", nullable: false),
                    DataDodania = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zalaczniki", x => x.Id);
                    table.ForeignKey(
                        name: "FK_zalaczniki_UsterkiBud_UsterkiBudId",
                        column: x => x.UsterkiBudId,
                        principalTable: "UsterkiBud",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_zalaczniki_przeglady_PrzegladId",
                        column: x => x.PrzegladId,
                        principalTable: "przeglady",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AneksyUmow_UmowaNajmuId",
                table: "AneksyUmow",
                column: "UmowaNajmuId");

            migrationBuilder.CreateIndex(
                name: "IX_KosztorysMaterial_PraceRemontoweId",
                table: "KosztorysMaterial",
                column: "PraceRemontoweId");

            migrationBuilder.CreateIndex(
                name: "IX_LokaleWynajem_NajemcaId",
                table: "LokaleWynajem",
                column: "NajemcaId");

            migrationBuilder.CreateIndex(
                name: "IX_LokaleWynajem_ObiektId",
                table: "LokaleWynajem",
                column: "ObiektId");

            migrationBuilder.CreateIndex(
                name: "IX_PraceRemontowe_ObiektId",
                table: "PraceRemontowe",
                column: "ObiektId");

            migrationBuilder.CreateIndex(
                name: "IX_PraceRemontowe_PriorytetUsterkiId",
                table: "PraceRemontowe",
                column: "PriorytetUsterkiId");

            migrationBuilder.CreateIndex(
                name: "IX_PraceRemontowe_RodzajUsterkiId",
                table: "PraceRemontowe",
                column: "RodzajUsterkiId");

            migrationBuilder.CreateIndex(
                name: "IX_przeglady_ObiektId",
                table: "przeglady",
                column: "ObiektId");

            migrationBuilder.CreateIndex(
                name: "IX_przeglady_TerminDefinicjaId",
                table: "przeglady",
                column: "TerminDefinicjaId");

            migrationBuilder.CreateIndex(
                name: "IX_UmowaLokal_LokalWynajemId",
                table: "UmowaLokal",
                column: "LokalWynajemId");

            migrationBuilder.CreateIndex(
                name: "IX_UmowyNajmu_NajemcaId",
                table: "UmowyNajmu",
                column: "NajemcaId");

            migrationBuilder.CreateIndex(
                name: "IX_UsterkiBud_ObiektId",
                table: "UsterkiBud",
                column: "ObiektId");

            migrationBuilder.CreateIndex(
                name: "IX_UsterkiBud_PriorytetUsterkiId",
                table: "UsterkiBud",
                column: "PriorytetUsterkiId");

            migrationBuilder.CreateIndex(
                name: "IX_UsterkiBud_RodzajUsterkiId",
                table: "UsterkiBud",
                column: "RodzajUsterkiId");

            migrationBuilder.CreateIndex(
                name: "IX_UzytkownikRola_UzytkownicyId",
                table: "UzytkownikRola",
                column: "UzytkownicyId");

            migrationBuilder.CreateIndex(
                name: "IX_UzytkownikUprawnienie_UzytkownicyId",
                table: "UzytkownikUprawnienie",
                column: "UzytkownicyId");

            migrationBuilder.CreateIndex(
                name: "IX_zalaczniki_PrzegladId",
                table: "zalaczniki",
                column: "PrzegladId");

            migrationBuilder.CreateIndex(
                name: "IX_zalaczniki_UsterkiBudId",
                table: "zalaczniki",
                column: "UsterkiBudId");

            migrationBuilder.CreateIndex(
                name: "IX_ZalacznikiUmow_UmowaId",
                table: "ZalacznikiUmow",
                column: "UmowaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AneksyUmow");

            migrationBuilder.DropTable(
                name: "KosztorysMaterial");

            migrationBuilder.DropTable(
                name: "UmowaLokal");

            migrationBuilder.DropTable(
                name: "UzytkownikRola");

            migrationBuilder.DropTable(
                name: "UzytkownikUprawnienie");

            migrationBuilder.DropTable(
                name: "zalaczniki");

            migrationBuilder.DropTable(
                name: "ZalacznikiUmow");

            migrationBuilder.DropTable(
                name: "PraceRemontowe");

            migrationBuilder.DropTable(
                name: "LokaleWynajem");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "Uprawnienia");

            migrationBuilder.DropTable(
                name: "Uzytkownicy");

            migrationBuilder.DropTable(
                name: "UsterkiBud");

            migrationBuilder.DropTable(
                name: "przeglady");

            migrationBuilder.DropTable(
                name: "UmowyNajmu");

            migrationBuilder.DropTable(
                name: "PriorytetyUsterek");

            migrationBuilder.DropTable(
                name: "RodzajeUsterek");

            migrationBuilder.DropTable(
                name: "obiekty");

            migrationBuilder.DropTable(
                name: "terminy_definicje");

            migrationBuilder.DropTable(
                name: "Najemcy");
        }
    }
}
