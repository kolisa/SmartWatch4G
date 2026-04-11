# SmartWatch4G — 4G Wearable Device Data API

A **.NET 10** ASP.NET Core Web API built with **Clean Architecture**, fully aligned with the
[iwown IoT 1.0.1 documentation](https://api8.iwown.com/iot_platform/index.html).

**Reference:** [iwown IoT RESTful API specification](https://iwap1.iwown.com/iot_platform/restful.html)

---

## Architecture

```
SmartWatch4G.sln
├── Directory.Packages.props     ← Centralised NuGet versions (CPM)
├── Directory.Build.props        ← net10.0, TreatWarningsAsErrors=true, nullable
│
└── src/
    ├── SmartWatch4G.Domain/           ← Entities, repository & service interfaces
    ├── SmartWatch4G.Application/      ← DTOs, DateTimeUtilities
    ├── SmartWatch4G.Infrastructure/   ← EF Core, repositories, protobuf processors,
    │   ├── Persistence/               ← AppDbContext, repositories, migrations
    │   ├── Processors/                ← Packet processors + dispatcher
    │   ├── Services/                  ← Algo client, command client, sleep service
    │   ├── Extensions/                ← DI wiring
    │   └── Protobuf/                  ← Auto-generated protobuf C# (DO NOT EDIT)
    └── SmartWatch4G.Api/              ← Controllers, Program.cs
```

---

## Getting started

```bash
cd src/SmartWatch4G.Api
dotnet run
```

Browse to `http://localhost:5000/swagger`. The SQLite database (`smartwatch4g.db`) is created
automatically on first run via `db.Database.MigrateAsync()`.

**Before going live**, set your iwown credentials in `appsettings.json`:

```json
"WownAlgo": {
  "BaseUrl": "https://api1.iwown.com/algoservice",
  "Account": "YOUR_ACCOUNT",
  "Password": "YOUR_PASSWORD"
},
"WownCommand": {
  "BaseUrl": "https://search.iwown.com",
  "Account": "YOUR_ACCOUNT",
  "Password": "YOUR_PASSWORD"
}
```

Use `https://iwap1.iwown.com/algoservice` and `https://euapi.iwown.com` if your server is
**outside mainland China**.

---

## API Endpoints (data upload — device → server)

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/pb/upload` | Binary protobuf — health history (0x80), OldMan GPS (0x0A) |
| `POST` | `/alarm/upload` | Binary protobuf — alarm events (0x12) |
| `POST` | `/call_log/upload` | JSON SOS + call log upload |
| `POST` | `/deviceinfo/upload` | JSON device-info registration/update |
| `POST` | `/status/notify` | JSON device online/offline notification |
| `GET`  | `/health/sleep` | Sleep result query (`?deviceid=&sleep_date=yyyy-MM-dd`) |

The first three routes are **mandatory**. Routes 4–6 are optional per the iwown docs.

### Binary packet protocol

```
[0..14]  15 bytes — Device ID (UTF-8, null-padded)
repeat {
  [+0..+1]  0x44 0x54  — Frame prefix
  [+2..+3]  LE uint16  — Protobuf payload length
  [+4..+5]  LE uint16  — CRC (not verified server-side)
  [+6..+7]  LE uint16  — Opcode
  [+8..]    N bytes    — Protobuf payload
}
```

| Opcode | Protocol | Data |
|--------|----------|------|
| `0x0A` | OM0Report | OldMan GPS + battery + step/distance/calorie |
| `0x80` | HisNotification | All health history data (health, ECG, RRI, SpO2, ACC, PPG, multi-leads ECG, YYLPFE, third-party V1/V2) |
| `0x12` | Alarm\_infokConfirm | HR, SpO2, BP, temperature, thrombus, fall, sedentary, SOS, blood-sugar, blood-potassium, power alarms |

### Response codes

| HTTP body byte | Meaning |
|----------------|---------|
| `0x00` | Success |
| `0x01` | Failed to read request body |
| `0x02` | Packet too short / truncated |
| `0x03` | Invalid frame prefix |

---

## iwown Algorithm Service Integration

Sleep calculation is wired end-to-end. When `GET /health/sleep` is called:

1. Sleep pre-processed slots (`SleepDataRecords`) for `sleep_date` and the prior day are loaded.
2. Each day's slots are combined into the compact JSON-array string the algo API expects.
3. RRI records for both days are flattened and passed as `prevDayRri` / `nextDayRri`.
4. `POST /calculation/sleep` is called on the iwown algo service.
5. Returned section types are mapped to minute totals: `3`→`deep_sleep`, `4`→`light_sleep`, `6`→`weak_sleep`, `7`→`eyemove_sleep`.

Other algorithm endpoints available at `https://api1.iwown.com/algoservice`:

| Endpoint | Input data | Purpose |
|----------|-----------|---------|
| `POST /calculation/ecg` | `EcgDataRecords` | ECG rhythm classification (6 results) |
| `POST /calculation/af` | `RriDataRecords` | AF / arrhythmia detection |
| `POST /calculation/spo2` | `Spo2DataRecords` | Continuous SpO2 OSAHS risk scoring |
| `POST /calculation/parkinson/acc` | `AccDataRecords` | Parkinson tremor/activity scoring |
| `POST /calculation/matress/sleep` | mattress sleep slots + RRI | Mattress-based sleep staging |

---

## Entservice — Sending Commands to Devices

Inject `IWownCommandClient` to send any of the 34 supported commands:

```csharp
// Example: push user profile to device
await _commandClient.SendUserInfoAsync(new UserInfoCommand(
    DeviceId: "860132060872223",
    Height: 175, Weight: 70,
    Gender: 1, Age: 35));

// Example: set heart-rate alarm
await _commandClient.SetHrAlarmAsync(new HrAlarmCommand(
    DeviceId: "860132060872223",
    Open: true, High: 130, Low: 50,
    Threshold: 3, AlarmIntervalMinutes: 10));

// Example: query device online status
DeviceOnlineStatus? status = await _commandClient.GetDeviceStatusAsync("860132060872223");
```

Authentication is automatic — the client MD5-hashes your password and injects the
`account` / `pwd` HTTP headers on every request.

---

## Database Tables

| Table | Purpose |
|---|---|
| `DeviceInfoRecords` | One row per device; upserted on `/deviceinfo/upload` |
| `DeviceStatusRecords` | Append-only online/offline history |
| `CallLogRecords` | Normal + SOS call logs (`IsSosAlarm` flag) |
| `AlarmEventRecords` | All alarm types with typed `AlarmType`, `Value1`, `Value2` |
| `HealthDataRecords` | Per-minute health snapshots (HR, BP, SpO2, HRV, temp, steps, bioz, blood-sugar, blood-potassium, uric-acid, mattress humidity/temp, BP-BPM, temperature validity) |
| `SleepDataRecords` | Per-slot sleep JSON for downstream sleep-stage calculation |
| `EcgDataRecords` | ECG chunks (base64 raw), keyed by device + data_time |
| `RriDataRecords` | RRI sequences for AF / sleep eye-movement calculation |
| `Spo2DataRecords` | Per-sample SpO2 for continuous OSAHS risk analysis |
| `AccDataRecords` | Per-minute accelerometer X/Y/Z for Parkinson analysis |
| `GnssTrackRecords` | WGS-84 GPS track-points from OldMan (OM0) devices |

---

## Adding an EF Core migration

```bash
cd src/SmartWatch4G.Infrastructure
dotnet ef migrations add <Name> \
    --startup-project ../SmartWatch4G.Api \
    --output-dir Persistence/Migrations
```

---

## Configuration Reference

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=smartwatch4g.db"
  },
  "WownAlgo": {
    "BaseUrl": "https://api1.iwown.com/algoservice",
    "Account": "your_account",
    "Password": "your_password"
  },
  "WownCommand": {
    "BaseUrl": "https://search.iwown.com",
    "Account": "your_account",
    "Password": "your_password"
  }
}
```
