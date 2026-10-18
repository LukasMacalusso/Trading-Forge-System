using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TraderForge.Infrastructure.Migrations;

/// <inheritdoc />
public partial class MarketAssetRefactor : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CurrentPrice",
            table: "MarketAssets");

        migrationBuilder.DropColumn(
            name: "LastUpdated",
            table: "MarketAssets");

        migrationBuilder.AddColumn<bool>(
            name: "IsActive",
            table: "MarketAssets",
            type: "boolean",
            nullable: false,
            defaultValue: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsActive",
            table: "MarketAssets");

        migrationBuilder.AddColumn<decimal>(
            name: "CurrentPrice",
            table: "MarketAssets",
            type: "numeric",
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AddColumn<DateTime>(
            name: "LastUpdated",
            table: "MarketAssets",
            type: "timestamp with time zone",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
    }
}
