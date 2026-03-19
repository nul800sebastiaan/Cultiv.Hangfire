# Design: appsettings JSON Schema for Cultiv.Hangfire

**Date:** 2026-03-19
**Issue:** https://github.com/nul800sebastiaan/Cultiv.Hangfire/issues/50

## Summary

Add an `appsettings` JSON schema so IDEs (Rider, VS Code, Visual Studio) provide autocompletion and validation for the `Hangfire:` configuration section when the package is installed.

## Approach

Schema file + MSBuild `.targets` file, following the pattern documented at
https://cornehoskam.com/posts/how-to-include-appsettings-schemajson-files-in-umbraco-packages/

When the consuming app builds, Umbraco's MSBuild machinery reads `UmbracoJsonSchemaFiles`, copies the schema file to the app's project directory, and adds a `$ref` to it in `appsettings-schema.json`. The IDE then uses that merged schema for completions.

## Changes

All changes are in the `Cultiv.Hangfire` package project.

### 1. `appsettings-schema.Cultiv.Hangfire.json`

A JSON Schema (draft-04, matching Umbraco's existing schemas) placed at the root of the `Cultiv.Hangfire` project. Describes the top-level `Hangfire` key with all properties derived from `HangfireSettings.cs`:

```
Hangfire
├── UseStandaloneSection  (boolean, default: false)
├── Server (object, optional)
│   ├── Disabled          (boolean, optional)
│   └── QueueNames        (array of string, optional)
└── StorageOptions (object)
    ├── PrepareSchemaIfNecessary    (boolean, default: true)
    ├── EnableHeavyMigrations       (boolean, default: true)
    ├── CommandBatchMaxTimeout      (string/duration, default: "00:05:00")
    ├── SlidingInvisibilityTimeout  (string/duration, default: "00:05:00")
    ├── QueuePollInterval           (string/duration, default: "00:00:00")
    ├── UseRecommendedIsolationLevel (boolean, default: true)
    └── DisableGlobalLocks          (boolean, default: true)
```

`TimeSpan` properties are represented as `string` with a description noting the `HH:mm:ss` format, matching how .NET configuration binds `TimeSpan` values from JSON.

### 2. `buildTransitive/Cultiv.Hangfire.targets`

MSBuild targets file that appends the schema file to `UmbracoJsonSchemaFiles`:

```xml
<Project>
  <ItemGroup>
    <UmbracoJsonSchemaFiles Include="$(MSBuildThisFileDirectory)..\appsettings-schema.Cultiv.Hangfire.json" Weight="-10" />
  </ItemGroup>
</Project>
```

`buildTransitive/` is the correct NuGet folder — unlike `build/`, it propagates the import transitively through the dependency graph, which is required for Umbraco's schema machinery to work in consuming apps. The `Weight="-10"` attribute controls ordering in `appsettings-schema.json`; a negative value places the package's schema after Umbraco CMS entries, which is conventional for third-party packages.

### 3. `Cultiv.Hangfire.csproj` updates

- Pack `appsettings-schema.Cultiv.Hangfire.json` as a `None` item into the NuGet package root (`PackagePath=""`)
- Pack `buildTransitive/Cultiv.Hangfire.targets` into `buildTransitive/` in the NuGet package (`PackagePath="buildTransitive/"`)

## Testing

After building and referencing the package:
- The consuming app's `appsettings-schema.json` should gain a `$ref` to `appsettings-schema.Cultiv.Hangfire.json`
- The schema file should be copied to the app project directory
- Typing `"Hangfire":` in `appsettings.json` should trigger IDE completions for all nested properties

The test web project (`Cultiv.Hangfire.Web`) already references the package directly and can be used to verify the schema is picked up after a build.
