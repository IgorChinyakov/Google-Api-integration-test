using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmoMeter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class emometerfixuserid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "user_id",
                table: "events");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "events",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "IX_events_UserId",
                table: "events",
                newName: "IX_events_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_events_users_user_id",
                table: "events",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_events_users_user_id",
                table: "events");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "events",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_events_user_id",
                table: "events",
                newName: "IX_events_UserId");

            migrationBuilder.AddForeignKey(
                name: "user_id",
                table: "events",
                column: "UserId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
