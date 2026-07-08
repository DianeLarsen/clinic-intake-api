using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicIntakeApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientIdentityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PatientName",
                table: "IntakeRequests");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Patients",
                newName: "LastName");

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                table: "Patients",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Patients",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Patients");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Patients",
                newName: "FullName");

            migrationBuilder.AddColumn<string>(
                name: "PatientName",
                table: "IntakeRequests",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
