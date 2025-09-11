using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoyaltyCardsWebApi.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndex_UserId_Barcode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Cards_Barcode_UserId",
                table: "Cards",
                columns: new[] { "Barcode", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cards_Barcode_UserId",
                table: "Cards");
        }
    }
}
