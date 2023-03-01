using Limbo.Umbraco.Feedback.Constants;
using Limbo.Umbraco.Feedback.Models.Entries;
using Umbraco.Cms.Infrastructure.Migrations;

#pragma warning disable 1591

namespace Limbo.Umbraco.Feedback.Migrations {

    public class CreateTableMigration : MigrationBase {

        public CreateTableMigration(IMigrationContext context) : base(context) { }

        protected override void Migrate() {
            if (TableExists(FeedbackConstants.TableName)) return;
            Create.Table<FeedbackEntrySchema>().Do();
        }

    }

}