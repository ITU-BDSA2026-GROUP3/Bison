using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bison.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddObservationImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ImgBytes",
                table: "Post",
                type: "BLOB",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImgBytes",
                table: "Post");
        }
    }
}
