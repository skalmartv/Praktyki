using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pokoje",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nazwa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SzacowanaWielkosc = table.Column<int>(type: "int", nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pokoje", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pokoje_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PokojZdjecia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NazwaPliku = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SciezkaPliku = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataDodania = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PokojId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PokojZdjecia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PokojZdjecia_Pokoje_PokojId",
                        column: x => x.PokojId,
                        principalTable: "Pokoje",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PunktyLokacji",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    DataPomiaru = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PokojId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PunktyLokacji", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PunktyLokacji_Pokoje_PokojId",
                        column: x => x.PokojId,
                        principalTable: "Pokoje",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pokoje_UserId",
                table: "Pokoje",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PokojZdjecia_PokojId",
                table: "PokojZdjecia",
                column: "PokojId");

            migrationBuilder.CreateIndex(
                name: "IX_PunktyLokacji_PokojId",
                table: "PunktyLokacji",
                column: "PokojId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PokojZdjecia");

            migrationBuilder.DropTable(
                name: "PunktyLokacji");

            migrationBuilder.DropTable(
                name: "Pokoje");
        }
    }
}
