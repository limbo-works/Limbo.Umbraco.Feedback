# Limbo Feedback

<table>
  <tr>
    <td><strong>License:</strong></td>
    <td><a href="./LICENSE.md"><strong>MIT License</strong></a></td>
  </tr>
  <tr>
    <td><strong>Umbraco:</strong></td>
    <td>
      Umbraco 17
    </td>
  </tr>
  <tr>
    <td><strong>Target Framework:</strong></td>
    <td>
      .NET 10
    </td>
  </tr>
</table>





<br /><br />

## Installation

The Umbraco 17 version of this package is only available via [NuGet](https://www.nuget.org/packages/Limbo.Umbraco.Feedback/17.0.0-alpha000). To install the package, you can use either .NET CLI:

```
dotnet add package Limbo.Umbraco.Feedback --version 17.0.0-alpha000
```

or the NuGet Package Manager:

```
Install-Package Limbo.Umbraco.Feedback -Version 17.0.0-alpha000
```

For Umbraco 13, see the [`v13/main`](https://github.com/limbo-works/Limbo.Umbraco.Feedback/tree/v13/main) branch. For older versions of Umbraco, see our [Skybrud.Umbraco.Feedback](https://github.com/skybrud/Skybrud.Umbraco.Feedback) package.

<br /><br />

## Upgrading from Umbraco 13

Umbraco 14 replaced the AngularJS back office, so this version rewrites the editor experience as a workspace view
and moves the back office endpoints to the Management API. See
[documentation/UPGRADE-UMBRACO-17.md](./documentation/UPGRADE-UMBRACO-17.md) for the full list of changes,
including the breaking changes to `IFeedbackPlugin` and `FeedbackPluginDependencies`.

The database schema, the configuration keys and the public `POST /api/feedback` endpoints are unchanged.

<br /><br />

## Development

The package consists of a .NET class library and a TypeScript/Lit client for the back office.

```bash
# C# only
dotnet build src/Limbo.Umbraco.Feedback.sln

# Back office client (requires Node.js 22+)
cd src/Limbo.Umbraco.Feedback/Client
npm install
npm run build      # or: npm run watch

# Release package, including the client assets
dotnet pack src/Limbo.Umbraco.Feedback.sln -c Release -p:BuildClientAssets=true
```
