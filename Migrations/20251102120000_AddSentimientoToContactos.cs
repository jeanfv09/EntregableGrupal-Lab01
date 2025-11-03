using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lab01_Grupal.Migrations
{
    /// <inheritdoc />
    public partial class AddSentimientoToContactos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Sentimiento",
                table: "Contactos",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sentimiento",
                table: "Contactos");
        }
    }
}
