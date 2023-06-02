using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace crypto_bank.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSaltField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "salt",
                table: "users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "salt",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
