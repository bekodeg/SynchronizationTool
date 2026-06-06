using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SynchronizationTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class SynchronisationTool_v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "entity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Change",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ColumnName = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true),
                    ChangeLogId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Change", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChangeLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    EntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RowId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClientId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClientVersion = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChangeLog_entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SynchClient",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: false),
                    LastChangeLogId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SynchClient", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SynchClient_ChangeLog_LastChangeLogId",
                        column: x => x.LastChangeLogId,
                        principalTable: "ChangeLog",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Change_ChangeLogId",
                table: "Change",
                column: "ChangeLogId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeLog_ClientId",
                table: "ChangeLog",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeLog_EntityId",
                table: "ChangeLog",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_entity_Code",
                table: "entity",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SynchClient_LastChangeLogId",
                table: "SynchClient",
                column: "LastChangeLogId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Change_ChangeLog_ChangeLogId",
                table: "Change",
                column: "ChangeLogId",
                principalTable: "ChangeLog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeLog_SynchClient_ClientId",
                table: "ChangeLog",
                column: "ClientId",
                principalTable: "SynchClient",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SynchClient_ChangeLog_LastChangeLogId",
                table: "SynchClient");

            migrationBuilder.DropTable(
                name: "Change");

            migrationBuilder.DropTable(
                name: "ChangeLog");

            migrationBuilder.DropTable(
                name: "SynchClient");

            migrationBuilder.DropTable(
                name: "entity");
        }
    }
}
