-- =============================================================================
-- Seed: Miner devices -> company_id = 1
-- Idempotent: safe to re-run (MERGE upserts on device_id PK).
-- =============================================================================

DECLARE @companyId INT = 1;

MERGE user_profiles AS target
USING (VALUES
    ('863758060926127', 'Miner', 'Watch 58'),
    ('863758060927778', 'Miner', 'Watch 59'),
    ('863758060941704', 'Miner', 'Watch 61'),
    ('863758060987152', 'Miner', 'Watch 63'),
    ('863758060982021', 'Miner', 'Watch 68'),
    ('863758060980504', 'Miner', 'Watch 69'),
    ('863758060981700', 'Miner', 'Watch 70'),
    ('863758060987525', 'Miner', 'Watch 73'),
    ('863758060981502', 'Miner', 'Watch 75'),
    ('863758060980678', 'Miner', 'Watch 77'),
    ('863758060927356', 'Miner', 'Watch 78'),
    ('863758060987574', 'Miner', 'Watch 79'),
    ('863758060983904', 'Miner', 'Watch 80'),
    ('863758060987202', 'Miner', 'Watch 81'),
    ('863758060983821', 'Miner', 'Watch 82'),
    ('863758060926473', 'Miner', 'Watch 83'),
    ('863758060926697', 'Miner', 'Watch 84')
) AS source (device_id, name, surname)
ON target.device_id = source.device_id
WHEN MATCHED THEN
    UPDATE SET
        name       = source.name,
        surname    = source.surname,
        company_id = @companyId,
        is_active  = 1,
        updated_at = GETDATE()
WHEN NOT MATCHED THEN
    INSERT (device_id, name, surname, company_id, is_active, updated_at)
    VALUES (source.device_id, source.name, source.surname, @companyId, 1, GETDATE());

PRINT CONCAT(@@ROWCOUNT, ' row(s) affected.');
