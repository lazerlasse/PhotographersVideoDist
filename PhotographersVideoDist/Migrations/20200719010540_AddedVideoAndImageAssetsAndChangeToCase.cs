using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PhotographersVideoDist.Migrations
{
    public partial class AddedVideoAndImageAssetsAndChangeToCase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cases_Postals_PostalCode",
                table: "Cases");

            migrationBuilder.AlterColumn<string>(
                name: "Street",
                table: "Cases",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "PostalCode",
                table: "Cases",
                maxLength: 4,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(4) CHARACTER SET utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Details",
                table: "Cases",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "Cases",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ImageAssets",
                columns: table => new
                {
                    ImageAssetsID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ImageFileName = table.Column<string>(nullable: true),
                    ImageFilePath = table.Column<string>(nullable: true),
                    ImageIsPrimary = table.Column<bool>(nullable: false),
                    CaseID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageAssets", x => x.ImageAssetsID);
                    table.ForeignKey(
                        name: "FK_ImageAssets_Cases_CaseID",
                        column: x => x.CaseID,
                        principalTable: "Cases",
                        principalColumn: "CaseID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoAssets",
                columns: table => new
                {
                    VideoAssetsID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VideoAssetsFileName = table.Column<string>(nullable: true),
                    VideoAssetsFilePath = table.Column<string>(nullable: true),
                    CaseID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoAssets", x => x.VideoAssetsID);
                    table.ForeignKey(
                        name: "FK_VideoAssets_Cases_CaseID",
                        column: x => x.CaseID,
                        principalTable: "Cases",
                        principalColumn: "CaseID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImageAssets_CaseID",
                table: "ImageAssets",
                column: "CaseID");

            migrationBuilder.CreateIndex(
                name: "IX_VideoAssets_CaseID",
                table: "VideoAssets",
                column: "CaseID");

            migrationBuilder.AddForeignKey(
                name: "FK_Cases_Postals_PostalCode",
                table: "Cases",
                column: "PostalCode",
                principalTable: "Postals",
                principalColumn: "PostalCode",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cases_Postals_PostalCode",
                table: "Cases");

            migrationBuilder.DropTable(
                name: "ImageAssets");

            migrationBuilder.DropTable(
                name: "VideoAssets");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "Cases");

            migrationBuilder.AlterColumn<string>(
                name: "Street",
                table: "Cases",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PostalCode",
                table: "Cases",
                type: "varchar(4) CHARACTER SET utf8mb4",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 4,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Details",
                table: "Cases",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 200);

            migrationBuilder.AddForeignKey(
                name: "FK_Cases_Postals_PostalCode",
                table: "Cases",
                column: "PostalCode",
                principalTable: "Postals",
                principalColumn: "PostalCode",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
