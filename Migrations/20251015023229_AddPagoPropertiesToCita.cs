using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lab01_Grupal.Migrations
{
    /// <inheritdoc />
    public partial class AddPagoPropertiesToCita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "estado_pago",
                table: "Cita",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_pago",
                table: "Cita",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "metodo_pago",
                table: "Cita",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "precio",
                table: "Cita",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "transaction_id",
                table: "Cita",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "estado_pago",
                table: "Cita");

            migrationBuilder.DropColumn(
                name: "fecha_pago",
                table: "Cita");

            migrationBuilder.DropColumn(
                name: "metodo_pago",
                table: "Cita");

            migrationBuilder.DropColumn(
                name: "precio",
                table: "Cita");

            migrationBuilder.DropColumn(
                name: "transaction_id",
                table: "Cita");
        }
    }
}
