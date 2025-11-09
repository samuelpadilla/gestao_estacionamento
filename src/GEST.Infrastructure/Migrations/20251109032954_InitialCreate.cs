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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    MaxCapacity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sector", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LicensePlate = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicle", x => x.Id);
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
                    SectorId = table.Column<int>(type: "int", nullable: false),
                    Lat = table.Column<double>(type: "float", nullable: false),
                    Lng = table.Column<double>(type: "float", nullable: false),
                    IsOccupied = table.Column<bool>(type: "bit", nullable: false),
                    CurrentLicensePlate = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sector_Spot",
                        column: x => x.SectorId,
                        principalTable: "Sectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ParkingSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectorId = table.Column<int>(type: "int", nullable: true),
                    SpotId = table.Column<int>(type: "int", nullable: true),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    EntryTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ParkedTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExitTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Multiplier = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PricingTier = table.Column<int>(type: "int", nullable: false),
                    AppliedPricePerHour = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sector_ParkingSession",
                        column: x => x.SectorId,
                        principalTable: "Sectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Spot_ParkingSession",
                        column: x => x.SpotId,
                        principalTable: "Spots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Vehicle_ParkingSession",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_SpotId",
                table: "ParkingSessions",
                column: "SpotId");

            migrationBuilder.CreateIndex(
                name: "IX_Session_BySector_ExitTime",
                table: "ParkingSessions",
                columns: new[] { "SectorId", "ExitTimeUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Session_ByVehicle_Status",
                table: "ParkingSessions",
                columns: new[] { "VehicleId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Spot_BySector",
                table: "Spots",
                column: "SectorId");

            migrationBuilder.CreateIndex(
                name: "IX_Spot_BySector_IsOccupied",
                table: "Spots",
                columns: new[] { "SectorId", "IsOccupied" });

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_ByLicensePlate",
                table: "Vehicles",
                column: "LicensePlate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebhookEventLog_ByLicensePlate_ReceivedAt",
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
