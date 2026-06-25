using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TraderForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Feature1_2_Subscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SubscriptionPlanId",
                table: "Traders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Portfolios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VirtualBalance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TraderId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Portfolios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Portfolios_Traders_TraderId",
                        column: x => x.TraderId,
                        principalTable: "Traders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MonthlyPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    InitialVirtualBalance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MaxActiveStrategies = table.Column<int>(type: "integer", nullable: true),
                    MaxActiveAssets = table.Column<int>(type: "integer", nullable: true),
                    CanModifyVirtualBalance = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SubscriptionPlans",
                columns: new[] { "Id", "CanModifyVirtualBalance", "InitialVirtualBalance", "MaxActiveAssets", "MaxActiveStrategies", "MonthlyPrice", "Name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), false, 10000m, 5, 2, 9.99m, "Basic" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), false, 50000m, 20, 10, 29.99m, "Pro" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), true, 100000m, null, null, 99.99m, "Enterprise" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Traders_SubscriptionPlanId",
                table: "Traders",
                column: "SubscriptionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Portfolios_TraderId",
                table: "Portfolios",
                column: "TraderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Traders_SubscriptionPlans_SubscriptionPlanId",
                table: "Traders",
                column: "SubscriptionPlanId",
                principalTable: "SubscriptionPlans",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Traders_SubscriptionPlans_SubscriptionPlanId",
                table: "Traders");

            migrationBuilder.DropTable(
                name: "Portfolios");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");

            migrationBuilder.DropIndex(
                name: "IX_Traders_SubscriptionPlanId",
                table: "Traders");

            migrationBuilder.DropColumn(
                name: "SubscriptionPlanId",
                table: "Traders");
        }
    }
}
