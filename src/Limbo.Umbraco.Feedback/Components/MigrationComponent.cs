using System.Threading;
using System.Threading.Tasks;
using Limbo.Umbraco.Feedback.Migrations;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Scoping;

#pragma warning disable 1591

namespace Limbo.Umbraco.Feedback.Components;

/// <summary>
/// Runs the migration plan of the package on startup.
/// </summary>
/// <remarks><c>IComponent</c> is obsolete in Umbraco 17, so the component is now an <see cref="IAsyncComponent"/>.</remarks>
public class MigrationComponent : IAsyncComponent {

    private readonly IMigrationPlanExecutor _migrationPlanExecutor;
    private readonly IScopeProvider _scopeProvider;
    private readonly IKeyValueService _keyValueService;

    public MigrationComponent(IMigrationPlanExecutor migrationPlanExecutor, IScopeProvider scopeProvider, IKeyValueService keyValueService) {
        _migrationPlanExecutor = migrationPlanExecutor;
        _scopeProvider = scopeProvider;
        _keyValueService = keyValueService;
    }

    public async Task InitializeAsync(bool isRestarting, CancellationToken cancellationToken) {

        MigrationPlan plan = new("Limbo.Umbraco.Feedback");

        plan.From(string.Empty)
            .To<CreateTableMigration>("1.0.0-alpha001")
            .To<FixEmptyStringValuesMigration>("1.0.0-alpha004")
            .To<NoopMigration>("279d64a4");

        Upgrader upgrader = new(plan);

        await upgrader.ExecuteAsync(_migrationPlanExecutor, _scopeProvider, _keyValueService);

    }

    public Task TerminateAsync(bool isRestarting, CancellationToken cancellationToken) => Task.CompletedTask;

}
