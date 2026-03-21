# SmartWatch4G — 4G Wearable Device Data API

A **.NET 10** ASP.NET Core Web API built with **Clean Architecture**, replacing the original
single-project `net8` flat structure.

---

## Architecture

```
SmartWatch4G.sln
├── Directory.Packages.props          ← Centralised NuGet versions (CPM)
├── Directory.Build.props             ← net10.0, TreatWarningsAsErrors=true, nullable
│
└── src/
    ├── SmartWatch4G.Domain/             ← Entities, repository interfaces, service interfaces
    │                                    No external dependencies.
    ├── SmartWatch4G.Application/        ← DTOs, DateTimeUtilities
    │                                    Depends on: Domain only.
    ├── SmartWatch4G.Infrastructure/     ← EF Core (SQLite), repositories, processors
    │   ├── Persistence/              ← AppDbContext, repositories, migrations
    │   ├── Processors/               ← Protobuf packet processors + dispatcher
    │   ├── Services/                 ← SleepQueryService
    │   ├── Extensions/               ← InfrastructureServiceExtensions (DI wiring)
    │   └── Protobuf/                 ← Auto-generated protobuf C# files (DO NOT EDIT)
    │                                    Depends on: Domain, Application, Google.Protobuf, EF Core.
    └── SmartWatch4G.Api/                ← Controllers, Program.cs
                                         Depends on: Application, Infrastructure.
```

### Key design decisions

| Decision | Rationale |
|---|---|
| `TreatWarningsAsErrors=true` on hand-written code | Zero warnings enforced at build time |
| Protobuf files isolated in `Infrastructure/Protobuf/` with local `Directory.Build.props` override | Auto-generated code is exempt from `TreatWarningsAsErrors` |
| `IProtobufPacketHandler` in Domain | Controllers depend only on the domain interface, not concrete processors |
| Serilog replacing `MyFileLoggerProvider` | Rolling file sink, structured logging, configurable via `appsettings.json` |
| SQLite (EF Core) replacing text-file logging | All device data persisted; `dotnet ef migrations add` supported |
| `PacketParserBase` shared base controller | Binary frame-parsing logic is not duplicated between `/pb/upload` and `/alarm/upload` |
| Scoped lifetime for processors | Processors share the same `AppDbContext` per HTTP request |

---

## Getting started

### Prerequisites
- .NET 10 SDK

### Run locally

```bash
cd src/SmartWatch4G.Api
dotnet run
```

Browse to `http://localhost:5000/swagger` for the Swagger UI.

The SQLite database (`fourGData.db`) is created automatically on first run via
`db.Database.MigrateAsync()` in `Program.cs`.

### Adding a new EF Core migration

```bash
cd src/SmartWatch4G.Infrastructure
dotnet ef migrations add <MigrationName> \
    --startup-project ../SmartWatch4G.Api \
    --output-dir Persistence/Migrations
```

---

## API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/pb/upload` | Binary protobuf upload — health history (0x80), OldMan GPS (0x0A) |
| `POST` | `/alarm/upload` | Binary protobuf upload — alarm events (0x12) |
| `POST` | `/call_log/upload` | JSON call-log upload |
| `POST` | `/deviceinfo/upload` | JSON device-info registration/update |
| `POST` | `/status/notify` | JSON device-status notification |
| `GET` | `/health/sleep` | Sleep result query (`?deviceid=&sleep_date=yyyy-MM-dd`) |

### Binary packet protocol

```
[0..14]  15 bytes — Device ID (UTF-8, null-padded)
repeat {
  [+0..+1]  0x44 0x54  — Frame prefix
  [+2..+3]  LE uint16  — Protobuf payload length
  [+4..+5]  LE uint16  — CRC (not verified)
  [+6..+7]  LE uint16  — Opcode (0x0A = OldMan, 0x80 = HisData, 0x12 = Alarm)
  [+8..]    N bytes    — Protobuf payload
}
```

### Response codes

| Code | Meaning |
|------|---------|
| `0` | Success |
| `10002` | Bad request / deserialisation failure |
| `10404` | Data not found |

---

## Database tables

| Table | Purpose |
|---|---|
| `DeviceInfoRecords` | One row per device; upserted on `/deviceinfo/upload` |
| `DeviceStatusRecords` | Append-only status history |
| `CallLogRecords` | Normal and SOS call-logs (`IsSosAlarm` flag) |
| `AlarmEventRecords` | All alarm types with typed `AlarmType`, `Value1`, `Value2` |
| `HealthDataRecords` | Per-slot health snapshots (HR, BP, SpO2, HRV, temperature, steps, …) |
| `SleepDataRecords` | Per-slot sleep JSON for downstream sleep-stage calculation |
| `EcgDataRecords` | ECG chunks keyed by device + data_time (base64-encoded raw data) |
| `RriDataRecords` | RRI sequences for AF calculation (JSON array of ms values) |
| `GnssTrackRecords` | WGS-84 GPS track-points from OldMan (OM0) devices |

---

## Extending

- **Sleep algorithm**: implement the body of `SleepQueryService.GetSleepResultAsync`, reading from `SleepDataRecords`.
- **AF / ECG analysis**: read from `RriDataRecords` / `EcgDataRecords` and call your calculation engine.
- **Switch to SQL Server**: replace `UseSqlite(...)` with `UseSqlServer(...)` in `InfrastructureServiceExtensions` and update `Directory.Packages.props` to include `Microsoft.EntityFrameworkCore.SqlServer`.
