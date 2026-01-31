using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaRenamer.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddProposalStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "IsRejected",
                table: "Proposals");

            migrationBuilder.RenameColumn(
                name: "RequiresApproval",
                table: "Proposals",
                newName: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Proposals",
                newName: "RequiresApproval");

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Proposals",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRejected",
                table: "Proposals",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
