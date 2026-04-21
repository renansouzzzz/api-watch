using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiWatch.Core.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MonitoredEndpoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    IntervalSeconds = table.Column<int>(type: "integer", nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoredEndpoints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CheckResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MonitoredEndpointId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsUp = table.Column<bool>(type: "boolean", nullable: false),
                    StatusCode = table.Column<int>(type: "integer", nullable: true),
                    LatencyMs = table.Column<double>(type: "double precision", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    CheckedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckResults_MonitoredEndpoints_MonitoredEndpointId",
                        column: x => x.MonitoredEndpointId,
                        principalTable: "MonitoredEndpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Incidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MonitoredEndpointId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Cause = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incidents_MonitoredEndpoints_MonitoredEndpointId",
                        column: x => x.MonitoredEndpointId,
                        principalTable: "MonitoredEndpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "MonitoredEndpoints",
                columns: new[] { "Id", "CreatedAt", "IntervalSeconds", "IsActive", "Name", "TimeoutSeconds", "UpdatedAt", "Url" },
                values: new object[] { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 4, 21, 18, 35, 36, 941, DateTimeKind.Utc).AddTicks(5536), 60, true, "Google", 10, null, "https://www.google.com" });

            migrationBuilder.CreateIndex(
                name: "IX_CheckResults_CheckedAt",
                table: "CheckResults",
                column: "CheckedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CheckResults_MonitoredEndpointId",
                table: "CheckResults",
                column: "MonitoredEndpointId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_MonitoredEndpointId",
                table: "Incidents",
                column: "MonitoredEndpointId");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoredEndpoints_Url",
                table: "MonitoredEndpoints",
                column: "Url");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckResults");

            migrationBuilder.DropTable(
                name: "Incidents");

            migrationBuilder.DropTable(
                name: "MonitoredEndpoints");
        }
    }
}
