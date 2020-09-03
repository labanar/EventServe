using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventServe.SampleApp.Infrastructure.Migrations
{
    public partial class TrackLastEventId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LastEventId",
                schema: "Sample",
                table: "Products",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastEventId",
                schema: "Sample",
                table: "Products");
        }
    }
}
