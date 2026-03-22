using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartWatch4G.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccDataRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    DataTime = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    XValuesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    YValuesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ZValuesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SampleCount = table.Column<int>(type: "int", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccDataRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AlarmEventRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    AlarmType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AlarmTime = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Value1 = table.Column<double>(type: "float", nullable: true),
                    Value2 = table.Column<double>(type: "float", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlarmEventRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CallLogRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CallStatus = table.Column<int>(type: "int", nullable: false),
                    CallNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EndTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSosAlarm = table.Column<bool>(type: "bit", nullable: false),
                    AlarmTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AlarmLat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AlarmLon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CallLogRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviceInfoRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Imsi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mac = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NetType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NetOperator = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WearingStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sim1IccId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sim1CellId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sim1NetAdhere = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NetworkStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BandDetail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefSignal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Band = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CommunicationMode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WatchEvent = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceInfoRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviceStatusRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    EventTime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceStatusRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EcgDataRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    DataTime = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Seq = table.Column<long>(type: "bigint", nullable: false),
                    SampleCount = table.Column<int>(type: "int", nullable: false),
                    RawDataBase64 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcgDataRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GnssTrackRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    TrackTime = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    GpsType = table.Column<int>(type: "int", nullable: false),
                    BatteryLevel = table.Column<int>(type: "int", nullable: true),
                    Rssi = table.Column<int>(type: "int", nullable: true),
                    Steps = table.Column<long>(type: "bigint", nullable: true),
                    DistanceMetres = table.Column<float>(type: "real", nullable: true),
                    CaloriesKcal = table.Column<float>(type: "real", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GnssTrackRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HealthDataRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    DataTime = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Seq = table.Column<long>(type: "bigint", nullable: false),
                    Steps = table.Column<long>(type: "bigint", nullable: true),
                    DistanceMetres = table.Column<float>(type: "real", nullable: true),
                    CaloriesKcal = table.Column<float>(type: "real", nullable: true),
                    ActivityType = table.Column<long>(type: "bigint", nullable: true),
                    ActivityState = table.Column<long>(type: "bigint", nullable: true),
                    AvgHeartRate = table.Column<long>(type: "bigint", nullable: true),
                    MaxHeartRate = table.Column<long>(type: "bigint", nullable: true),
                    MinHeartRate = table.Column<long>(type: "bigint", nullable: true),
                    AvgSpo2 = table.Column<long>(type: "bigint", nullable: true),
                    MaxSpo2 = table.Column<long>(type: "bigint", nullable: true),
                    MinSpo2 = table.Column<long>(type: "bigint", nullable: true),
                    Sbp = table.Column<long>(type: "bigint", nullable: true),
                    Dbp = table.Column<long>(type: "bigint", nullable: true),
                    HrvSdnn = table.Column<double>(type: "float", nullable: true),
                    HrvRmssd = table.Column<double>(type: "float", nullable: true),
                    HrvPnn50 = table.Column<double>(type: "float", nullable: true),
                    HrvMean = table.Column<double>(type: "float", nullable: true),
                    Fatigue = table.Column<int>(type: "int", nullable: true),
                    TemperatureIsValid = table.Column<int>(type: "int", nullable: true),
                    AxillaryTemp = table.Column<float>(type: "real", nullable: true),
                    EstimatedTemp = table.Column<float>(type: "real", nullable: true),
                    ShellTemp = table.Column<float>(type: "real", nullable: true),
                    EnvTemp = table.Column<float>(type: "real", nullable: true),
                    MatressTemperature = table.Column<float>(type: "real", nullable: true),
                    MatressHumidity = table.Column<float>(type: "real", nullable: true),
                    SleepDataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BiozR = table.Column<int>(type: "int", nullable: true),
                    BiozX = table.Column<int>(type: "int", nullable: true),
                    BodyFat = table.Column<float>(type: "real", nullable: true),
                    Bmi = table.Column<float>(type: "real", nullable: true),
                    BloodSugar = table.Column<float>(type: "real", nullable: true),
                    BloodPotassium = table.Column<float>(type: "real", nullable: true),
                    BpBpm = table.Column<long>(type: "bigint", nullable: true),
                    UricAcid = table.Column<long>(type: "bigint", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthDataRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RriDataRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    DataTime = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Seq = table.Column<long>(type: "bigint", nullable: false),
                    SampleCount = table.Column<int>(type: "int", nullable: false),
                    RriValuesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RriDataRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SleepDataRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    SleepDate = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    DataTime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Seq = table.Column<long>(type: "bigint", nullable: false),
                    SleepJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SleepDataRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Spo2DataRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    DataTime = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Spo2 = table.Column<int>(type: "int", nullable: false),
                    HeartRate = table.Column<int>(type: "int", nullable: false),
                    Perfusion = table.Column<int>(type: "int", nullable: false),
                    Touch = table.Column<int>(type: "int", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spo2DataRecords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccDataRecords_DeviceId_DataTime",
                table: "AccDataRecords",
                columns: new[] { "DeviceId", "DataTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AlarmEventRecords_DeviceId_AlarmType_AlarmTime",
                table: "AlarmEventRecords",
                columns: new[] { "DeviceId", "AlarmType", "AlarmTime" });

            migrationBuilder.CreateIndex(
                name: "IX_CallLogRecords_DeviceId_IsSosAlarm",
                table: "CallLogRecords",
                columns: new[] { "DeviceId", "IsSosAlarm" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceInfoRecords_DeviceId",
                table: "DeviceInfoRecords",
                column: "DeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceStatusRecords_DeviceId_ReceivedAt",
                table: "DeviceStatusRecords",
                columns: new[] { "DeviceId", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EcgDataRecords_DeviceId_DataTime",
                table: "EcgDataRecords",
                columns: new[] { "DeviceId", "DataTime" });

            migrationBuilder.CreateIndex(
                name: "IX_GnssTrackRecords_DeviceId_TrackTime",
                table: "GnssTrackRecords",
                columns: new[] { "DeviceId", "TrackTime" });

            migrationBuilder.CreateIndex(
                name: "IX_HealthDataRecords_DeviceId_DataTime",
                table: "HealthDataRecords",
                columns: new[] { "DeviceId", "DataTime" });

            migrationBuilder.CreateIndex(
                name: "IX_RriDataRecords_DeviceId_DataTime",
                table: "RriDataRecords",
                columns: new[] { "DeviceId", "DataTime" });

            migrationBuilder.CreateIndex(
                name: "IX_SleepDataRecords_DeviceId_SleepDate_Seq",
                table: "SleepDataRecords",
                columns: new[] { "DeviceId", "SleepDate", "Seq" });

            migrationBuilder.CreateIndex(
                name: "IX_Spo2DataRecords_DeviceId_DataTime",
                table: "Spo2DataRecords",
                columns: new[] { "DeviceId", "DataTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccDataRecords");

            migrationBuilder.DropTable(
                name: "AlarmEventRecords");

            migrationBuilder.DropTable(
                name: "CallLogRecords");

            migrationBuilder.DropTable(
                name: "DeviceInfoRecords");

            migrationBuilder.DropTable(
                name: "DeviceStatusRecords");

            migrationBuilder.DropTable(
                name: "EcgDataRecords");

            migrationBuilder.DropTable(
                name: "GnssTrackRecords");

            migrationBuilder.DropTable(
                name: "HealthDataRecords");

            migrationBuilder.DropTable(
                name: "RriDataRecords");

            migrationBuilder.DropTable(
                name: "SleepDataRecords");

            migrationBuilder.DropTable(
                name: "Spo2DataRecords");
        }
    }
}
