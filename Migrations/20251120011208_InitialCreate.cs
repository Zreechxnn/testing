using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace testing.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kelas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nama = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kelas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ruangan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nama = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ruangan", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kartu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Uid = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: true, defaultValue: "AKTIF"),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    KelasId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kartu", x => x.Id);
                    table.CheckConstraint("CK_Kartu_SingleOwner", "\"UserId\" IS NULL AND \"KelasId\" IS NOT NULL OR \r\n                      \"UserId\" IS NOT NULL AND \"KelasId\" IS NULL OR \r\n                      \"UserId\" IS NULL AND \"KelasId\" IS NULL");
                    table.ForeignKey(
                        name: "FK_Kartu_Kelas_KelasId",
                        column: x => x.KelasId,
                        principalTable: "Kelas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Kartu_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AksesLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KartuId = table.Column<int>(type: "integer", nullable: false),
                    RuanganId = table.Column<int>(type: "integer", nullable: false),
                    TimestampMasuk = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimestampKeluar = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AksesLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AksesLog_Kartu_KartuId",
                        column: x => x.KartuId,
                        principalTable: "Kartu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AksesLog_Ruangan_RuanganId",
                        column: x => x.RuanganId,
                        principalTable: "Ruangan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AksesLog_KartuId",
                table: "AksesLog",
                column: "KartuId");

            migrationBuilder.CreateIndex(
                name: "IX_AksesLog_RuanganId",
                table: "AksesLog",
                column: "RuanganId");

            migrationBuilder.CreateIndex(
                name: "IX_Kartu_KelasId",
                table: "Kartu",
                column: "KelasId");

            migrationBuilder.CreateIndex(
                name: "IX_Kartu_Uid",
                table: "Kartu",
                column: "Uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kartu_UserId",
                table: "Kartu",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AksesLog");

            migrationBuilder.DropTable(
                name: "Kartu");

            migrationBuilder.DropTable(
                name: "Ruangan");

            migrationBuilder.DropTable(
                name: "Kelas");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
