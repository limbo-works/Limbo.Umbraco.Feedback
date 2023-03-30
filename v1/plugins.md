# Plugins

The feedback module may be extended through *plugins*. Usually it shouldn't be necessary to implement more than one custom plugin, but multiple plugins may be added to the feedback module to handle different scenarios.

In the package, plugins are represented by the `IFeedbackPlugin` interface, so your custom plugin should either implement this interface in it's entirety - or extend the `FeedbackPluginBase` base class. Extending the base class is the recommended approach, as you then get the base class' default implementation while also allowing you to override the parts that you need to.




## Composer

Your custom feedback plugin may be registered via a composer - eg. such as:

```csharp
public class MyFeedbackComposer : IComposer {

    public void Compose(IUmbracoBuilder builder) {
        builder.FeedbackPlugins().Append<MyFeedbackPlugin>();
    }

}
```



## Methods





### TryGetSite

Each feedback entry must be associated with a site. Some Umbraco installations will only ever have a single site, while others may have several sites. The feedback package doesn't know about which sites that is in your Umbraco installation, so this is where the `TryGetSite` method comes in.

The method follows the `TryGet` pattern, so if the method is able to lookup a site from a given GUID key, the method returns `true` and exposes a `FeedbackSiteSettings` instance via the method's `result` out parameter. If unsuccessful, the method on the other hand returns `null` and the `return` parameter is `null`.

The example implementation below uses the Umbraco context to look up the site node. In this fictional setup, the Umbraco installation allows one or more sites using the content type with the alias `site`. So the method is successful if the content type of the resolved `IPublishedContent` instance has the alias `site`.

The `FeedbackSiteSettings` class represents limited information about the site. You may extend this class to provide additional information about your sites.

```csharp
public override bool TryGetSite(Guid key, [NotNullWhen(true)] out FeedbackSiteSettings? result) {

    if (!_umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? context)) {
        result = null;
        return false;
    }

    IPublishedContent? content = context.Content?.GetById(key);

    switch (content?.ContentType.Alias) {

        case "site":
            result = new FeedbackSiteSettings(content);
            return true;

        default:
            result = null;
            return false;

    }

}
```

An overload of the `TryGetSite` method takes an `IPublishedContent` instance as it's first parameter instead of the GUID `key`. The `IPublishedContent` instance represents a page of the site, so the method should determine the site based on the page.

The example implementation assumes that the site node is always the node at the first level of the node's path:

```csharp
public override bool TryGetSite(IContent content, [NotNullWhen(true)] out FeedbackSiteSettings? result) {

    result = null;

    int[] path = content.Path.ToInt32Array();
    if (path.Length <= 1) return false;

    if (!_umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbraco)) return false;

    IPublishedContent? site = umbraco.Content?.GetById(path[1]);
    result = site is null ? null : new FeedbackSiteSettings(site);
    return result is not null;

}
```



### TryGetContentApp

The feedback package features a content app, which is available in two different versions - one works at the site-level showing all feedback entries under a given site, while the other works at the page-level only showing feedback entries for a specific page.

The `TryGetContentApp` method has two normal parameters - the first being an instance of `IContent` representing the current node being viewed by the user, and the second being a collection of `IReadOnlyUserGroup` representing the user groups that the user is part of.

If the content app should be shown for the page, the third parameter should hold a `ContentApp` instance with the details about the content app. If you're extending the `FeedbackPluginBase` base class, you can use the `GetContentAppForSite` and `GetContentAppForPage` methods to get the defualt content app for either a site or a page respectively. These two methods are also virtual, meaning you're able to override the default implementation.

```csharp
public override bool TryGetContentApp(IContent content, IEnumerable<IReadOnlyUserGroup> userGroups, [NotNullWhen(true)] out ContentApp? result) {

    result = null;

    if (content.ContentType.Alias == "site") {
        result = GetContentAppForSite(content);
    } else {

        int[] path = content.Path.ToInt32Array();

        if (path.Length >= 1) {
            IContent? site = _contentService.GetById(path[1]);
            result = site is null ? null : GetContentAppForPage(site, content);
        }

    }

    return result != null;

}
```



### GetUsers

The feedback package supports that a user may be assigned to each feedback entry. The package does not dictate the source of these users - but it could be based on all Umbraco backoffice users:

```csharp
public override IFeedbackUser[] GetUsers() {
    return _userService
        .GetAll(0, int.MaxValue, out _)
        .Select(x => (IFeedbackUser) new FeedbackUser(x))
        .ToArray();
}
```


### OnUserAssigned

When a user is assigned to a feedback entry, the `OnUserAssigned` method is called. This can be used to set up a specific workflow - eg. send a mail to the user when they are assigned.

In the example below, we ignore the entry if the user was unassigned (when `newUser` is `null`). And also if `oldUser` and `newUser` represents the same user.

```csharp
public override void OnUserAssigned(FeedbackService service, FeedbackEntry entry, IFeedbackUser? oldUser, IFeedbackUser? newUser) {

    // Skip if no user was assigned
    if (newUser == null) return;

    // Skip if the new user is the same as the old user
    if (oldUser != null && oldUser.Id == newUser.Id) return;

    // TODO: Notify the user

}
```





## Add new entry

If a new feedback entry is added via the [`/api/feedback`](../endpoints.md#update-entry) endpoint or directly via the `FeedbackService.AddEntry` method, the `OnEntryAdding` and `OnEntryAdded` methods of your plugin are called.

### OnEntryAdding

The `OnEntryAdding` method may be used to validate the request as well as updating the entry before it is saved in the database. As shown in the exmaple below, the method checks against a fictious rate limit, and if everything still looks OK, assigns a user to the entry.

Notice that the event may be cancelled. If a plugin cancels the event, the `OnEntryAdding` won't be called for any subsequent plugins.

```csharp
public override void OnEntryAdding(EntryAddingEventArgs args) {

    // Check whether the user has hit the rate limit
    if (...) {
        args.Cancel("Hold on!", HttpStatusCode.TooManyRequests);
        return;
    }

    // Get a reference to the published page
    IPublishedContent? page = _umbracoContextAccessor.GetRequiredUmbracoContext().Content?.GetById(args.Entry.PageKey);
    Guid editorResponsibleUser = page?.Value<Guid>("editorResponsibleUser") ?? Guid.Empty;

    // Skip if page doesn't have a responsible user
    if (editorResponsibleUser == Guid.Empty) return;

    // Skip if the responsible user isn't found (eg. no longer exists)
    if (!args.Service.TryGetUser(editorResponsibleUser, out IFeedbackUser? user)) return;

    // Assign the new entry to the responsible user
    args.Entry.AssignedTo = user;

}
```

### OnEntryAdded

The `OnEntryAdded` is called after the new feedback entry has been added to the database:

```csharp
public override void OnRatingSubmitted(RatingSubmittedEventArgs args) {

   // do your thing 

}
```




## Update an existing entry

If your frontend is adding entries in two steps, you may update an existing entry via the [`/api/feedback/{key}`](../endpoints.md#add-new-entry) endpoint. If this is the case - our you're calling the `FeedbackService.UpdateEntry` method directly, the `OnEntryUpdating` and `OnEntryUpdated` methods of your plugin are called.

### OnEntryUpdating

### OnEntryUpdated