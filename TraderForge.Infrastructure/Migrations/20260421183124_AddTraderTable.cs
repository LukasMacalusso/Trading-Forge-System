using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TraderForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTraderTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Traders",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    FreeTrialExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FreeTrialRegistrationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Traders", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Traders");
        }
    }
}
