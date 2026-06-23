using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Template.Migrations
{
    /// <inheritdoc />
    public partial class AddUserStudyStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GiorniDiFila",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "StudioOreDomenica",
                table: "Users",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StudioOreGiovedici",
                table: "Users",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StudioOreLunedici",
                table: "Users",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StudioOreMartedici",
                table: "Users",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StudioOreMercoledici",
                table: "Users",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StudioOreSabato",
                table: "Users",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StudioOreVenerdici",
                table: "Users",
                type: "double",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GiorniDiFila",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StudioOreDomenica",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StudioOreGiovedici",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StudioOreLunedici",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StudioOreMartedici",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StudioOreMercoledici",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StudioOreSabato",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StudioOreVenerdici",
                table: "Users");
        }
    }
}
