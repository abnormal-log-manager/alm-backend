using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Migrations
{
    /// <inheritdoc />
    public partial class update_1a_250610 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropIndex(
            //     name: "IX_ShortUrls_OriginalUrl",
            //     table: "ShortUrls");

            migrationBuilder.AlterColumn<string>(
                name: "ShortenedUrl",
                table: "ShortUrls",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShortUrls_ShortenedUrl",
                table: "ShortUrls",
                column: "ShortenedUrl",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShortUrls_ShortenedUrl",
                table: "ShortUrls");

            migrationBuilder.AlterColumn<string>(
                name: "ShortenedUrl",
                table: "ShortUrls",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            // migrationBuilder.CreateIndex(
            //     name: "IX_ShortUrls_OriginalUrl",
            //     table: "ShortUrls",
            //     column: "OriginalUrl",
            //     unique: true);
        }
    }
}
