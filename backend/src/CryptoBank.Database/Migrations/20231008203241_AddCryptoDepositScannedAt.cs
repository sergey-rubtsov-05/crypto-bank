using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoBank.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCryptoDepositScannedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ConfirmedAt",
                table: "crypto_deposits",
                newName: "confirmed_at");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "scanned_at",
                table: "crypto_deposits",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "scanned_at",
                table: "crypto_deposits");

            migrationBuilder.RenameColumn(
                name: "confirmed_at",
                table: "crypto_deposits",
                newName: "ConfirmedAt");
        }
    }
}
