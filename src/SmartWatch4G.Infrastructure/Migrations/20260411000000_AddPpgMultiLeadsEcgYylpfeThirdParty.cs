using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartWatch4G.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPpgMultiLeadsEcgYylpfeThirdParty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PpgDataRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    DataTime = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Seq = table.Column<long>(type: "bigint", nullable: false),
                    SampleCount = table.Column<int>(type: "int", nullable: false),
                    RawDataJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PpgDataRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MultiLeadsEcgRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    DataTime = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Seq = table.Column<long>(type: "bigint", nullable: false),
                    Channels = table.Column<int>(type: "int", nullable: false),
                    SampleByteLen = table.Column<int>(type: "int", nullable: false),
                    RawDataBase64 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiLeadsEcgRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "YylpfeRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    DataTime = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Seq = table.Column<long>(type: "bigint", nullable: false),
                    AreaUp = table.Column<int>(type: "int", nullable: false),
                    AreaDown = table.Column<int>(type: "int", nullable: false),
                    Rri = table.Column<int>(type: "int", nullable: false),
                    Motion = table.Column<int>(type: "int", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YylpfeRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThirdPartyDataRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    MacAddr = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    DataTime = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    BpSbp = table.Column<int>(type: "int", nullable: true),
                    BpDbp = table.Column<int>(type: "int", nullable: true),
                    BpHr = table.Column<int>(type: "int", nullable: true),
                    BpPulse = table.Column<int>(type: "int", nullable: true),
                    ScaleWeight = table.Column<float>(type: "real", nullable: true),
                    ScaleImpedance = table.Column<float>(type: "real", nullable: true),
                    ScaleBodyFatPercentage = table.Column<float>(type: "real", nullable: true),
                    OximeterSpo2 = table.Column<int>(type: "int", nullable: true),
                    OximeterHr = table.Column<int>(type: "int", nullable: true),
                    OximeterPi = table.Column<float>(type: "real", nullable: true),
                    BodyTemp = table.Column<float>(type: "real", nullable: true),
                    BloodGlucose = table.Column<float>(type: "real", nullable: true),
                    BloodKetones = table.Column<float>(type: "real", nullable: true),
                    UricAcid = table.Column<float>(type: "real", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThirdPartyDataRecords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PpgDataRecords_DeviceId_DataTime",
                table: "PpgDataRecords",
                columns: new[] { "DeviceId", "DataTime" });

            migrationBuilder.CreateIndex(
                name: "IX_MultiLeadsEcgRecords_DeviceId_DataTime",
                table: "MultiLeadsEcgRecords",
                columns: new[] { "DeviceId", "DataTime" });

            migrationBuilder.CreateIndex(
                name: "IX_YylpfeRecords_DeviceId_DataTime",
                table: "YylpfeRecords",
                columns: new[] { "DeviceId", "DataTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ThirdPartyDataRecords_DeviceId_DataTime",
                table: "ThirdPartyDataRecords",
                columns: new[] { "DeviceId", "DataTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PpgDataRecords");
            migrationBuilder.DropTable(name: "MultiLeadsEcgRecords");
            migrationBuilder.DropTable(name: "YylpfeRecords");
            migrationBuilder.DropTable(name: "ThirdPartyDataRecords");
        }
    }
}
