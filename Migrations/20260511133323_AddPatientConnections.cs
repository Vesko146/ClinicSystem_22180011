using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicSystem_22180011.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientConnections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastModified_22180011",
                schema: "22180011",
                table: "Patients");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                schema: "22180011",
                table: "Patients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                schema: "22180011",
                table: "Patients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChosenDoctorId",
                schema: "22180011",
                table: "Patients",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_ChosenDoctorId",
                schema: "22180011",
                table: "Patients",
                column: "ChosenDoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_Doctors_ChosenDoctorId",
                schema: "22180011",
                table: "Patients",
                column: "ChosenDoctorId",
                principalSchema: "22180011",
                principalTable: "Doctors",
                principalColumn: "DoctorID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Patients_Doctors_ChosenDoctorId",
                schema: "22180011",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Patients_ChosenDoctorId",
                schema: "22180011",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "ChosenDoctorId",
                schema: "22180011",
                table: "Patients");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                schema: "22180011",
                table: "Patients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                schema: "22180011",
                table: "Patients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified_22180011",
                schema: "22180011",
                table: "Patients",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
