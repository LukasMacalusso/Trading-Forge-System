using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TraderForge.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddOrderEntity : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Orders",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                PortfolioId = table.Column<Guid>(type: "uuid", nullable: false),
                Symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                Side = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                Quantity = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                Price = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                Commission = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                Total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                FilledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Orders", x => x.Id);
                table.ForeignKey(
                    name: "FK_Orders_Portfolios_PortfolioId",
                    column: x => x.PortfolioId,
                    principalTable: "Portfolios",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Orders_PortfolioId",
            table: "Orders",
            column: "PortfolioId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Orders");
    }
}
