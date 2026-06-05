using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SynchronizationTool.Demo.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "device_type",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("device_type_id", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "home",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("home_id", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    password = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_id", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "state_type",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    change_command = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    device_type_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("state_type_id", x => x.id);
                    table.ForeignKey(
                        name: "state_type_device_type_id_fkey",
                        column: x => x.device_type_id,
                        principalTable: "device_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "device",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    mqtt_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    device_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    home_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("device_id", x => x.id);
                    table.ForeignKey(
                        name: "device_device_type_id_fkey",
                        column: x => x.device_type_id,
                        principalTable: "device_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "device_home_id_fkey",
                        column: x => x.home_id,
                        principalTable: "home",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_home",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    home_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_home_pk", x => new { x.user_id, x.home_id });
                    table.ForeignKey(
                        name: "user_home_home_id_fkey",
                        column: x => x.home_id,
                        principalTable: "home",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "user_home_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "device_state",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    state_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("device_state_id", x => x.id);
                    table.ForeignKey(
                        name: "device_state_device_id_fkey",
                        column: x => x.device_id,
                        principalTable: "device",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "device_state_state_type_id_fkey",
                        column: x => x.state_type_id,
                        principalTable: "state_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_device_device_type_id",
                table: "device",
                column: "device_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_device_home_id",
                table: "device",
                column: "home_id");

            migrationBuilder.CreateIndex(
                name: "IX_device_state_device_id",
                table: "device_state",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_device_state_state_type_id",
                table: "device_state",
                column: "state_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_state_type_device_type_id",
                table: "state_type",
                column: "device_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_home_home_id",
                table: "user_home",
                column: "home_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "device_state");

            migrationBuilder.DropTable(
                name: "user_home");

            migrationBuilder.DropTable(
                name: "device");

            migrationBuilder.DropTable(
                name: "state_type");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "home");

            migrationBuilder.DropTable(
                name: "device_type");
        }
    }
}
