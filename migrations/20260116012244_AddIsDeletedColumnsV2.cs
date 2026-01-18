using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineStoreSystem.Migrations
{
    public partial class AddIsDeletedColumnsV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Додаємо колонку IsDeleted тільки для Category
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Category",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Відкочуємо зміни лише для Category
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Category");
        }
    }
}
