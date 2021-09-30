using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommandsTest.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Blacklist",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blacklist", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Blacklist_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommandEntity",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    BlacklistEntityId = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommandEntity", x => x.Name);
                    table.ForeignKey(
                        name: "FK_CommandEntity_Blacklist_BlacklistEntityId",
                        column: x => x.BlacklistEntityId,
                        principalTable: "Blacklist",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GroupEntity",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    BlacklistEntityId = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupEntity", x => x.Name);
                    table.ForeignKey(
                        name: "FK_GroupEntity_Blacklist_BlacklistEntityId",
                        column: x => x.BlacklistEntityId,
                        principalTable: "Blacklist",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Blacklist_GuildId",
                table: "Blacklist",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_CommandEntity_BlacklistEntityId",
                table: "CommandEntity",
                column: "BlacklistEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupEntity_BlacklistEntityId",
                table: "GroupEntity",
                column: "BlacklistEntityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommandEntity");

            migrationBuilder.DropTable(
                name: "GroupEntity");

            migrationBuilder.DropTable(
                name: "Blacklist");

            migrationBuilder.DropTable(
                name: "Guilds");
        }
    }
}
