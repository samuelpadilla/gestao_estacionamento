using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEST.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sectors",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    MaxCapacity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sectors", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    LicensePlate = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.LicensePlate);
                });

            migrationBuilder.CreateTable(
                name: "WebhookEventLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    LicensePlate = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    ReceivedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EntryTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExitTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Lat = table.Column<double>(type: "float", nullable: true),
                    Lng = table.Column<double>(type: "float", nullable: true),
                    RawPayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookEventLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Spots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectorCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Lat = table.Column<double>(type: "float", nullable: false),
                    Lng = table.Column<double>(type: "float", nullable: false),
                    IsOccupied = table.Column<bool>(type: "bit", nullable: false),
                    CurrentLicensePlate = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Spots_Sectors_SectorCode",
                        column: x => x.SectorCode,
                        principalTable: "Sectors",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ParkingSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LicensePlate = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    SpotId = table.Column<int>(type: "int", nullable: true),
                    SectorCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    EntryTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ParkedTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExitTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AppliedPricePerHour = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    PricingTier = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParkingSessions_Sectors_SectorCode",
                        column: x => x.SectorCode,
                        principalTable: "Sectors",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParkingSessions_Spots_SpotId",
                        column: x => x.SpotId,
                        principalTable: "Spots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ParkingSessions_Vehicles_LicensePlate",
                        column: x => x.LicensePlate,
                        principalTable: "Vehicles",
                        principalColumn: "LicensePlate",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_SectorCode_ExitTimeUtc",
                table: "ParkingSessions",
                columns: new[] { "SectorCode", "ExitTimeUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_SpotId",
                table: "ParkingSessions",
                column: "SpotId");

            migrationBuilder.CreateIndex(
                name: "IX_Session_ActiveByPlate",
                table: "ParkingSessions",
                columns: new[] { "LicensePlate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Spots_SectorCode",
                table: "Spots",
                column: "SectorCode");

            migrationBuilder.CreateIndex(
                name: "IX_Spots_SectorCode_IsOccupied",
                table: "Spots",
                columns: new[] { "SectorCode", "IsOccupied" });

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_LicensePlate",
                table: "Vehicles",
                column: "LicensePlate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebhookEventLogs_LicensePlate_ReceivedAtUtc",
                table: "WebhookEventLogs",
                columns: new[] { "LicensePlate", "ReceivedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParkingSessions");

            migrationBuilder.DropTable(
                name: "WebhookEventLogs");

            migrationBuilder.DropTable(
                name: "Spots");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "Sectors");
        }
    }
}
