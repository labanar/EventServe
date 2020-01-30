using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventServe.SqlStreamStore.MsSql.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubscriptionPositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Position = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPositions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPositions_Name",
                table: "SubscriptionPositions",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriptionPositions");
        }
    }
}
