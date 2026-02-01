using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaRenamer.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddParsedTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParsedTitle",
                table: "Files",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParsedTitle",
                table: "Files");
        }
    }
}
