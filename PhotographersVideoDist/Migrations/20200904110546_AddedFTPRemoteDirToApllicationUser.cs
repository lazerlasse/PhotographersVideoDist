using Microsoft.EntityFrameworkCore.Migrations;

namespace PhotographersVideoDist.Migrations
{
    public partial class AddedFTPRemoteDirToApllicationUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FTP_RemoteDir",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FTP_RemoteDir",
                table: "AspNetUsers");
        }
    }
}
