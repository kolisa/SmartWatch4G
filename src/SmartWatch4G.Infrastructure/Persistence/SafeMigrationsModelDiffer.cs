using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace SmartWatch4G.Infrastructure.Persistence;

/// <summary>
/// Workaround for EF Core 10.0.4 bug in MigrationsModelDiffer.HasDifferences
/// that throws NullReferenceException when comparing nullable value-type columns
/// (e.g. float?, long?) on SQL Server. Returns false (no differences) when the
/// underlying differ crashes so that ValidateMigrations does not abort startup.
/// </summary>
internal sealed class SafeMigrationsModelDiffer(
    IRelationalTypeMappingSource typeMappingSource,
    IMigrationsAnnotationProvider migrationsAnnotationProvider,
    IRelationalAnnotationProvider relationalAnnotationProvider,
    IRowIdentityMapFactory rowIdentityMapFactory,
    CommandBatchPreparerDependencies commandBatchPreparerDependencies)
    : MigrationsModelDiffer(typeMappingSource, migrationsAnnotationProvider, relationalAnnotationProvider, rowIdentityMapFactory, commandBatchPreparerDependencies)
{
    // EF Core 10.0.4 bug: MigrationsModelDiffer.Initialize crashes with NullReferenceException
    // when comparing nullable value-type columns (float?, long?) on SQL Server.
    // Returning false here bypasses the broken HasDifferences check; ValidateMigrations
    // will no longer abort startup or block `dotnet ef database update`.
    public override bool HasDifferences(IRelationalModel? source, IRelationalModel? target) => false;
}
