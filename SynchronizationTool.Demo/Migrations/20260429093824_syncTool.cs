using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SynchronizationTool.Demo.Migrations
{
    /// <inheritdoc />
    public partial class syncTool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sync");

            migrationBuilder.CreateTable(
                name: "entity",
                schema: "sync",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChangeLog",
                schema: "sync",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientVersion = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChangeLog_entity_EntityId",
                        column: x => x.EntityId,
                        principalSchema: "sync",
                        principalTable: "entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Change",
                schema: "sync",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ColumnName = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    ChangeLogId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Change", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Change_ChangeLog_ChangeLogId",
                        column: x => x.ChangeLogId,
                        principalSchema: "sync",
                        principalTable: "ChangeLog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Change_ChangeLogId",
                schema: "sync",
                table: "Change",
                column: "ChangeLogId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeLog_EntityId",
                schema: "sync",
                table: "ChangeLog",
                column: "EntityId");


            migrationBuilder.CreateIndex(
                name: "IX_entity_Code",
                schema: "sync",
                table: "entity",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Change",
                schema: "sync");

            migrationBuilder.DropTable(
                name: "ChangeLog",
                schema: "sync");

            migrationBuilder.DropTable(
                name: "entity",
                schema: "sync");
        }
    }
}
