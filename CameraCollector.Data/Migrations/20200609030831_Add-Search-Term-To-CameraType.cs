using Microsoft.EntityFrameworkCore.Migrations;

namespace CameraCollector.Data.Migrations
{
    public partial class AddSearchTermToCameraType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SearchTerm",
                table: "CameraTypes",
                maxLength: 128,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SearchTerm",
                table: "CameraTypes");
        }
    }
}
