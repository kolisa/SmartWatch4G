using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartWatch4G.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlarmEventRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceId = table.Column<string>(type: "TEXT", nullable: true),
                    AlarmType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    AlarmTime = table.Column<string>(type: "TEXT", nullable: false),
                    Value1 = table.Column<double>(type: "REAL", nullable: true),
                    Value2 = table.Column<double>(type: "REAL", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_AlarmEventRecords", x => x.Id));

            migrationBuilder.CreateTable(
                name: "CallLogRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CallStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    CallNumber = table.Column<string>(type: "TEXT", nullable: true),
                    StartTime = table.Column<string>(type: "TEXT", nullable: true),
                    EndTime = table.Column<string>(type: "TEXT", nullable: true),
                    IsSosAlarm = table.Column<bool>(type: "INTEGER", nullable: false),
                    AlarmTime = table.Column<string>(type: "TEXT", nullable: true),
                    AlarmLat = table.Column<string>(type: "TEXT", nullable: true),
                    AlarmLon = table.Column<string>(type: "TEXT", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_CallLogRecords", x => x.Id));

            migrationBuilder.CreateTable(
                name: "DeviceInfoRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Imsi = table.Column<string>(type: "TEXT", nullable: false),
                    Sn = table.Column<string>(type: "TEXT", nullable: false),
                    Mac = table.Column<string>(type: "TEXT", nullable: false),
                    NetType = table.Column<string>(type: "TEXT", nullable: false),
                    NetOperator = table.Column<string>(type: "TEXT", nullable: false),
                    WearingStatus = table.Column<string>(type: "TEXT", nullable: false),
                    Model = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<string>(type: "TEXT", nullable: false),
                    Sim1IccId = table.Column<string>(type: "TEXT", nullable: false),
                    Sim1CellId = table.Column<string>(type: "TEXT", nullable: false),
                    Sim1NetAdhere = table.Column<string>(type: "TEXT", nullable: false),
                    NetworkStatus = table.Column<string>(type: "TEXT", nullable: false),
                    BandDetail = table.Column<string>(type: "TEXT", nullable: false),
                    RefSignal = table.Column<string>(type: "TEXT", nullable: false),
                    Band = table.Column<string>(type: "TEXT", nullable: false),
                    CommunicationMode = table.Column<string>(type: "TEXT", nullable: false),
                    WatchEvent = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_DeviceInfoRecords", x => x.Id));

            migrationBuilder.CreateTable(
                name: "DeviceStatusRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    EventTime = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_DeviceStatusRecords", x => x.Id));

            migrationBuilder.CreateTable(
                name: "EcgDataRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceId = table.Column<string>(type: "TEXT", nullable: true),
                    DataTime = table.Column<string>(type: "TEXT", nullable: false),
                    Seq = table.Column<long>(type: "INTEGER", nullable: false),
                    SampleCount = table.Column<int>(type: "INTEGER", nullable: false),
                    RawDataBase64 = table.Column<string>(type: "TEXT", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_EcgDataRecords", x => x.Id));

            migrationBuilder.CreateTable(
                name: "GnssTrackRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceId = table.Column<string>(type: "TEXT", nullable: true),
                    TrackTime = table.Column<string>(type: "TEXT", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    GpsType = table.Column<string>(type: "TEXT", nullable: true),
                    BatteryLevel = table.Column<int>(type: "INTEGER", nullable: true),
                    Rssi = table.Column<int>(type: "INTEGER", nullable: true),
                    Steps = table.Column<long>(type: "INTEGER", nullable: true),
                    DistanceMetres = table.Column<float>(type: "REAL", nullable: true),
                    CaloriesKcal = table.Column<float>(type: "REAL", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_GnssTrackRecords", x => x.Id));

            migrationBuilder.CreateTable(
                name: "HealthDataRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceId = table.Column<string>(type: "TEXT", nullable: true),
                    DataTime = table.Column<string>(type: "TEXT", nullable: false),
                    Seq = table.Column<long>(type: "INTEGER", nullable: false),
                    Steps = table.Column<long>(type: "INTEGER", nullable: true),
                    DistanceMetres = table.Column<float>(type: "REAL", nullable: true),
                    CaloriesKcal = table.Column<float>(type: "REAL", nullable: true),
                    ActivityType = table.Column<long>(type: "INTEGER", nullable: true),
                    ActivityState = table.Column<long>(type: "INTEGER", nullable: true),
                    AvgHeartRate = table.Column<long>(type: "INTEGER", nullable: true),
                    MaxHeartRate = table.Column<long>(type: "INTEGER", nullable: true),
                    MinHeartRate = table.Column<long>(type: "INTEGER", nullable: true),
                    AvgSpo2 = table.Column<long>(type: "INTEGER", nullable: true),
                    MaxSpo2 = table.Column<long>(type: "INTEGER", nullable: true),
                    MinSpo2 = table.Column<long>(type: "INTEGER", nullable: true),
                    Sbp = table.Column<long>(type: "INTEGER", nullable: true),
                    Dbp = table.Column<long>(type: "INTEGER", nullable: true),
                    HrvSdnn = table.Column<double>(type: "REAL", nullable: true),
                    HrvRmssd = table.Column<double>(type: "REAL", nullable: true),
                    HrvPnn50 = table.Column<double>(type: "REAL", nullable: true),
                    HrvMean = table.Column<double>(type: "REAL", nullable: true),
                    Fatigue = table.Column<int>(type: "INTEGER", nullable: true),
                    AxillaryTemp = table.Column<float>(type: "REAL", nullable: true),
                    EstimatedTemp = table.Column<float>(type: "REAL", nullable: true),
                    ShellTemp = table.Column<float>(type: "REAL", nullable: true),
                    EnvTemp = table.Column<float>(type: "REAL", nullable: true),
                    SleepDataJson = table.Column<string>(type: "TEXT", nullable: true),
                    BiozR = table.Column<int>(type: "INTEGER", nullable: true),
                    BiozX = table.Column<int>(type: "INTEGER", nullable: true),
                    BodyFat = table.Column<float>(type: "REAL", nullable: true),
                    Bmi = table.Column<float>(type: "REAL", nullable: true),
                    BloodSugar = table.Column<float>(type: "REAL", nullable: true),
                    BloodPotassium = table.Column<float>(type: "REAL", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_HealthDataRecords", x => x.Id));

            migrationBuilder.CreateTable(
                name: "RriDataRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceId = table.Column<string>(type: "TEXT", nullable: true),
                    DataTime = table.Column<string>(type: "TEXT", nullable: false),
                    Seq = table.Column<long>(type: "INTEGER", nullable: false),
                    SampleCount = table.Column<int>(type: "INTEGER", nullable: false),
                    RriValuesJson = table.Column<string>(type: "TEXT", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_RriDataRecords", x => x.Id));

            migrationBuilder.CreateTable(
                name: "SleepDataRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceId = table.Column<string>(type: "TEXT", nullable: true),
                    SleepDate = table.Column<string>(type: "TEXT", nullable: false),
                    DataTime = table.Column<string>(type: "TEXT", nullable: false),
                    Seq = table.Column<long>(type: "INTEGER", nullable: false),
                    SleepJson = table.Column<string>(type: "TEXT", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_SleepDataRecords", x => x.Id));

            // ── Indexes ──────────────────────────────────────────────────────

            migrationBuilder.CreateIndex("IX_AlarmEventRecords_DeviceId_AlarmType_AlarmTime",
                "AlarmEventRecords", new[] { "DeviceId", "AlarmType", "AlarmTime" });

            migrationBuilder.CreateIndex("IX_CallLogRecords_DeviceId_IsSosAlarm",
                "CallLogRecords", new[] { "DeviceId", "IsSosAlarm" });

            migrationBuilder.CreateIndex("IX_DeviceInfoRecords_DeviceId",
                "DeviceInfoRecords", "DeviceId", unique: true);

            migrationBuilder.CreateIndex("IX_DeviceStatusRecords_DeviceId_ReceivedAt",
                "DeviceStatusRecords", new[] { "DeviceId", "ReceivedAt" });

            migrationBuilder.CreateIndex("IX_EcgDataRecords_DeviceId_DataTime",
                "EcgDataRecords", new[] { "DeviceId", "DataTime" });

            migrationBuilder.CreateIndex("IX_GnssTrackRecords_DeviceId_TrackTime",
                "GnssTrackRecords", new[] { "DeviceId", "TrackTime" });

            migrationBuilder.CreateIndex("IX_HealthDataRecords_DeviceId_DataTime",
                "HealthDataRecords", new[] { "DeviceId", "DataTime" });

            migrationBuilder.CreateIndex("IX_RriDataRecords_DeviceId_DataTime",
                "RriDataRecords", new[] { "DeviceId", "DataTime" });

            migrationBuilder.CreateIndex("IX_SleepDataRecords_DeviceId_SleepDate_Seq",
                "SleepDataRecords", new[] { "DeviceId", "SleepDate", "Seq" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("AlarmEventRecords");
            migrationBuilder.DropTable("CallLogRecords");
            migrationBuilder.DropTable("DeviceInfoRecords");
            migrationBuilder.DropTable("DeviceStatusRecords");
            migrationBuilder.DropTable("EcgDataRecords");
            migrationBuilder.DropTable("GnssTrackRecords");
            migrationBuilder.DropTable("HealthDataRecords");
            migrationBuilder.DropTable("RriDataRecords");
            migrationBuilder.DropTable("SleepDataRecords");
        }
    }
}
