using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Norse.Reference.Data.Migrations.SqlServer.Migrations;

/// <inheritdoc />
public partial class _20260723004352_InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Region",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                Level = table.Column<int>(type: "int", nullable: false),
                ParentRegionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Region", x => x.Id);
                table.ForeignKey(
                    name: "FK_Region_Region_ParentRegionId",
                    column: x => x.ParentRegionId,
                    principalTable: "Region",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "CountryOrArea",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<int>(type: "int", nullable: false),
                Alpha2 = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                Alpha3 = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                ParentRegionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Classification = table.Column<int>(type: "int", nullable: false),
                View = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CountryOrArea", x => x.Id);
                table.ForeignKey(
                    name: "FK_CountryOrArea_Region_ParentRegionId",
                    column: x => x.ParentRegionId,
                    principalTable: "Region",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_CountryOrArea_Alpha2",
            table: "CountryOrArea",
            column: "Alpha2",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_CountryOrArea_Alpha3",
            table: "CountryOrArea",
            column: "Alpha3",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_CountryOrArea_Code",
            table: "CountryOrArea",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_CountryOrArea_ParentRegionId",
            table: "CountryOrArea",
            column: "ParentRegionId");

        migrationBuilder.CreateIndex(
            name: "IX_Region_Code",
            table: "Region",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Region_ParentRegionId",
            table: "Region",
            column: "ParentRegionId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CountryOrArea");

        migrationBuilder.DropTable(
            name: "Region");
    }
}
