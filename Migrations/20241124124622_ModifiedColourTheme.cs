using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApp.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedColourTheme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Colours",
                table: "ColourThemes");

            migrationBuilder.CreateTable(
                name: "Colour",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ColourProperty = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ColourValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ColourThemeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colour", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Colour_ColourThemes_ColourThemeId",
                        column: x => x.ColourThemeId,
                        principalTable: "ColourThemes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Colour_ColourThemeId",
                table: "Colour",
                column: "ColourThemeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Colour");

            migrationBuilder.AddColumn<string>(
                name: "Colours",
                table: "ColourThemes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
