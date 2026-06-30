using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TraderForge.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddBotGraphEngine : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsEngineActive",
            table: "Strategies",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateTable(
            name: "BotNodes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                StrategyId = table.Column<Guid>(type: "uuid", nullable: false),
                Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Config = table.Column<string>(type: "jsonb", nullable: false),
                PositionX = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                PositionY = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BotNodes", x => x.Id);
                table.ForeignKey(
                    name: "FK_BotNodes_Strategies_StrategyId",
                    column: x => x.StrategyId,
                    principalTable: "Strategies",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "StrategyExecutions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                StrategyId = table.Column<Guid>(type: "uuid", nullable: false),
                Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                CurrentFlag = table.Column<string>(type: "jsonb", nullable: true),
                CurrentNodeId = table.Column<Guid>(type: "uuid", nullable: true),
                StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                LastActivityAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StrategyExecutions", x => x.Id);
                table.ForeignKey(
                    name: "FK_StrategyExecutions_Strategies_StrategyId",
                    column: x => x.StrategyId,
                    principalTable: "Strategies",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "BotEdges",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                StrategyId = table.Column<Guid>(type: "uuid", nullable: false),
                SourceNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                SourcePort = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "Out"),
                TargetNodeId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BotEdges", x => x.Id);
                table.ForeignKey(
                    name: "FK_BotEdges_BotNodes_SourceNodeId",
                    column: x => x.SourceNodeId,
                    principalTable: "BotNodes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_BotEdges_BotNodes_TargetNodeId",
                    column: x => x.TargetNodeId,
                    principalTable: "BotNodes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_BotEdges_Strategies_StrategyId",
                    column: x => x.StrategyId,
                    principalTable: "Strategies",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_BotEdges_SourceNodeId_SourcePort",
            table: "BotEdges",
            columns: new[] { "SourceNodeId", "SourcePort" });

        migrationBuilder.CreateIndex(
            name: "IX_BotEdges_StrategyId",
            table: "BotEdges",
            column: "StrategyId");

        migrationBuilder.CreateIndex(
            name: "IX_BotEdges_TargetNodeId",
            table: "BotEdges",
            column: "TargetNodeId");

        migrationBuilder.CreateIndex(
            name: "IX_BotNodes_StrategyId",
            table: "BotNodes",
            column: "StrategyId");

        migrationBuilder.CreateIndex(
            name: "IX_StrategyExecutions_StrategyId",
            table: "StrategyExecutions",
            column: "StrategyId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "BotEdges");

        migrationBuilder.DropTable(
            name: "StrategyExecutions");

        migrationBuilder.DropTable(
            name: "BotNodes");

        migrationBuilder.DropColumn(
            name: "IsEngineActive",
            table: "Strategies");
    }
}
