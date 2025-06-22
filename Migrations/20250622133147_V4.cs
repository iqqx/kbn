using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kaban.Migrations
{
    /// <inheritdoc />
    public partial class V4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EventPhotoEntities",
                table: "EventPhotoEntities");

            migrationBuilder.RenameTable(
                name: "EventPhotoEntities",
                newName: "EventPhotos");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventPhotos",
                table: "EventPhotos",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EventPhotos",
                table: "EventPhotos");

            migrationBuilder.RenameTable(
                name: "EventPhotos",
                newName: "EventPhotoEntities");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventPhotoEntities",
                table: "EventPhotoEntities",
                column: "Id");
        }
    }
}
