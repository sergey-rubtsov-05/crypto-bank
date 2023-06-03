using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace crypto_bank.Database.Migrations
{
    /// <inheritdoc />
    public partial class AccountNumberAsStringAndGenerateByCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "temp_number",
                table: "accounts",
                type: "text",
                nullable: true);

            migrationBuilder.Sql(@"
update accounts
set temp_number = t.temp_number
from (
	select 'ACC' || trim(to_char(""number"", '000000000')) as temp_number, ""number""
	from accounts
) t
where accounts.""number"" = t.""number""");

            migrationBuilder.AlterColumn<string>(
                    name: "number",
                    table: "accounts")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "number",
                table: "accounts",
                type: "text",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.Sql(@"
update accounts
set number = temp_number");

            migrationBuilder.DropColumn(
                name: "temp_number",
                table: "accounts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "number",
                table: "accounts",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
