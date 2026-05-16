using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicSystem_22180011.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailToPatient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Оставяме САМО добавянето на Email в Patients
            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "22180011",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // При връщане назад (Rollback) изтриваме САМО колоната Email
            migrationBuilder.DropColumn(
                name: "Email",
                schema: "22180011",
                table: "Patients");
        }
    }
}