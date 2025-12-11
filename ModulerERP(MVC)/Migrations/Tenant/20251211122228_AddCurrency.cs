using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModulerERP_MVC_.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Companies_CurrencyCode",
                table: "Companies",
                column: "CurrencyCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_Currencies_CurrencyCode",
                table: "Companies",
                column: "CurrencyCode",
                principalTable: "Currencies",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_Currencies_CurrencyCode",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_CurrencyCode",
                table: "Companies");
        }
    }
}
