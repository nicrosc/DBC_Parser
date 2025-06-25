using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ZadatakJuric.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DBCEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Networks");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Networks");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Networks");

            migrationBuilder.AddColumn<int>(
                name: "DBCId",
                table: "Networks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DBCId",
                table: "Attributes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DBCFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DBCFiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Networks_DBCId",
                table: "Networks",
                column: "DBCId");

            migrationBuilder.CreateIndex(
                name: "IX_Attributes_DBCId",
                table: "Attributes",
                column: "DBCId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attributes_DBCFiles_DBCId",
                table: "Attributes",
                column: "DBCId",
                principalTable: "DBCFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Networks_DBCFiles_DBCId",
                table: "Networks",
                column: "DBCId",
                principalTable: "DBCFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attributes_DBCFiles_DBCId",
                table: "Attributes");

            migrationBuilder.DropForeignKey(
                name: "FK_Networks_DBCFiles_DBCId",
                table: "Networks");

            migrationBuilder.DropTable(
                name: "DBCFiles");

            migrationBuilder.DropIndex(
                name: "IX_Networks_DBCId",
                table: "Networks");

            migrationBuilder.DropIndex(
                name: "IX_Attributes_DBCId",
                table: "Attributes");

            migrationBuilder.DropColumn(
                name: "DBCId",
                table: "Networks");

            migrationBuilder.DropColumn(
                name: "DBCId",
                table: "Attributes");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Networks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Networks",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "Networks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
