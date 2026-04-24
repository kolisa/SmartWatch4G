# SmartWatch4G API Reference

> **Base URL**: `http://<host>:<port>`  
> **Content-Type**: `application/json` (unless noted otherwise)  
> **Swagger UI**: available at `/` (root) when the API is running

---

## Table of Contents

1. [Common Conventions](#common-conventions)
2. [Fleet](#fleet)
3. [Devices](#devices)
4. [Companies](#companies)
5. [Users](#users)
6. [GPS](#gps)
7. [Health](#health)
8. [Alerts](#alerts)
9. [Device Config](#device-config)
10. [Sleep](#sleep)
11. [Device Upload Endpoints](#device-upload-endpoints)

---

## Common Conventions

### Pagination

All paginated endpoints accept `page` (1-based) and `pageSize` query parameters and return a common envelope:

```json
{
  "Items": [ ... ],
  "TotalCount": 120,
  "Page": 1,
  "PageSize": 20,
  "TotalPages": 6
}
```

| Field | Type | Description |
|-------|------|-------------|
| `Items` | array | The records for the current page |
| `TotalCount` | int | Total number of records across all pages |
| `Page` | int | The current page number (1-based) |
| `PageSize` | int | The number of items requested per page |
| `TotalPages` | int | Computed: `ceil(TotalCount / PageSize)` |

### Date and time formats

| Context | Format | Example |
|---------|--------|---------|
| Device-reported timestamps (`RecordTime`, `GnssTime`, `AlarmTime`) | `YYYY-MM-DD HH:mm:ss` (local time as received from device) | `"2026-04-24 08:30:00"` |
| Server-assigned audit timestamps (`RecordedAt`, `CreatedAt`, `UpdatedAt`) | ISO 8601 UTC | `"2026-04-24T08:30:00Z"` |
| Query parameters (`from`, `to`) | ISO 8601 — any timezone offset accepted | `"2026-04-24T00:00:00Z"` |

### Error responses

| Status | Body | When |
|--------|------|------|
| `400` | `{ "message": "..." }` or model-state validation object | Missing/invalid input |
| `404` | `{ "message": "..." }` | Resource not found |
| `409` | `{ "message": "..." }` | Unique constraint violation (e.g. duplicate device) |
| `500` | `{ "message": "..." }` | Unexpected server-side error |

### DeviceStatus values

The `DeviceStatus` string field appears on device and telemetry responses. Possible values:

| Value | Meaning |
|-------|---------|
| `"online"` | Device was seen reporting data within the polling window |
| `"offline"` | Device has not reported within the polling window |
| `"unknown"` | No status record exists for this device yet |

---

## Fleet

### GET `/fleet/summary`

Returns aggregate counts over the last 24 hours. Useful as the primary data source for a live-monitoring dashboard header. Optionally filter by company.

**Query parameters**

| Name | Type | Default | Description |
|------|------|---------|-------------|
| `companyId` | `int` | — | Optional. When provided only devices linked to this company are counted |

**Response `200`**

```json
{
  "TotalWorkers": 45,
  "ActiveAlerts": 3,
  "SosCount": 1,
  "WorkersInDistress": 2
}
```

**Response fields**

| Field | Type | Description |
|-------|------|-------------|
| `TotalWorkers` | int | Total number of registered (active) users/devices |
| `ActiveAlerts` | int | Alarms raised in the last 24 hours |
| `SosCount` | int | SOS events raised in the last 24 hours |
| `WorkersInDistress` | int | Workers with at least one unresolved critical alert |

---

## Devices

### GET `/devices`

Paginated list of registered devices with their latest health snapshot and GPS coordinates. Each item is the combined user profile + most-recent sensor readings, making it ideal for a fleet overview table.

**Query parameters**

| Name | Type | Default | Description |
|------|------|---------|-------------|
| `page` | `int` | `1` | Page number (1-based) |
| `pageSize` | `int` | `10` | Items per page (max 100) |
| `companyId` | `int` | — | Optional. Filter to devices belonging to this company |

**Response `200`** — `PagedResult<UserProfileSummaryResponse>`

```json
{
  "Items": [
    {
      "DeviceId": "863719060012345",
      "Name": "Jane",
      "Surname": "Doe",
      "EmpNo": "EMP001",
      "LatestLatitude": -33.918861,
      "LatestLongitude": 18.423300,
      "LatestGnssTime": "2026-04-24 08:30:00",
      "SpO2": 98,
      "Steps": 4200,
      "HeartRate": 72,
      "Fatigue": 75,
      "Battery": 85,
      "Sbp": 118,
      "Dbp": 76,
      "HealthRecordedAt": "2026-04-24T08:30:00Z"
    }
  ],
  "TotalCount": 45,
  "Page": 1,
  "PageSize": 10,
  "TotalPages": 5
}
```

**Item fields**

| Field | Type | Unit / Range | Description |
|-------|------|-------------|-------------|
| `DeviceId` | string | — | Device IMEI (up to 15 digits) |
| `Name` | string | — | Worker's first name |
| `Surname` | string | — | Worker's last name |
| `EmpNo` | string? | — | Employee number; `null` if not set |
| `LatestLatitude` | double? | degrees | Last known latitude; `null` if no GPS fix recorded |
| `LatestLongitude` | double? | degrees | Last known longitude; `null` if no GPS fix recorded |
| `LatestGnssTime` | string? | `YYYY-MM-DD HH:mm:ss` | Timestamp of the last GPS fix as reported by the device |
| `SpO2` | int? | 0–100 % | Blood oxygen saturation from the most recent health record |
| `Steps` | int? | — | Cumulative step count for the day |
| `HeartRate` | int? | bpm | Average heart rate from the most recent health record |
| `Fatigue` | int? | score | HRV-derived fatigue score (derived from RMSSD). Higher = more fatigued |
| `Battery` | int? | 0–100 % | Battery level reported by the device |
| `Sbp` | int? | mmHg | Systolic blood pressure |
| `Dbp` | int? | mmHg | Diastolic blood pressure |
| `HealthRecordedAt` | datetime? | ISO 8601 UTC | Server timestamp when the health record was saved |

---

### GET `/devices/{deviceId}`

Returns the full profile and latest sensor data for a single device. Extends the summary response with contact details, heart-rate extremes, distance, and calorie burn.

**Path parameters**

| Name | Type | Description |
|------|------|-------------|
| `deviceId` | `string` | Device IMEI |

**Response `200`** — `UserProfileDetailResponse`

```json
{
  "DeviceId": "863719060012345",
  "Name": "Jane",
  "Surname": "Doe",
  "EmpNo": "EMP001",
  "Email": "jane.doe@example.com",
  "Cell": "+27821234567",
  "Address": "123 Main Street",
  "DeviceStatus": "online",
  "LatestLatitude": -33.918861,
  "LatestLongitude": 18.423300,
  "LatestGnssTime": "2026-04-24 08:30:00",
  "SpO2": 98,
  "Steps": 4200,
  "HeartRate": 72,
  "MaxHeartRate": 110,
  "MinHeartRate": 58,
  "Fatigue": 75,
  "Battery": 85,
  "Sbp": 118,
  "Dbp": 76,
  "Distance": 3.2,
  "Calorie": 210.5,
  "HealthRecordedAt": "2026-04-24T08:30:00Z"
}
```

**Additional fields** _(fields shared with the summary are described above)_

| Field | Type | Unit / Range | Description |
|-------|------|-------------|-------------|
| `Email` | string? | — | Worker's email address |
| `Cell` | string? | — | Worker's mobile number |
| `Address` | string? | — | Worker's physical address |
| `DeviceStatus` | string | enum | See [DeviceStatus values](#devicestatus-values) |
| `MaxHeartRate` | int? | bpm | Peak heart rate in the most recent health record period |
| `MinHeartRate` | int? | bpm | Lowest heart rate in the most recent health record period |
| `Distance` | double? | km | Distance covered during the measurement period |
| `Calorie` | double? | kcal | Calories burned during the measurement period |

**Error responses**: `400`, `404`, `500`

---

### GET `/devices/telemetry`

Returns the latest operational telemetry for **all** registered devices in a single call. Each entry contains only the operational subset of fields — no contact info. Useful for populating live-monitoring dashboards.

> **Note**: This endpoint returns all matching devices as an array (not paginated). For large fleets, use `companyId` to reduce payload size.

**Query parameters**

| Name | Type | Default | Description |
|------|------|---------|-------------|
| `companyId` | `int` | — | Optional. Filter to devices belonging to this company |

**Response `200`** — `DeviceTelemetryResponse[]`

```json
[
  {
    "DeviceId": "863719060012345",
    "DeviceStatus": "online",
    "Battery": 85,
    "HeartRate": 72,
    "SpO2": 98,
    "Fatigue": 75,
    "Sbp": 118,
    "Dbp": 76,
    "Steps": 4200,
    "Latitude": -33.918861,
    "Longitude": 18.423300,
    "GnssTime": "2026-04-24 08:30:00",
    "HealthRecordedAt": "2026-04-24T08:30:00Z"
  }
]
```

**Response fields**

| Field | Type | Unit / Range | Description |
|-------|------|-------------|-------------|
| `DeviceId` | string | — | Device IMEI |
| `DeviceStatus` | string | enum | See [DeviceStatus values](#devicestatus-values) |
| `Battery` | int? | 0–100 % | Battery level |
| `HeartRate` | int? | bpm | Average heart rate |
| `SpO2` | int? | 0–100 % | Blood oxygen saturation |
| `Fatigue` | int? | score | HRV-derived fatigue score |
| `Sbp` | int? | mmHg | Systolic blood pressure |
| `Dbp` | int? | mmHg | Diastolic blood pressure |
| `Steps` | int? | — | Step count |
| `Latitude` | double? | degrees | Last known latitude |
| `Longitude` | double? | degrees | Last known longitude |
| `GnssTime` | string? | `YYYY-MM-DD HH:mm:ss` | Timestamp of last GPS fix as reported by the device |
| `HealthRecordedAt` | datetime? | ISO 8601 UTC | Server timestamp of the health record |

---

### GET `/devices/{deviceId}/telemetry`

Returns the latest operational telemetry for a **single** device. Same response shape as one element from `GET /devices/telemetry`. Intended for high-frequency polling.

**Path parameters**

| Name | Type | Description |
|------|------|-------------|
| `deviceId` | `string` | Device IMEI |

**Response `200`** — `DeviceTelemetryResponse` (same fields as table above)

**Error responses**: `400`, `404`, `500`

---

## Companies

### POST `/companies`

Creates a new company.

**Request body**

```json
{
  "Name": "Acme Corp",
  "Description": "Optional description"
}
```

**Response `201`** — company object with assigned `Id`

---

### GET `/companies`

Returns all active companies.

**Response `200`** — array of company objects

---

### GET `/companies/{id}`

Returns a company by ID.

**Path parameters**

| Name | Type | Description |
|------|------|-------------|
| `id` | `int` | Company ID |

**Response `200`** — company object  
**Error responses**: `404`, `500`

---

### PUT `/companies/{id}`

Updates company details.

**Path parameters**

| Name | Type | Description |
|------|------|-------------|
| `id` | `int` | Company ID |

**Request body** — same shape as `POST /companies`

**Response `200`** — updated company object  
**Error responses**: `400`, `404`, `500`

---

### DELETE `/companies/{id}`

Soft-deletes a company. Users linked to this company will have their `company_id` set to `NULL`.

**Path parameters**

| Name | Type | Description |
|------|------|-------------|
| `id` | `int` | Company ID |

**Response `204 No Content`**  
**Error responses**: `404`, `500`

---

### GET `/companies/{id}/users`

Returns all active users belonging to this company.

**Path parameters**

| Name | Type | Description |
|------|------|-------------|
| `id` | `int` | Company ID |

**Response `200`** — `UserResponse[]`  
**Error responses**: `404`, `500`

---

## Users

### POST `/users`

Creates a new user linked to a device.

**Request body**

```json
{
  "DeviceId": "863719060012345",
  "Name": "Jane",
  "Surname": "Doe",
  "Email": "jane.doe@example.com",
  "Cell": "+27821234567",
  "EmpNo": "EMP001",
  "Address": "123 Main Street",
  "CompanyId": 1
}
```

| Field | Required | Constraints |
|-------|----------|-------------|
| `DeviceId` | Yes | max 50 chars |
| `Name` | Yes | max 100 chars |
| `Surname` | Yes | max 100 chars |
| `Email` | No | valid email, max 200 chars |
| `Cell` | No | max 30 chars |
| `EmpNo` | No | max 50 chars |
| `Address` | No | max 500 chars |
| `CompanyId` | No | links user to a company |

**Response `201`** — `UserResponse`

```json
{
  "DeviceId": "863719060012345",
  "UserId": 42,
  "Name": "Jane",
  "Surname": "Doe",
  "Email": "jane.doe@example.com",
  "Cell": "+27821234567",
  "EmpNo": "EMP001",
  "Address": "123 Main Street",
  "CompanyId": 1,
  "CompanyName": "Acme Corp",
  "UpdatedAt": "2026-04-24T08:00:00Z"
}
```

**Error responses**: `400`, `409` (duplicate device), `500`

---

### GET `/users`

Returns all active users. Optionally filter by company.

**Query parameters**

| Name | Type | Default | Description |
|------|------|---------|-------------|
| `companyId` | `int` | — | Optional company filter |

**Response `200`** — `UserResponse[]`

---

### GET `/users/{deviceId}`

Returns a single user by device ID.

**Path parameters**

| Name | Type | Description |
|------|------|-------------|
| `deviceId` | `string` | Device IMEI |

**Response `200`** — `UserResponse`  
**Error responses**: `400`, `404`, `500`

---

### PUT `/users/{deviceId}`

Updates an existing user's details.

**Path parameters**

| Name | Type | Description |
|------|------|-------------|
| `deviceId` | `string` | Device IMEI |

**Request body**

```json
{
  "Name": "Jane",
  "Surname": "Smith",
  "Email": "jane.smith@example.com",
  "Cell": "+27821234567",
  "EmpNo": "EMP001",
  "Address": "456 New Street"
}
```

**Response `200`** — updated `UserResponse`  
**Error responses**: `400`, `404`, `500`

---

### DELETE `/users/{deviceId}`

Deactivates a user (soft delete).

**Path parameters**

| Name | Type | Description |
|------|------|-------------|
| `deviceId` | `string` | Device IMEI |

**Response `204 No Content`**  
**Error responses**: `400`, `404`, `500`

---

### PUT `/users/{deviceId}/company`

Links or unlinks a user to a company. Set `companyId` to `null` to remove the association. All historical data rows for this device are automatically updated.

**Request body**

```json
{ "CompanyId": 1 }
```

Set `CompanyId` to `null` to unlink.

**Response `200`** — updated `UserResponse`  
**Error responses**: `400`, `404`, `500`

---

### POST `/users/{deviceId}/backfill`

Backfills `user_id` and `company_id` on all historical data rows (across 29 tables) for this device. Call this after linking a user to a device or reassigning a device to a different user.

**Response `200`**

```json
{ "RowsUpdated": 1420 }
```

**Error responses**: `400`, `404`, `500`

---

## GPS

### GET `/companies/{companyId}/gps`

All GPS tracks for a company, paginated. Includes online/offline device counts.

**Path parameters**

| Name | Type | Description |
|------|------|-------------|
| `companyId` | `int` | Company ID |

**Query parameters**

| Name | Type | Default | Description |
|------|------|---------|-------------|
| `page` | `int` | `1` | Page number (min 1) |
| `pageSize` | `int` | `50` | Items per page (max 500) |
| `from` | `datetime` | — | Start of time range (ISO 8601) |
| `to` | `datetime` | — | End of time range (ISO 8601) |
| `sortBy` | `string` | `"time"` | `"time"` or `"device"` |
| `sortDir` | `string` | `"desc"` | `"asc"` or `"desc"` |

**Response `200`** — `GpsPagedResult`

```json
{
  "Items": [
    {
      "DeviceId": "863719060012345",
      "UserName": "Jane Doe",
      "GnssTime": "2026-04-24 08:30:00",
      "Latitude": -33.918861,
      "Longitude": 18.423300,
      "LocType": "GPS",
      "RecordedAt": "2026-04-24T08:30:00Z"
    }
  ],
  "TotalCount": 1200,
  "Page": 1,
  "PageSize": 50,
  "TotalPages": 24,
  "OnlineCount": 30,
  "OfflineCount": 15
}
```

**Item fields**

| Field | Type | Description |
|-------|------|-------------|
| `DeviceId` | string | Device IMEI |
| `UserName` | string? | Full name of the linked worker; `null` if no user registered |
| `GnssTime` | string | Timestamp of the fix as reported by the device (`YYYY-MM-DD HH:mm:ss`) |
| `Latitude` | double | Latitude in decimal degrees (negative = south) |
| `Longitude` | double | Longitude in decimal degrees (negative = west) |
| `LocType` | string? | Positioning method reported by the device (e.g. `"GPS"`, `"LBS"`, `"WIFI"`); `null` if not reported |
| `RecordedAt` | datetime | ISO 8601 UTC timestamp when the server persisted this record |

**Envelope-level fields**

| Field | Type | Description |
|-------|------|-------------|
| `OnlineCount` | int | Number of distinct online devices with tracks in the result set |
| `OfflineCount` | int | Number of distinct offline devices with tracks in the result set |

---

### GET `/companies/{companyId}/gps/online`

GPS tracks for **online** devices in a company only. Same query parameters and response shape as above (without `OfflineCount`).

---

### GET `/companies/{companyId}/gps/offline`

GPS tracks for **offline** devices in a company only. Same query parameters and response shape as above.

---

### GET `/devices/{deviceId}/gps`

GPS track history for a single device.

**Path parameters**

| Name | Type | Description |
|------|------|-------------|
| `deviceId` | `string` | Device IMEI |

**Query parameters** — same as company GPS (`page`, `pageSize`, `from`, `to`, `sortBy`, `sortDir`)

**Response `200`** — `GpsPagedResult`  
**Error responses**: `400`, `404`, `500`

---

## Health

### GET `/companies/{companyId}/health`

Paged health records for all devices in a company with optional date filters.

**Path parameters**

| Name | Type | Description |
|------|------|-------------|
| `companyId` | `int` | Company ID |

**Query parameters**

| Name | Type | Default | Description |
|------|------|---------|-------------|
| `page` | `int` | `1` | Page number (min 1) |
| `pageSize` | `int` | `20` | Items per page (max 200) |
| `from` | `datetime` | — | Start of time range (ISO 8601) |
| `to` | `datetime` | — | End of time range (ISO 8601) |
| `sortBy` | `string` | `"time"` | `"time"` or `"device"` |
| `sortDir` | `string` | `"desc"` | `"asc"` or `"desc"` |

**Response `200`** — `HealthPagedResult`

```json
{
  "Items": [
    {
      "DeviceId": "863719060012345",
      "UserName": "Jane Doe",
      "RecordTime": "2026-04-24 08:30:00",
      "Battery": 85,
      "Steps": 4200,
      "Distance": 3.2,
      "Calorie": 210.5,
      "HeartRate": 72,
      "MaxHr": 110,
      "MinHr": 58,
      "SpO2": 98,
      "Sbp": 118,
      "Dbp": 76,
      "Fatigue": 75,
      "RecordedAt": "2026-04-24T08:30:00Z"
    }
  ],
  "TotalCount": 860,
  "Page": 1,
  "PageSize": 20,
  "TotalPages": 43
}
```

**Item fields**

| Field | Type | Unit / Range | Description |
|-------|------|-------------|-------------|
| `DeviceId` | string | — | Device IMEI |
| `UserName` | string? | — | Full name of the linked worker; `null` if no user registered |
| `RecordTime` | string | `YYYY-MM-DD HH:mm:ss` | Measurement timestamp as reported by the device |
| `Battery` | int? | 0–100 % | Battery level at time of record |
| `Steps` | int? | — | Step count accumulated up to this measurement |
| `Distance` | double? | km | Distance covered up to this measurement |
| `Calorie` | double? | kcal | Calories burned up to this measurement |
| `HeartRate` | int? | bpm | Average heart rate for the measurement interval |
| `MaxHr` | int? | bpm | Peak heart rate for the interval |
| `MinHr` | int? | bpm | Lowest heart rate for the interval |
| `SpO2` | int? | 0–100 % | Average blood oxygen saturation |
| `Sbp` | int? | mmHg | Systolic blood pressure |
| `Dbp` | int? | mmHg | Diastolic blood pressure |
| `Fatigue` | int? | score | HRV-derived fatigue score (higher = more fatigued) |
| `RecordedAt` | datetime | ISO 8601 UTC | Server timestamp when the record was persisted |

---

### GET `/companies/{companyId}/health/summary`

Per-device health aggregates (averages, totals) for a company over an optional time window.

**Path parameters**

| Name | Type | Description |
|------|------|-------------|
| `companyId` | `int` | Company ID |

**Query parameters**

| Name | Type | Default | Description |
|------|------|---------|-------------|
| `from` | `datetime` | — | Start of aggregation window |
| `to` | `datetime` | — | End of aggregation window |

**Response `200`** — `HealthSummaryResponse[]`

```json
[
  {
    "DeviceId": "863719060012345",
    "UserName": "Jane Doe",
    "AvgHeartRate": 73.4,
    "AvgSpO2": 97.8,
    "AvgFatigue": 62.5,
    "MaxHr": 125,
    "MinHr": 55,
    "TotalSteps": 12400,
    "RecordCount": 48
  }
]
```

**Response fields**

| Field | Type | Unit / Range | Description |
|-------|------|-------------|-------------|
| `DeviceId` | string | — | Device IMEI |
| `UserName` | string? | — | Full name of the linked worker |
| `AvgHeartRate` | double? | bpm | Mean heart rate across all records in the window |
| `AvgSpO2` | double? | 0–100 % | Mean blood oxygen saturation |
| `AvgFatigue` | double? | score | Mean HRV-derived fatigue score |
| `MaxHr` | int? | bpm | Highest single heart-rate reading in the window |
| `MinHr` | int? | bpm | Lowest single heart-rate reading in the window |
| `TotalSteps` | int? | — | Sum of step counts across all records |
| `RecordCount` | int | — | Number of health records that contributed to these aggregates |

---

### GET `/devices/{deviceId}/health`

Paged health records for a single device with optional date filters.

**Path parameters**

| Name | Type | Description |
|------|------|-------------|
| `deviceId` | `string` | Device IMEI |

**Query parameters** — same as company health

**Response `200`** — `HealthPagedResult`  
**Error responses**: `400`, `404`, `500`

---

### GET `/devices/{deviceId}/health/latest`

Latest single health snapshot for a device.

**Path parameters**

| Name | Type | Description |
|------|------|-------------|
| `deviceId` | `string` | Device IMEI |

**Response `200`** — `HealthRecordResponse` (single object, same fields as items above)  
**Error responses**: `400`, `404`, `500`

---

## Alerts

### GET `/alerts`

Returns recent device alerts enriched with the linked user's name.

**Query parameters**

| Name | Type | Default | Constraints | Description |
|------|------|---------|-------------|-------------|
| `withinHours` | `int` | `24` | max 720 | Look-back window in hours |
| `limit` | `int` | `50` | max 500 | Maximum records to return |

**Response `200`** — `AlertSummaryResponse[]`

```json
[
  {
    "Id": 101,
    "DeviceId": "863719060012345",
    "WorkerName": "Jane Doe",
    "AlarmTime": "2026-04-24 07:45:00",
    "AlarmType": "sos",
    "Details": null,
    "CreatedAt": "2026-04-24T07:45:12Z"
  }
]
```

**Response fields**

| Field | Type | Description |
|-------|------|-------------|
| `Id` | int | Surrogate key of the alarm record |
| `DeviceId` | string | Device IMEI that raised the alarm |
| `WorkerName` | string? | Full name of the linked worker (JOIN enrichment); `null` if no user registered |
| `AlarmTime` | string | Timestamp of the alarm event as reported by the device (`YYYY-MM-DD HH:mm:ss`) |
| `AlarmType` | string | Type code — see table below |
| `Details` | string? | Optional detail string (e.g. battery level for low-power alarms); `null` for most alarm types |
| `CreatedAt` | datetime | ISO 8601 UTC timestamp when the server persisted this record |

**`AlarmType` values**

Alarm types are set by the server when it parses device log files or protobuf upload frames.

| Value | Trigger |
|-------|---------|
| `sos` | Worker pressed the SOS button on the watch |
| `low_power` | Battery dropped below the low-power threshold; `Details` contains `"battery:<level>"` |
| `not_wear` | Device detected it is not being worn |

> Additional alarm types may be produced by the protobuf upload pipeline (HR out-of-range, SpO2 drop, blood-pressure alarm, fall detection, temperature alarm). These will surface with descriptive type codes in future versions.

---

## Device Config

### GET `/companies/{companyId}/devices/config`

Command configurations for all active devices in a company, paginated.

**Path parameters**

| Name | Type | Description |
|------|------|-------------|
| `companyId` | `int` | Company ID |

**Query parameters**

| Name | Type | Default | Description |
|------|------|---------|-------------|
| `page` | `int` | `1` | Page number |
| `pageSize` | `int` | `50` | Items per page |

**Response `200`** — `PagedResult<DeviceConfigResponse>`

---

### GET `/devices/{deviceId}/config`

Full command configuration for a single device including GPS, HR/SpO2/BP alarms, sleep schedule, display settings, and more.

**Path parameters**

| Name | Type | Description |
|------|------|-------------|
| `deviceId` | `string` | Device IMEI |

**Response `200`** — `DeviceConfigResponse`

```json
{
  "DeviceId": "863719060012345",
  "UserName": "Jane Doe",
  "GpsAutoCheck": true,
  "GpsIntervalTime": 60,
  "PowerMode": 0,
  "DataAutoUpload": true,
  "DataUploadInterval": 300,
  "HrAlarmOpen": true,
  "HrAlarmHigh": 160,
  "HrAlarmLow": 45,
  "HrAlarmThreshold": 10,
  "HrAlarmInterval": 5,
  "DynHrAlarmOpen": false,
  "Spo2AlarmOpen": true,
  "Spo2AlarmLow": 90,
  "BpAlarmOpen": false,
  "BpSbpHigh": 160,
  "BpSbpBelow": 90,
  "BpDbpHigh": 100,
  "BpDbpBelow": 60,
  "TempAlarmOpen": false,
  "TempAlarmHigh": 38.5,
  "TempAlarmLow": 35.0,
  "FallCheckEnabled": true,
  "Language": "en",
  "HourFormat": 24,
  "DateFormat": "MM/DD/YYYY",
  "DistanceUnit": 0,
  "TemperatureUnit": 0,
  "HrInterval": 5,
  "OtherInterval": 10,
  "GoalStep": 10000,
  "GoalDistance": 5.0,
  "GoalCalorie": 400.0,
  "LcdGestureOpen": true,
  "LcdGestureStartHour": 7,
  "LcdGestureEndHour": 22
}
```

**Key fields**

| Field | Type | Unit / Values | Description |
|-------|------|--------------|-------------|
| `DeviceId` | string | — | Device IMEI |
| `UserName` | string? | — | Full name of the linked worker |
| `GpsAutoCheck` | bool? | — | Whether automatic periodic GPS polling is enabled |
| `GpsIntervalTime` | int? | seconds | Interval between automatic GPS fixes |
| `PowerMode` | int? | `0` normal, `1` power-saving | Power/sampling mode sent to the device |
| `DataAutoUpload` | bool? | — | Whether the device auto-uploads health data |
| `DataUploadInterval` | int? | seconds | Interval between automatic data uploads |
| `HrAlarmOpen` | bool? | — | Whether the static heart-rate alarm is enabled |
| `HrAlarmHigh` | int? | bpm | Upper HR threshold that triggers an alarm |
| `HrAlarmLow` | int? | bpm | Lower HR threshold that triggers an alarm |
| `HrAlarmThreshold` | int? | bpm | Number of consecutive out-of-range readings before alarm fires |
| `HrAlarmInterval` | int? | minutes | Minimum interval between consecutive HR alarms |
| `DynHrAlarmOpen` | bool? | — | Whether the dynamic (activity-aware) HR alarm is enabled |
| `Spo2AlarmOpen` | bool? | — | Whether the SpO2 alarm is enabled |
| `Spo2AlarmLow` | int? | 0–100 % | SpO2 percentage below which an alarm is triggered |
| `BpAlarmOpen` | bool? | — | Whether the blood-pressure alarm is enabled |
| `BpSbpHigh` | int? | mmHg | Systolic threshold above which an alarm fires |
| `BpSbpBelow` | int? | mmHg | Systolic threshold below which an alarm fires |
| `BpDbpHigh` | int? | mmHg | Diastolic threshold above which an alarm fires |
| `BpDbpBelow` | int? | mmHg | Diastolic threshold below which an alarm fires |
| `TempAlarmOpen` | bool? | — | Whether the temperature alarm is enabled |
| `TempAlarmHigh` | double? | °C | Temperature above which an alarm fires |
| `TempAlarmLow` | double? | °C | Temperature below which an alarm fires |
| `FallCheckEnabled` | bool? | — | Whether fall-detection is enabled |
| `HourFormat` | int? | `12` or `24` | Clock format displayed on the watch |
| `DistanceUnit` | int? | `0` = km, `1` = miles | Distance unit shown on the watch |
| `TemperatureUnit` | int? | `0` = °C, `1` = °F | Temperature unit shown on the watch |
| `HrInterval` | int? | minutes | Automatic heart-rate measurement interval |
| `OtherInterval` | int? | minutes | Automatic measurement interval for SpO2/BP/fatigue |
| `GoalStep` | int? | steps/day | Daily step goal configured on the device |
| `GoalDistance` | double? | km/day | Daily distance goal |
| `GoalCalorie` | double? | kcal/day | Daily calorie burn goal |
| `LcdGestureOpen` | bool? | — | Whether raise-to-wake gesture is enabled |
| `LcdGestureStartHour` | int? | 0–23 | Hour from which raise-to-wake is active |
| `LcdGestureEndHour` | int? | 0–23 | Hour at which raise-to-wake deactivates |

> All fields are `null` when the device has not yet synced that configuration category with the server.

**Error responses**: `400`, `404`, `500`

---

## Sleep

### GET `/health/sleep`

Returns a sleep analysis result for a device on a given date.

**Query parameters**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `deviceid` | `string` | Yes | Device IMEI |
| `sleep_date` | `string` | Yes | Date in `YYYY-MM-DD` format |

**Response `200`**

```json
{
  "ReturnCode": 0,
  "Data": {
    "deviceid": "863719060012345",
    "sleep_date": "2026-04-24",
    "start_time": "2026-04-23 23:15:00",
    "end_time": "2026-04-24 07:00:00",
    "deep_sleep": 85,
    "light_sleep": 300,
    "weak_sleep": 30,
    "eyemove_sleep": 50,
    "score": 80,
    "osahs_risk": 0,
    "spo2_score": 0,
    "sleep_hr": 60
  }
}
```

**`ReturnCode` values**

| Code | Meaning |
|------|---------|
| `0` | Success |
| `10002` | Invalid request — `deviceid` was missing or `sleep_date` was not a valid `YYYY-MM-DD` date |

**`Data` fields**

| Field | Type | Unit | Description |
|-------|------|------|-------------|
| `deviceid` | string | — | The queried device IMEI |
| `sleep_date` | string | `YYYY-MM-DD` | The date for which sleep data was requested |
| `start_time` | string | `YYYY-MM-DD HH:mm:ss` | Detected sleep start time (typically the evening before `sleep_date`) |
| `end_time` | string | `YYYY-MM-DD HH:mm:ss` | Detected wake-up time |
| `deep_sleep` | int | minutes | Duration of deep-sleep stage |
| `light_sleep` | int | minutes | Duration of light-sleep stage |
| `weak_sleep` | int | minutes | Duration of shallow/weak-sleep stage |
| `eyemove_sleep` | int | minutes | Duration of REM (rapid eye movement) stage |
| `score` | int | 0–100 | Overall sleep quality score (100 = best) |
| `osahs_risk` | int | 0–1 | Obstructive sleep apnoea risk flag (`0` = low, `1` = elevated) |
| `spo2_score` | int | — | Overnight SpO2 quality score (0 = not evaluated) |
| `sleep_hr` | int | bpm | Average heart rate during the sleep session |

---

## Device Upload Endpoints

These endpoints receive binary payloads pushed directly by the watch firmware. They are not intended for REST clients.

### POST `/alarm/upload`

Receives a binary protobuf-encoded alarm payload from the device. The first 15 bytes are the device ID (UTF-8), followed by one or more `DT`-prefixed frames.

**Content-Type**: `application/octet-stream` (raw body)

**Binary frame structure**

```
Bytes 0–14   : Device ID (15 bytes, UTF-8, zero-padded)
Bytes 15+    : One or more frames, each:
  [0–1]  Prefix   = 0x44 0x54 ('DT')
  [2–3]  Length   = little-endian uint16 — byte count of the protobuf payload
  [4–5]  CRC      = little-endian uint16
  [6–7]  Opt      = little-endian uint16 — frame type (0x12 = alarm v2)
  [8 …]  Payload  = protobuf bytes (length bytes)
```

**Response byte codes**

| Byte | Meaning |
|------|---------|
| `0x00` | Success — all frames processed |
| `0x01` | Failed to read the request body |
| `0x02` | Payload is too short / a frame's declared length exceeds remaining bytes |
| `0x03` | Invalid frame header (expected `0x44 0x54`) |

---

### POST `/status/notify`

Receives a JSON device-status notification pushed by the device or gateway. The raw payload is also persisted to the file store for audit purposes.

**Request body**

```json
{
  "DeviceId": "863719060012345",
  "Status": "online"
}
```

**Request fields**

| Field | Type | Description |
|-------|------|-------------|
| `DeviceId` | string | Device IMEI |
| `Status` | string | Current device status reported by the gateway |

**Response `200`**

```json
{ "ReturnCode": 0 }
```

**`ReturnCode` values**

| Code | Meaning |
|------|---------|
| `0` | Notification accepted and persisted |
| `10002` | Body was empty, not valid JSON, or `DeviceId` could not be determined |
