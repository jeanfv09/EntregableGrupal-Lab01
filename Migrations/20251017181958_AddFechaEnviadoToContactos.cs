using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lab01_Grupal.Migrations
{
    /// <inheritdoc />
    public partial class AddFechaEnviadoToContactos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEnviado",
                table: "Contactos",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Mensaje",
                table: "Contactos",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaEnviado",
                table: "Contactos");

            migrationBuilder.DropColumn(
                name: "Mensaje",
                table: "Contactos");
        }
    }
}
