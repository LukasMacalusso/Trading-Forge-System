using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TraderForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPositionTableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PortfolioAssets_Portfolios_PortfolioId",
                table: "PortfolioAssets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PortfolioAssets",
                table: "PortfolioAssets");

            migrationBuilder.RenameTable(
                name: "PortfolioAssets",
                newName: "Positions");

            migrationBuilder.RenameIndex(
                name: "IX_PortfolioAssets_PortfolioId",
                table: "Positions",
                newName: "IX_Positions_PortfolioId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Positions",
                table: "Positions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_Portfolios_PortfolioId",
                table: "Positions",
                column: "PortfolioId",
                principalTable: "Portfolios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Positions_Portfolios_PortfolioId",
                table: "Positions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Positions",
                table: "Positions");

            migrationBuilder.RenameTable(
                name: "Positions",
                newName: "PortfolioAssets");

            migrationBuilder.RenameIndex(
                name: "IX_Positions_PortfolioId",
                table: "PortfolioAssets",
                newName: "IX_PortfolioAssets_PortfolioId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PortfolioAssets",
                table: "PortfolioAssets",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PortfolioAssets_Portfolios_PortfolioId",
                table: "PortfolioAssets",
                column: "PortfolioId",
                principalTable: "Portfolios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
