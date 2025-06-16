using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Testes.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationFieldsToDonation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_DocumentNumber",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "DocumentNumber",
                table: "Users",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsReserved",
                table: "Donations",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReservedAt",
                table: "Donations",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReservedByUserId",
                table: "Donations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Donations_ReservedByUserId",
                table: "Donations",
                column: "ReservedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Donations_Users_ReservedByUserId",
                table: "Donations",
                column: "ReservedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Donations_Users_ReservedByUserId",
                table: "Donations");

            migrationBuilder.DropIndex(
                name: "IX_Donations_ReservedByUserId",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "IsReserved",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "ReservedAt",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "ReservedByUserId",
                table: "Donations");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "DocumentNumber",
                table: "Users",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DocumentNumber",
                table: "Users",
                column: "DocumentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }
    }
}
