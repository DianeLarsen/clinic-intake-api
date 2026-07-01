using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicIntakeApi.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClinicId",
                table: "IntakeRequests",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Clinics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clinics", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntakeRequests_ClinicId",
                table: "IntakeRequests",
                column: "ClinicId");

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeRequests_Clinics_ClinicId",
                table: "IntakeRequests",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IntakeRequests_Clinics_ClinicId",
                table: "IntakeRequests");

            migrationBuilder.DropTable(
                name: "Clinics");

            migrationBuilder.DropIndex(
                name: "IX_IntakeRequests_ClinicId",
                table: "IntakeRequests");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                table: "IntakeRequests");
        }
    }
}
