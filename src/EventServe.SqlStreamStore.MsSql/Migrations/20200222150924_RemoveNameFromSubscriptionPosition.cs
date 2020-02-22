using Microsoft.EntityFrameworkCore.Migrations;

namespace EventServe.SqlStreamStore.MsSql.Migrations
{
    public partial class RemoveNameFromSubscriptionPosition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPositions_Name",
                table: "SubscriptionPositions");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "SubscriptionPositions");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "SubscriptionPositions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPositions_Name",
                table: "SubscriptionPositions",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");
        }
    }
}
