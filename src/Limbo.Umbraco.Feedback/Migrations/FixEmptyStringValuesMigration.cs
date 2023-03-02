using Limbo.Umbraco.Feedback.Constants;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Migrations;

#pragma warning disable 1591

namespace Limbo.Umbraco.Feedback.Migrations {

    public class FixEmptyStringValuesMigration : MigrationBase {

        public FixEmptyStringValuesMigration(IMigrationContext context) : base(context) { }

        protected override void Migrate() {

            if (!TableExists(FeedbackConstants.TableName)) return;

            int affected1 = Context.Database.Execute("UPDATE [LimboFeedback] SET [Name] = null WHERE [Name] LIKE '';");
            int affected2 = Context.Database.Execute("UPDATE [LimboFeedback] SET [Email] = null WHERE [Email] LIKE '';");
            int affected3 = Context.Database.Execute("UPDATE [LimboFeedback] SET [Comment] = null WHERE [Comment] LIKE '';");

            Logger.LogInformation($"Fixed empty string values for name column in {affected1} rows.");
            Logger.LogInformation($"Fixed empty string values for email column in {affected2} rows.");
            Logger.LogInformation($"Fixed empty string values for comment column in {affected3} rows.");

        }

    }

}