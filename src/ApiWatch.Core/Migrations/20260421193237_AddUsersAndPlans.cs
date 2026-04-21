using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ApiWatch.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersAndPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "MonitoredEndpoints",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PriceMonthly = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    MaxEndpoints = table.Column<int>(type: "integer", nullable: false),
                    MinIntervalSeconds = table.Column<int>(type: "integer", nullable: false),
                    HistoryDays = table.Column<int>(type: "integer", nullable: false),
                    HasEmailAlerts = table.Column<bool>(type: "boolean", nullable: false),
                    HasWebhooks = table.Column<bool>(type: "boolean", nullable: false),
                    HasStatusPage = table.Column<bool>(type: "boolean", nullable: false),
                    MaxTeamMembers = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    PlanId = table.Column<int>(type: "integer", nullable: false),
                    StripeCustomerId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "MonitoredEndpoints",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "UserId" },
                values: new object[] { new DateTime(2026, 4, 21, 19, 32, 36, 991, DateTimeKind.Utc).AddTicks(5121), null });

            migrationBuilder.InsertData(
                table: "Plans",
                columns: new[] { "Id", "HasEmailAlerts", "HasStatusPage", "HasWebhooks", "HistoryDays", "MaxEndpoints", "MaxTeamMembers", "MinIntervalSeconds", "Name", "PriceMonthly" },
                values: new object[,]
                {
                    { 1, false, false, false, 7, 3, 1, 300, "Free", 0m },
                    { 2, true, false, false, 30, 15, 1, 60, "Starter", 12m },
                    { 3, true, true, true, 90, 50, 3, 30, "Pro", 39m },
                    { 4, true, true, true, 365, -1, 10, 30, "Business", 89m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonitoredEndpoints_UserId",
                table: "MonitoredEndpoints",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_PlanId",
                table: "Users",
                column: "PlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_MonitoredEndpoints_Users_UserId",
                table: "MonitoredEndpoints",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MonitoredEndpoints_Users_UserId",
                table: "MonitoredEndpoints");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Plans");

            migrationBuilder.DropIndex(
                name: "IX_MonitoredEndpoints_UserId",
                table: "MonitoredEndpoints");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "MonitoredEndpoints");

            migrationBuilder.UpdateData(
                table: "MonitoredEndpoints",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 21, 18, 35, 36, 941, DateTimeKind.Utc).AddTicks(5536));
        }
    }
}
