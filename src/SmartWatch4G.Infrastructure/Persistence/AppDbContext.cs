using SmartWatch4G.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SmartWatch4G.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the 4G wearable data platform.
/// Replaces the original file-based logging with a proper relational store.
/// </summary>
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<DeviceInfoRecord> DeviceInfoRecords => Set<DeviceInfoRecord>();
    public DbSet<DeviceStatusRecord> DeviceStatusRecords => Set<DeviceStatusRecord>();
    public DbSet<CallLogRecord> CallLogRecords => Set<CallLogRecord>();
    public DbSet<AlarmEventRecord> AlarmEventRecords => Set<AlarmEventRecord>();
    public DbSet<HealthDataRecord> HealthDataRecords => Set<HealthDataRecord>();
    public DbSet<SleepDataRecord> SleepDataRecords => Set<SleepDataRecord>();
    public DbSet<EcgDataRecord> EcgDataRecords => Set<EcgDataRecord>();
    public DbSet<RriDataRecord> RriDataRecords => Set<RriDataRecord>();
    public DbSet<GnssTrackRecord> GnssTrackRecords => Set<GnssTrackRecord>();
    public DbSet<Spo2DataRecord> Spo2DataRecords => Set<Spo2DataRecord>();
    public DbSet<AccDataRecord> AccDataRecords => Set<AccDataRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // DeviceInfoRecord — upsert by DeviceId; keep history in other tables
        modelBuilder.Entity<DeviceInfoRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.DeviceId).IsUnique();
            e.Property(x => x.DeviceId).IsRequired().HasMaxLength(64);
        });

        modelBuilder.Entity<DeviceStatusRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.DeviceId, x.ReceivedAt });
            e.Property(x => x.DeviceId).HasMaxLength(64);
        });

        modelBuilder.Entity<CallLogRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.DeviceId, x.IsSosAlarm });
            e.Property(x => x.DeviceId).HasMaxLength(64);
        });

        modelBuilder.Entity<AlarmEventRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.DeviceId, x.AlarmType, x.AlarmTime });
            e.Property(x => x.AlarmType).HasMaxLength(64);
            e.Property(x => x.AlarmTime).HasMaxLength(32);
            e.Property(x => x.DeviceId).HasMaxLength(64);
        });

        modelBuilder.Entity<HealthDataRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.DeviceId, x.DataTime });
            e.Property(x => x.DeviceId).HasMaxLength(64);
            e.Property(x => x.DataTime).HasMaxLength(32);
        });

        modelBuilder.Entity<SleepDataRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.DeviceId, x.SleepDate, x.Seq });
            e.Property(x => x.DeviceId).HasMaxLength(64);
            e.Property(x => x.SleepDate).HasMaxLength(32);
        });

        modelBuilder.Entity<EcgDataRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.DeviceId, x.DataTime });
            e.Property(x => x.DeviceId).HasMaxLength(64);
            e.Property(x => x.DataTime).HasMaxLength(32);
        });

        modelBuilder.Entity<RriDataRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.DeviceId, x.DataTime });
            e.Property(x => x.DeviceId).HasMaxLength(64);
            e.Property(x => x.DataTime).HasMaxLength(32);
        });

        modelBuilder.Entity<GnssTrackRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.DeviceId, x.TrackTime });
            e.Property(x => x.DeviceId).HasMaxLength(64);
            e.Property(x => x.TrackTime).HasMaxLength(32);
        });

        modelBuilder.Entity<Spo2DataRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.DeviceId, x.DataTime });
            e.Property(x => x.DeviceId).HasMaxLength(64);
            e.Property(x => x.DataTime).HasMaxLength(32);
        });

        modelBuilder.Entity<AccDataRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.DeviceId, x.DataTime });
            e.Property(x => x.DeviceId).HasMaxLength(64);
            e.Property(x => x.DataTime).HasMaxLength(32);
        });

        ApplyUtcDateTimeConvention(modelBuilder);
    }

    /// <summary>
    /// Registers value converters for every DateTime / DateTime? property so that:
    ///   • on write — values are normalised to UTC before persisting
    ///   • on read  — the Kind is forced to Utc, preventing off-by-offset bugs
    /// </summary>
    private static void ApplyUtcDateTimeConvention(ModelBuilder modelBuilder)
    {
        var utcConverter = new ValueConverter<System.DateTime, System.DateTime>(
            v => v.Kind == System.DateTimeKind.Unspecified
                     ? System.DateTime.SpecifyKind(v, System.DateTimeKind.Utc)
                     : v.ToUniversalTime(),
            v => System.DateTime.SpecifyKind(v, System.DateTimeKind.Utc));

        var utcNullableConverter = new ValueConverter<System.DateTime?, System.DateTime?>(
            v => ToUtcNullable(v),
            v => v.HasValue ? System.DateTime.SpecifyKind(v.Value, System.DateTimeKind.Utc) : null);

        // EF Core 10 requires an explicit column type when a ValueConverter maps
        // T → T (same type), otherwise MigrationsModelDiffer receives a null
        // typeMapping and throws NullReferenceException during HasDifferences().
        // "datetime2" is what the SQL Server provider would infer anyway.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(System.DateTime))
                {
                    property.SetValueConverter(utcConverter);
                    property.SetColumnType("datetime2");
                }
                else if (property.ClrType == typeof(System.DateTime?))
                {
                    property.SetValueConverter(utcNullableConverter);
                    property.SetColumnType("datetime2");
                }
            }
        }
    }

    private static System.DateTime? ToUtcNullable(System.DateTime? v)
    {
        if (!v.HasValue) return null;
        return v.Value.Kind == System.DateTimeKind.Unspecified
            ? System.DateTime.SpecifyKind(v.Value, System.DateTimeKind.Utc)
            : v.Value.ToUniversalTime();
    }
}
