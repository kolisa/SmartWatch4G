using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartWatch4G.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSpo2AndAccTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Spo2DataRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceId = table.Column<string>(type: "TEXT", nullable: true),
                    DataTime = table.Column<string>(type: "TEXT", nullable: false),
                    Spo2 = table.Column<int>(type: "INTEGER", nullable: false),
                    HeartRate = table.Column<int>(type: "INTEGER", nullable: false),
                    Perfusion = table.Column<int>(type: "INTEGER", nullable: false),
                    Touch = table.Column<int>(type: "INTEGER", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Spo2DataRecords", x => x.Id));

            migrationBuilder.CreateTable(
                name: "AccDataRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceId = table.Column<string>(type: "TEXT", nullable: true),
                    DataTime = table.Column<string>(type: "TEXT", nullable: false),
                    XValuesJson = table.Column<string>(type: "TEXT", nullable: false),
                    YValuesJson = table.Column<string>(type: "TEXT", nullable: false),
                    ZValuesJson = table.Column<string>(type: "TEXT", nullable: false),
                    SampleCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_AccDataRecords", x => x.Id));

            migrationBuilder.CreateIndex(
                name: "IX_Spo2DataRecords_DeviceId_DataTime",
                table: "Spo2DataRecords",
                columns: new[] { "DeviceId", "DataTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AccDataRecords_DeviceId_DataTime",
                table: "AccDataRecords",
                columns: new[] { "DeviceId", "DataTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("Spo2DataRecords");
            migrationBuilder.DropTable("AccDataRecords");
        }
    }
}
