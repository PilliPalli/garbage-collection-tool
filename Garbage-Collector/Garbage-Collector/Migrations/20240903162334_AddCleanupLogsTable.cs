using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Garbage_Collector.Migrations
{
    /// <inheritdoc />
    public partial class AddCleanupLogsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Users_UserId",
                table: "UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Users__1788CC4CE14C9D38",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "UQ__Users__536C85E4755F0D07",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Roles",
                table: "Roles");

            migrationBuilder.RenameIndex(
                name: "IX_Roles_RoleName",
                table: "Roles",
                newName: "UQ__Roles__8A2B6160FAC7BD11");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "UserRoles",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "RoleId",
                table: "UserRoles",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Users__1788CC4C1FCD3355",
                table: "Users",
                column: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK__UserRole__3D978A3517636FDC",
                table: "UserRoles",
                column: "UserRoleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Roles__8AFACE1AB4E0B91A",
                table: "Roles",
                column: "RoleId");

            migrationBuilder.CreateTable(
                name: "CleanupLogs",
                columns: table => new
                {
                    CleanupLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    CleanupDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    FilesDeleted = table.Column<int>(type: "int", nullable: false),
                    SpaceFreedInMB = table.Column<double>(type: "float", nullable: false),
                    CleanupType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Standard")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CleanupL__33B67D89C437F914", x => x.CleanupLogId);
                    table.ForeignKey(
                        name: "FK__CleanupLo__UserI__7BE56230",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CleanupLogs_UserId",
                table: "CleanupLogs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK__UserRoles__RoleI__7908F585",
                table: "UserRoles",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK__UserRoles__UserI__7814D14C",
                table: "UserRoles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__UserRoles__RoleI__7908F585",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK__UserRoles__UserI__7814D14C",
                table: "UserRoles");

            migrationBuilder.DropTable(
                name: "CleanupLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Users__1788CC4C1FCD3355",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK__UserRole__3D978A3517636FDC",
                table: "UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Roles__8AFACE1AB4E0B91A",
                table: "Roles");

            migrationBuilder.RenameIndex(
                name: "UQ__Roles__8A2B6160FAC7BD11",
                table: "Roles",
                newName: "IX_Roles_RoleName");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "UserRoles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RoleId",
                table: "UserRoles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK__Users__1788CC4CE14C9D38",
                table: "Users",
                column: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles",
                column: "UserRoleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Roles",
                table: "Roles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "UQ__Users__536C85E4755F0D07",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Users_UserId",
                table: "UserRoles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
