-- Seed default settings for 13 devices across all device_* settings tables.
-- Safe to re-run: uses INSERT WHERE NOT EXISTS so existing rows are never overwritten.
-- user_id is resolved from user_profiles; company_id = 1.

DECLARE @devices TABLE (device_id NVARCHAR(50));
INSERT INTO @devices VALUES
    ('863758060986873'),  -- Kethotse   Watch 19
    ('863758060926292'),  -- Christina  Watch 7
    ('863758060956587'),  -- Mpolokeng  Watch 8
    ('863758060926754'),  -- Patsiua    Watch 5
    ('863758060987517'),  -- Stephen    Watch 6
    ('863758060987855'),  -- Lebo       Watch 13
    ('863758060927422'),  -- Karabo     Watch 20
    ('863758060926499'),  -- Maomoji    Watch 9
    ('863758060927455'),  -- Solomon    Watch 10
    ('863758060982484'),  -- Tebogo     Watch 18
    ('863758060926564'),  -- Brayen     Watch 14
    ('863758060987483'),  -- Poloko     Watch 15
    ('863758060927232');  -- Gomolemo   Watch 16

-- NOTE: device_data_freq and device_gps_settings are intentionally NOT seeded here.
-- Those rows are only created by DeviceProvisioningService after the Iwown API
-- confirms success (ReturnCode == 0).  Seeding them would cause the provisioning
-- job to skip the API calls entirely, meaning the physical watches would never
-- receive their settings.

-- ── device_locate_freq ────────────────────────────────────────────────────────
INSERT INTO device_locate_freq (device_id, data_auto_upload, data_upload_interval, auto_locate, locate_interval_time, power_mode, user_id, company_id)
SELECT d.device_id, 1, 300, 1, 60, 2, u.user_id, 1
FROM @devices d
LEFT JOIN user_profiles u ON u.device_id = d.device_id
WHERE NOT EXISTS (SELECT 1 FROM device_locate_freq t WHERE t.device_id = d.device_id);

-- ── device_hr_alarm ───────────────────────────────────────────────────────────
INSERT INTO device_hr_alarm (device_id, open, high, low, threshold, alarm_interval, user_id, company_id)
SELECT d.device_id, 1, 160, 45, 5, 5, u.user_id, 1
FROM @devices d
LEFT JOIN user_profiles u ON u.device_id = d.device_id
WHERE NOT EXISTS (SELECT 1 FROM device_hr_alarm t WHERE t.device_id = d.device_id);

-- ── device_dynamic_hr_alarm ───────────────────────────────────────────────────
INSERT INTO device_dynamic_hr_alarm (device_id, open, high, low, timeout, interval, user_id, company_id)
SELECT d.device_id, 0, 160, 45, 30, 5, u.user_id, 1
FROM @devices d
LEFT JOIN user_profiles u ON u.device_id = d.device_id
WHERE NOT EXISTS (SELECT 1 FROM device_dynamic_hr_alarm t WHERE t.device_id = d.device_id);

-- ── device_spo2_alarm ─────────────────────────────────────────────────────────
INSERT INTO device_spo2_alarm (device_id, open, low, user_id, company_id)
SELECT d.device_id, 1, 90, u.user_id, 1
FROM @devices d
LEFT JOIN user_profiles u ON u.device_id = d.device_id
WHERE NOT EXISTS (SELECT 1 FROM device_spo2_alarm t WHERE t.device_id = d.device_id);

-- ── device_bp_alarm ───────────────────────────────────────────────────────────
INSERT INTO device_bp_alarm (device_id, open, sbp_high, sbp_below, dbp_high, dbp_below, user_id, company_id)
SELECT d.device_id, 0, 160, 90, 100, 60, u.user_id, 1
FROM @devices d
LEFT JOIN user_profiles u ON u.device_id = d.device_id
WHERE NOT EXISTS (SELECT 1 FROM device_bp_alarm t WHERE t.device_id = d.device_id);

-- ── device_temp_alarm ─────────────────────────────────────────────────────────
INSERT INTO device_temp_alarm (device_id, open, high, low, user_id, company_id)
SELECT d.device_id, 0, 39, 35, u.user_id, 1
FROM @devices d
LEFT JOIN user_profiles u ON u.device_id = d.device_id
WHERE NOT EXISTS (SELECT 1 FROM device_temp_alarm t WHERE t.device_id = d.device_id);

-- ── device_fall_settings ──────────────────────────────────────────────────────
INSERT INTO device_fall_settings (device_id, fall_check, fall_threshold, user_id, company_id)
SELECT d.device_id, 1, 3, u.user_id, 1
FROM @devices d
LEFT JOIN user_profiles u ON u.device_id = d.device_id
WHERE NOT EXISTS (SELECT 1 FROM device_fall_settings t WHERE t.device_id = d.device_id);

-- ── device_hr_interval ────────────────────────────────────────────────────────
INSERT INTO device_hr_interval (device_id, interval, user_id, company_id)
SELECT d.device_id, 5, u.user_id, 1
FROM @devices d
LEFT JOIN user_profiles u ON u.device_id = d.device_id
WHERE NOT EXISTS (SELECT 1 FROM device_hr_interval t WHERE t.device_id = d.device_id);

-- ── device_other_interval ─────────────────────────────────────────────────────
INSERT INTO device_other_interval (device_id, interval, user_id, company_id)
SELECT d.device_id, 10, u.user_id, 1
FROM @devices d
LEFT JOIN user_profiles u ON u.device_id = d.device_id
WHERE NOT EXISTS (SELECT 1 FROM device_other_interval t WHERE t.device_id = d.device_id);

