-- Bulk insert users for company_id = 1
-- Each row uses MERGE to avoid duplicate errors on re-run.
-- name  = first name only (as provided)
-- surname = watch label (e.g. "Watch 19") used as a placeholder — update if real surnames are known.

MERGE user_profiles AS target
USING (VALUES
    ('863758060986873', 'Kethotse',  'Watch 19', 1),
    ('863758060926292', 'Christina', 'Watch 7',  1),
    ('863758060956587', 'Mpolokeng', 'Watch 8',  1),
    ('863758060926754', 'Patsiua',   'Watch 5',  1),
    ('863758060987517', 'Stephen',   'Watch 6',  1),
    ('863758060987855', 'Lebo',      'Watch 13', 1),
    ('863758060927422', 'Karabo',    'Watch 20', 1),
    ('863758060926499', 'Maomoji',   'Watch 9',  1),
    ('863758060927455', 'Solomon',   'Watch 10', 1),
    ('863758060982484', 'Tebogo',    'Watch 18', 1),
    ('863758060926564', 'Brayen',    'Watch 14', 1),
    ('863758060987483', 'Poloko',    'Watch 15', 1),
    ('863758060927232', 'Gomolemo',  'Watch 16', 1)
) AS source (device_id, name, surname, company_id)
ON target.device_id = source.device_id
WHEN MATCHED THEN
    UPDATE SET
        name       = source.name,
        surname    = source.surname,
        company_id = source.company_id,
        is_active  = 1,
        updated_at = GETDATE()
WHEN NOT MATCHED THEN
    INSERT (device_id, name, surname, company_id, is_active, updated_at)
    VALUES (source.device_id, source.name, source.surname, source.company_id, 1, GETDATE());

SELECT device_id, name, surname, company_id, is_active, updated_at
FROM user_profiles
WHERE device_id IN (
    '863758060986873','863758060926292','863758060956587',
    '863758060926754','863758060987517','863758060987855',
    '863758060927422','863758060926499','863758060927455',
    '863758060982484','863758060926564','863758060987483',
    '863758060927232'
)
ORDER BY name;
