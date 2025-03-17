using System;
using System.Collections.Generic;
using Limbo.Umbraco.Feedback.Constants;
using Limbo.Umbraco.Feedback.Extensions;
using Limbo.Umbraco.Feedback.Models.Entries;
using Limbo.Umbraco.Feedback.Plugins;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

#pragma warning disable 1591

namespace Limbo.Umbraco.Feedback.Services;

public class FeedbackDatabaseService {

    private readonly IScopeProvider _scopeProvider;

    #region Properties

    protected FeedbackPluginCollection Plugins { get; }

    #endregion

    #region Constructors

    public FeedbackDatabaseService(IScopeProvider scopeProvider, FeedbackPluginCollection feedbackPlugins) {
        _scopeProvider = scopeProvider;
        Plugins = feedbackPlugins;
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Returns an unpaginated list of all feedback entries.
    /// </summary>
    /// <returns>An array of <see cref="FeedbackEntryDto"/>.</returns>
    public IReadOnlyList<FeedbackEntryDto> GetAllEntries() {

        // Create a new scope
        using IScope scope = _scopeProvider.CreateScope();

        // Declare the SQL for the query
        Sql sql = new($"SELECT * FROM {FeedbackConstants.TableName} WHERE Archived = 0 ORDER BY Created DESC");

        // Make the call to the database
        return scope.Database.Fetch<FeedbackEntryDto>(sql);

    }

    /// <summary>
    /// Gets an unpaginated array of all feedback entries for the site with the specified <paramref name="siteId"/>.
    /// </summary>
    /// <param name="siteId">The ID of the site.</param>
    /// <returns>An array of <see cref="FeedbackEntry"/>.</returns>
    public IReadOnlyList<FeedbackEntryDto> GetAllEntriesForSite(int siteId) {

        // Create a new scope
        using IScope scope = _scopeProvider.CreateScope();

        // Declare the SQL for the query
        Sql sql = new($"SELECT * FROM {FeedbackConstants.TableName} WHERE SiteId = @0 AND Archived = 0 ORDER BY Created DESC", siteId);

        // Make the call to the database
        return scope.Database.Fetch<FeedbackEntryDto>(sql);

    }

    public IReadOnlyList<FeedbackEntryDto> GetEntries(FeedbackGetEntriesOptions options, out int total) {

        // Create a new scope
        using IScope scope = _scopeProvider.CreateScope();

        // Declare the SQL for the query
        Sql<ISqlContext> sql = scope.SqlContext
            .Sql()
            .Select<FeedbackEntryDto>()
            .From<FeedbackEntryDto>();

        sql.Where<FeedbackEntryDto>(x => !x.IsArchived);

        if (options.Rating != null) {
            sql.Where<FeedbackEntryDto>(x => x.Rating == options.Rating);
        }

        if (options.Responsible != null) {
            sql.Where<FeedbackEntryDto>(x => x.AssignedTo == options.Responsible);
        }

        if (options.Status != null) {
            sql.Where<FeedbackEntryDto>(x => x.Status == options.Status);
        }

        if (options.Type == FeedbackEntryType.Comment) {
            sql.Where<FeedbackEntryDto>(x => x.Comment != null);
        }

        if (options.Type == FeedbackEntryType.Rating) {
            sql.Where<FeedbackEntryDto>(x => x.Comment == null);
        }

        if (options.SiteKey != Guid.Empty) {
            sql = sql.Where<FeedbackEntryDto>(x => x.SiteKey == options.SiteKey);
        }

        if (options.PageKey != Guid.Empty) {
            sql = sql.Where<FeedbackEntryDto>(x => x.PageKey == options.PageKey);
        }

        // Order the results
        sql = options.SortField switch {
            EntriesSortField.Rating => sql.OrderBy<FeedbackEntryDto>(x => x.Rating, options.SortOrder),
            EntriesSortField.Status => sql.OrderBy<FeedbackEntryDto>(x => x.Status, options.SortOrder),
            _ => sql.OrderBy<FeedbackEntryDto>(x => x.CreateDate, options.SortOrder)
        };

        // Make the call to the database
        return scope.Database.Page<FeedbackEntryDto>(options.Page, options.PerPage, sql, out total);

    }

    public IReadOnlyList<FeedbackEntryDto> GetEntriesForSite(Guid siteKey, int limit, int page, out int total) {

        // Create a new scope
        using IScope scope = _scopeProvider.CreateScope();

        // Declare the SQL for the query
        var sql = scope.SqlContext.Sql()
            .Select<FeedbackEntryDto>()
            .From<FeedbackEntryDto>()
            .Where<FeedbackEntryDto>(x => x.SiteKey == siteKey && x.IsArchived == false)
            .OrderByDescending<FeedbackEntryDto>(x => x.UpdateDate);

        // Make the call to the database
        return scope.Database.Page<FeedbackEntryDto>(page, limit, sql, out total);

    }

    /// <summary>
    /// Gets the entry with the specified <paramref name="entryId"/>.
    /// </summary>
    /// <param name="entryId">The ID of the entry.</param>
    /// <returns>An instance of <see cref="FeedbackEntryDto"/> or <c>null</c> if not found.</returns>
    public FeedbackEntryDto? GetEntryById(int entryId) {

        // Create a new scope
        using IScope scope = _scopeProvider.CreateScope();

        // Declare the SQL for the query
        Sql<ISqlContext> sql = scope.SqlContext.Sql()
            .Select<FeedbackEntryDto>()
            .From<FeedbackEntryDto>()
            .Where<FeedbackEntryDto>(x => x.Id == entryId && !x.IsArchived);

        // Make the call to the database
        return scope.Database.First<FeedbackEntryDto>(sql);

    }

    public FeedbackEntryDto? GetEntryByKey(Guid key) {

        // Create a new scope
        using IScope scope = _scopeProvider.CreateScope();

        // Declare the SQL for the query
        Sql<ISqlContext> sql = scope.SqlContext.Sql()
            .Select<FeedbackEntryDto>()
            .From<FeedbackEntryDto>()
            .Where<FeedbackEntryDto>(x => x.Key == key && !x.IsArchived);

        // Make the call to the database
        return scope.Database.FirstOrDefault<FeedbackEntryDto>(sql);

    }

    public void Insert(FeedbackEntryDto entry) {
        using IScope scope = _scopeProvider.CreateScope();
        entry.Id = (int) (decimal) scope.Database.Insert(entry);
        scope.Complete();
    }

    public void Update(FeedbackEntryDto entry) {
        using IScope scope = _scopeProvider.CreateScope();
        scope.Database.Update(entry);
        scope.Complete();
    }

    public void Delete(FeedbackEntryDto entry) {
        using IScope scope = _scopeProvider.CreateScope();
        scope.Database.Delete(entry);
        scope.Complete();
    }

    /// <summary>
    /// Deletes all entries before the specified <paramref name="date"/>.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <returns>The amount of affected/deleted rows.</returns>
    public int DeleteAll(DateTime date) {

        // Create a new scope
        using IScope scope = _scopeProvider.CreateScope();

        // Delete everything before the start of the day after "date"
        Sql sql = new($"DELETE FROM {FeedbackConstants.TableName} WHERE Created < '{date.Date.AddDays(1):yyyy-MM-dd}';");

        // Make the call to the database
        int affected = scope.Database.Execute(sql);

        // Complete the scope
        scope.Complete();

        // Return the amount of affected rows
        return affected;

    }

    #endregion

}