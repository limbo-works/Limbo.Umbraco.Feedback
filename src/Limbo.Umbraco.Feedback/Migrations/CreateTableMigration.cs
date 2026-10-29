using System.Threading.Tasks;
using Limbo.Umbraco.Feedback.Constants;
using Limbo.Umbraco.Feedback.Models.Entries;
using Umbraco.Cms.Infrastructure.Migrations;

#pragma warning disable 1591

namespace Limbo.Umbraco.Feedback.Migrations;

public class CreateTableMigration : AsyncMigrationBase {

    public CreateTableMigration(IMigrationContext context) : base(context) { }

    protected override Task MigrateAsync() {
        if (TableExists(FeedbackConstants.TableName)) return Task.CompletedTask;
        Create.Table<FeedbackEntrySchema>().Do();
        return Task.CompletedTask;
    }

}
