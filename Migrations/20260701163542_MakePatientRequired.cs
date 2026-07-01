using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicIntakeApi.Migrations
{
    /// <inheritdoc />
    public partial class MakePatientRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IntakeRequests_Patients_PatientId",
                table: "IntakeRequests");

            migrationBuilder.AlterColumn<int>(
                name: "PatientId",
                table: "IntakeRequests",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeRequests_Patients_PatientId",
                table: "IntakeRequests",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IntakeRequests_Patients_PatientId",
                table: "IntakeRequests");

            migrationBuilder.AlterColumn<int>(
                name: "PatientId",
                table: "IntakeRequests",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeRequests_Patients_PatientId",
                table: "IntakeRequests",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id");
        }
    }
}
