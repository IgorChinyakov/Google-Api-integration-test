using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmoMeter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class emometerfixforeignkeyname : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_events_users_UserId",
                table: "events");

            migrationBuilder.AddForeignKey(
                name: "user_id",
                table: "events",
                column: "UserId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "user_id",
                table: "events");

            migrationBuilder.AddForeignKey(
                name: "FK_events_users_UserId",
                table: "events",
                column: "UserId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
