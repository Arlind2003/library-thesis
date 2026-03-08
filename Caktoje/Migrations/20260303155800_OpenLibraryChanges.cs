using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Caktoje.Migrations
{
    /// <inheritdoc />
    public partial class OpenLibraryChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Searchables_Files_FileId",
                table: "Searchables");

            migrationBuilder.AlterColumn<long>(
                name: "FileId",
                table: "Searchables",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Authors",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Searchables_Files_FileId",
                table: "Searchables",
                column: "FileId",
                principalTable: "Files",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Searchables_Files_FileId",
                table: "Searchables");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Authors");

            migrationBuilder.AlterColumn<long>(
                name: "FileId",
                table: "Searchables",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Searchables_Files_FileId",
                table: "Searchables",
                column: "FileId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