-- ── device_goal ───────────────────────────────────────────────────────────────
INSERT INTO device_goal (device_id, step, distance, calorie, user_id, company_id)
SELECT d.device_id, 10000, 5, 400, u.user_id, 1
FROM @devices d
LEFT JOIN user_profiles u ON u.device_id = d.device_id
WHERE NOT EXISTS (SELECT 1 FROM device_goal t WHERE t.device_id = d.device_id);

-- ── device_display ────────────────────────────────────────────────────────────
-- language=0 (English), hour_format=24, date_format=0, distance_unit=0 (km),
-- temperature_unit=0 (Celsius), wear_hand_right=0 (left wrist)
INSERT INTO device_display (device_id, language, hour_format, date_format, distance_unit, temperature_unit, wear_hand_right, user_id, company_id)
SELECT d.device_id, 0, 24, 0, 0, 0, 0, u.user_id, 1
FROM @devices d
LEFT JOIN user_profiles u ON u.device_id = d.device_id
WHERE NOT EXISTS (SELECT 1 FROM device_display t WHERE t.device_id = d.device_id);

-- ── device_auto_af ────────────────────────────────────────────────────────────
INSERT INTO device_auto_af (device_id, open, interval, rri_single_time, rri_type, user_id, company_id)
SELECT d.device_id, 0, 60, 0, 0, u.user_id, 1
FROM @devices d
LEFT JOIN user_profiles u ON u.device_id = d.device_id
WHERE NOT EXISTS (SELECT 1 FROM device_auto_af t WHERE t.device_id = d.device_id);

-- ── device_user_info ──────────────────────────────────────────────────────────
INSERT INTO device_user_info (device_id, user_id, company_id)
SELECT d.device_id, u.user_id, 1
FROM @devices d
LEFT JOIN user_profiles u ON u.device_id = d.device_id
WHERE NOT EXISTS (SELECT 1 FROM device_user_info t WHERE t.device_id = d.device_id);

-- ── device_bp_adjust ──────────────────────────────────────────────────────────
INSERT INTO device_bp_adjust (device_id, user_id, company_id)
SELECT d.device_id, u.user_id, 1
FROM @devices d
LEFT JOIN user_profiles u ON u.device_id = d.device_id
WHERE NOT EXISTS (SELECT 1 FROM device_bp_adjust t WHERE t.device_id = d.device_id);

-- ── Verification: coverage report ────────────────────────────────────────────
SELECT
    d.device_id,
    CASE WHEN ddf.device_id  IS NOT NULL THEN 'Y' ELSE '-' END AS data_freq,
    CASE WHEN dlf.device_id  IS NOT NULL THEN 'Y' ELSE '-' END AS locate_freq,
    CASE WHEN dgs.device_id  IS NOT NULL THEN 'Y' ELSE '-' END AS gps_settings,
    CASE WHEN dhr.device_id  IS NOT NULL THEN 'Y' ELSE '-' END AS hr_alarm,
    CASE WHEN ddhr.device_id IS NOT NULL THEN 'Y' ELSE '-' END AS dyn_hr_alarm,
    CASE WHEN dsp.device_id  IS NOT NULL THEN 'Y' ELSE '-' END AS spo2_alarm,
    CASE WHEN dbp.device_id  IS NOT NULL THEN 'Y' ELSE '-' END AS bp_alarm,
    CASE WHEN dta.device_id  IS NOT NULL THEN 'Y' ELSE '-' END AS temp_alarm,
    CASE WHEN dfs.device_id  IS NOT NULL THEN 'Y' ELSE '-' END AS fall_settings,
    CASE WHEN dhi.device_id  IS NOT NULL THEN 'Y' ELSE '-' END AS hr_interval,
    CASE WHEN doi.device_id  IS NOT NULL THEN 'Y' ELSE '-' END AS other_interval,
    CASE WHEN dg.device_id   IS NOT NULL THEN 'Y' ELSE '-' END AS goal,
    CASE WHEN dd.device_id   IS NOT NULL THEN 'Y' ELSE '-' END AS display,
    CASE WHEN daa.device_id  IS NOT NULL THEN 'Y' ELSE '-' END AS auto_af,
    CASE WHEN dui.device_id  IS NOT NULL THEN 'Y' ELSE '-' END AS user_info,
    CASE WHEN dba.device_id  IS NOT NULL THEN 'Y' ELSE '-' END AS bp_adjust
FROM @devices d
LEFT JOIN device_data_freq        ddf  ON ddf.device_id  = d.device_id
LEFT JOIN device_locate_freq      dlf  ON dlf.device_id  = d.device_id
LEFT JOIN device_gps_settings     dgs  ON dgs.device_id  = d.device_id
LEFT JOIN device_hr_alarm         dhr  ON dhr.device_id  = d.device_id
LEFT JOIN device_dynamic_hr_alarm ddhr ON ddhr.device_id = d.device_id
LEFT JOIN device_spo2_alarm       dsp  ON dsp.device_id  = d.device_id
LEFT JOIN device_bp_alarm         dbp  ON dbp.device_id  = d.device_id
LEFT JOIN device_temp_alarm       dta  ON dta.device_id  = d.device_id
LEFT JOIN device_fall_settings    dfs  ON dfs.device_id  = d.device_id
LEFT JOIN device_hr_interval      dhi  ON dhi.device_id  = d.device_id
LEFT JOIN device_other_interval   doi  ON doi.device_id  = d.device_id
LEFT JOIN device_goal             dg   ON dg.device_id   = d.device_id
LEFT JOIN device_display          dd   ON dd.device_id   = d.device_id
LEFT JOIN device_auto_af          daa  ON daa.device_id  = d.device_id
LEFT JOIN device_user_info        dui  ON dui.device_id  = d.device_id
LEFT JOIN device_bp_adjust        dba  ON dba.device_id  = d.device_id
ORDER BY d.device_id;
