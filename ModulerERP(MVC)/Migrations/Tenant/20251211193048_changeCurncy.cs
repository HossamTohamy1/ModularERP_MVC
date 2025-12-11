using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModulerERP_MVC_.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class changeCurncy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Currencies_Code",
                table: "Currencies");

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_IsDeleted",
                table: "Currencies",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Name",
                table: "Currencies",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Currencies_IsDeleted",
                table: "Currencies");

            migrationBuilder.DropIndex(
                name: "IX_Currencies_Name",
                table: "Currencies");

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Code",
                table: "Currencies",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");
        }
    }
}
