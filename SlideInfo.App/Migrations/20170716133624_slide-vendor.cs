using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SlideInfo.App.Migrations
{
    public partial class slidevendor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuickHash",
                table: "Slides");

            migrationBuilder.AddColumn<string>(
                name: "Vendor",
                table: "Slides",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Vendor",
                table: "Slides");

            migrationBuilder.AddColumn<int>(
                name: "QuickHash",
                table: "Slides",
                nullable: false,
                defaultValue: 0);
        }
    }
}
