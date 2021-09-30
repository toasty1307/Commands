using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommandsTest.Migrations
{
    public partial class Fix2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupEntity",
                table: "GroupEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CommandEntity",
                table: "CommandEntity");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "GroupEntity",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "GroupEntity",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CommandEntity",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "CommandEntity",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupEntity",
                table: "GroupEntity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CommandEntity",
                table: "CommandEntity",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupEntity",
                table: "GroupEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CommandEntity",
                table: "CommandEntity");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "GroupEntity");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "CommandEntity");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "GroupEntity",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CommandEntity",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupEntity",
                table: "GroupEntity",
                column: "Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CommandEntity",
                table: "CommandEntity",
                column: "Name");
        }
    }
}
