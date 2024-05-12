using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlockShop.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBlockNumberOfBuys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumberOfBuys",
                table: "Blocks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberOfBuys",
                table: "Blocks");
        }
    }
}
