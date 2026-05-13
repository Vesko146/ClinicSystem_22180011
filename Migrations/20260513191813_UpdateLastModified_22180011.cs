using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicSystem_22180011.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLastModified_22180011 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified_22180011",
                schema: "22180011",
                table: "Patients",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModified_22180011",
                schema: "22180011",
                table: "Doctors",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "(getdate())");

           

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModified_22180011",
                schema: "22180011",
                table: "Appointments",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "(getdate())");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastModified_22180011",
                schema: "22180011",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "ScheduleGroup",
                schema: "22180011",
                table: "Doctors");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModified_22180011",
                schema: "22180011",
                table: "Doctors",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModified_22180011",
                schema: "22180011",
                table: "Appointments",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");
        }
    }
}
