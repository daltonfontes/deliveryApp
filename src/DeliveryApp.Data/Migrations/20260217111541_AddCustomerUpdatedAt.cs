using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerUpdatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "customers",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "customers");
        }
    }
}
