using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lab01_Grupal.Migrations
{
    /// <inheritdoc />
    public partial class AddCodigoCitaToCita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    usuario = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    clave = table.Column<string>(type: "TEXT", nullable: false),
                    nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    correo = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    rol = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.id_usuario);
                });

            migrationBuilder.CreateTable(
                name: "Medico",
                columns: table => new
                {
                    id_medico = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    id_usuario = table.Column<int>(type: "INTEGER", nullable: false),
                    especialidad = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medico", x => x.id_medico);
                    table.ForeignKey(
                        name: "FK_Medico_Usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "Usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cita",
                columns: table => new
                {
                    id_cita = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    id_paciente = table.Column<int>(type: "INTEGER", nullable: false),
                    especialidad = table.Column<string>(type: "TEXT", nullable: false),
                    id_medico = table.Column<int>(type: "INTEGER", nullable: true),
                    asignado_por = table.Column<int>(type: "INTEGER", nullable: true),
                    fecha_hora = table.Column<DateTime>(type: "TEXT", nullable: false),
                    estado = table.Column<string>(type: "TEXT", nullable: false),
                    motivo_consulta = table.Column<string>(type: "TEXT", nullable: true),
                    prioridad = table.Column<string>(type: "TEXT", nullable: true),
                    observaciones = table.Column<string>(type: "TEXT", nullable: true),
                    codigo_cita = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cita", x => x.id_cita);
                    table.ForeignKey(
                        name: "FK_Cita_Medico_id_medico",
                        column: x => x.id_medico,
                        principalTable: "Medico",
                        principalColumn: "id_medico");
                    table.ForeignKey(
                        name: "FK_Cita_Usuario_asignado_por",
                        column: x => x.asignado_por,
                        principalTable: "Usuario",
                        principalColumn: "id_usuario");
                    table.ForeignKey(
                        name: "FK_Cita_Usuario_id_paciente",
                        column: x => x.id_paciente,
                        principalTable: "Usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Perfil_Medico",
                columns: table => new
                {
                    id_perfil = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    id_medico = table.Column<int>(type: "INTEGER", nullable: false),
                    universidad = table.Column<string>(type: "TEXT", nullable: false),
                    pais_formacion = table.Column<string>(type: "TEXT", nullable: false),
                    egreso = table.Column<string>(type: "TEXT", nullable: false),
                    experiencia = table.Column<string>(type: "TEXT", nullable: false),
                    idiomas = table.Column<string>(type: "TEXT", nullable: false),
                    tipo_contrato = table.Column<string>(type: "TEXT", nullable: false),
                    turno_preferido = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perfil_Medico", x => x.id_perfil);
                    table.ForeignKey(
                        name: "FK_Perfil_Medico_Medico_id_medico",
                        column: x => x.id_medico,
                        principalTable: "Medico",
                        principalColumn: "id_medico",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Telefono_Medico",
                columns: table => new
                {
                    id_telefono = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    id_medico = table.Column<int>(type: "INTEGER", nullable: false),
                    telefono = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Telefono_Medico", x => x.id_telefono);
                    table.ForeignKey(
                        name: "FK_TelefonoMedico_Medico",
                        column: x => x.id_medico,
                        principalTable: "Medico",
                        principalColumn: "id_medico",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cita_asignado_por",
                table: "Cita",
                column: "asignado_por");

            migrationBuilder.CreateIndex(
                name: "IX_Cita_id_medico",
                table: "Cita",
                column: "id_medico");

            migrationBuilder.CreateIndex(
                name: "IX_Cita_id_paciente",
                table: "Cita",
                column: "id_paciente");

            migrationBuilder.CreateIndex(
                name: "IX_Medico_id_usuario",
                table: "Medico",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_Perfil_Medico_id_medico",
                table: "Perfil_Medico",
                column: "id_medico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Telefono_Medico_id_medico",
                table: "Telefono_Medico",
                column: "id_medico");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cita");

            migrationBuilder.DropTable(
                name: "Perfil_Medico");

            migrationBuilder.DropTable(
                name: "Telefono_Medico");

            migrationBuilder.DropTable(
                name: "Medico");

            migrationBuilder.DropTable(
                name: "Usuario");
        }
    }
}
