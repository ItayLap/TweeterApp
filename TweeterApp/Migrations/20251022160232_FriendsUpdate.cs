using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TweeterApp.Migrations
{
    /// <inheritdoc />
    public partial class FriendsUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RespondedAt",
                table: "Friends",
                newName: "UpdatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Friends",
                newName: "RespondedAt");
        }
    }
}
