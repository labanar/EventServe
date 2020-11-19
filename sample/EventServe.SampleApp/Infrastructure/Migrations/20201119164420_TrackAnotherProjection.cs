using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventServe.SampleApp.Infrastructure.Migrations
{
    public partial class TrackAnotherProjection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnotherProjections",
                schema: "Sample",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    Price = table.Column<double>(nullable: false),
                    CurrencyCode = table.Column<string>(nullable: true),
                    Available = table.Column<bool>(nullable: false),
                    LastEventId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnotherProjections", x => x.ProductId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnotherProjections",
                schema: "Sample");
        }
    }
}
