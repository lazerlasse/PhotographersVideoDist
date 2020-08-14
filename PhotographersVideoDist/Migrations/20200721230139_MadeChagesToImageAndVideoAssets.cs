using Microsoft.EntityFrameworkCore.Migrations;

namespace PhotographersVideoDist.Migrations
{
    public partial class MadeChagesToImageAndVideoAssets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoAssetsFilePath",
                table: "VideoAssets");

            migrationBuilder.DropColumn(
                name: "ImageFilePath",
                table: "ImageAssets");

            migrationBuilder.AlterColumn<string>(
                name: "VideoAssetsFileName",
                table: "VideoAssets",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImageFileName",
                table: "ImageAssets",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "VideoAssetsFileName",
                table: "VideoAssets",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<string>(
                name: "VideoAssetsFilePath",
                table: "VideoAssets",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImageFileName",
                table: "ImageAssets",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<string>(
                name: "ImageFilePath",
                table: "ImageAssets",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);
        }
    }
}
