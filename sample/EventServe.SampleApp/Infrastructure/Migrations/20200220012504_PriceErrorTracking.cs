using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventServe.SampleApp.Infrastructure.Migrations
{
    public partial class PriceErrorTracking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PriceErrorAlertLastPrices",
                schema: "Sample",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(nullable: false),
                    Price = table.Column<double>(nullable: false),
                    CurrencyCode = table.Column<string>(nullable: true),
                    DateLastModified = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceErrorAlertLastPrices", x => x.ProductId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PriceErrorAlertLastPrices",
                schema: "Sample");
        }
    }
}
