# appsettings JSON Schema Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ship an appsettings JSON schema for the Cultiv.Hangfire NuGet package so IDEs autocomplete and validate the `Hangfire:` configuration section.

**Architecture:** Three additions to the `Cultiv.Hangfire` package project: a JSON Schema file describing the `Hangfire:` settings, an MSBuild `.targets` file in `buildTransitive/` that registers the schema with Umbraco's build machinery, and two `None` item entries in the `.csproj` to pack both files into the NuGet package. No C# code changes required.

**Tech Stack:** JSON Schema draft-04, MSBuild, NuGet packaging, .NET 10

---

## File Map

| Action | Path | Responsibility |
|--------|------|----------------|
| Create | `Cultiv.Hangfire/appsettings-schema.Cultiv.Hangfire.json` | JSON Schema describing the `Hangfire:` config section |
| Create | `Cultiv.Hangfire/buildTransitive/Cultiv.Hangfire.targets` | MSBuild targets that register the schema with Umbraco |
| Modify | `Cultiv.Hangfire/Cultiv.Hangfire.csproj` | Pack both new files into the NuGet package |

---

### Task 1: Create the JSON Schema file

**Files:**
- Create: `Cultiv.Hangfire/appsettings-schema.Cultiv.Hangfire.json`

All properties come directly from `Cultiv.Hangfire/HangfireSettings.cs`.

- [ ] **Step 1: Create the schema file**

Create `Cultiv.Hangfire/appsettings-schema.Cultiv.Hangfire.json` with this exact content:

```json
{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "title": "CultivHangfireSchema",
  "type": "object",
  "properties": {
    "Hangfire": {
      "$ref": "#/definitions/HangfireDefinition"
    }
  },
  "definitions": {
    "HangfireDefinition": {
      "type": "object",
      "description": "Configuration for Cultiv.Hangfire.",
      "properties": {
        "UseStandaloneSection": {
          "type": "boolean",
          "description": "When true, the Hangfire dashboard is shown as a standalone section in Umbraco. Default: false.",
          "default": false
        },
        "Server": {
          "$ref": "#/definitions/HangfireServerDefinition"
        },
        "StorageOptions": {
          "$ref": "#/definitions/HangfireStorageOptionsDefinition"
        }
      }
    },
    "HangfireServerDefinition": {
      "type": "object",
      "description": "Configuration for the Hangfire server (job processor).",
      "properties": {
        "Disabled": {
          "type": "boolean",
          "description": "When true, the Hangfire server is not started and no jobs are processed. Default: false."
        },
        "QueueNames": {
          "type": "array",
          "description": "Queue names this server processes. Default: [\"default\"].",
          "items": {
            "type": "string"
          }
        }
      }
    },
    "HangfireStorageOptionsDefinition": {
      "type": "object",
      "description": "SQL Server storage options for Hangfire. Ignored when using SQLite.",
      "properties": {
        "PrepareSchemaIfNecessary": {
          "type": "boolean",
          "description": "Create or update the Hangfire database schema on startup. Default: true.",
          "default": true
        },
        "EnableHeavyMigrations": {
          "type": "boolean",
          "description": "Run heavy schema migrations on startup. Default: true.",
          "default": true
        },
        "CommandBatchMaxTimeout": {
          "type": "string",
          "description": "Maximum timeout for command batches in HH:mm:ss format. Default: \"00:05:00\".",
          "default": "00:05:00"
        },
        "SlidingInvisibilityTimeout": {
          "type": "string",
          "description": "Sliding invisibility timeout for queued jobs in HH:mm:ss format. Default: \"00:05:00\".",
          "default": "00:05:00"
        },
        "QueuePollInterval": {
          "type": "string",
          "description": "Interval between queue polls in HH:mm:ss format. \"00:00:00\" uses Hangfire's recommended setting. Default: \"00:00:00\".",
          "default": "00:00:00"
        },
        "UseRecommendedIsolationLevel": {
          "type": "boolean",
          "description": "Use the recommended SQL transaction isolation level. Default: true.",
          "default": true
        },
        "DisableGlobalLocks": {
          "type": "boolean",
          "description": "Disable global SQL locks in the storage provider. Default: true.",
          "default": true
        }
      }
    }
  }
}
```

- [ ] **Step 2: Validate it is well-formed JSON**

```bash
cat Cultiv.Hangfire/appsettings-schema.Cultiv.Hangfire.json | python3 -m json.tool > /dev/null && echo "Valid JSON"
```

Expected output: `Valid JSON`

- [ ] **Step 3: Commit**

```bash
git add Cultiv.Hangfire/appsettings-schema.Cultiv.Hangfire.json
git commit -m "feat: add appsettings JSON schema for Hangfire settings"
```

---

### Task 2: Create the MSBuild targets file

**Files:**
- Create: `Cultiv.Hangfire/buildTransitive/Cultiv.Hangfire.targets`

The `buildTransitive/` folder is the NuGet convention for `.targets` files that propagate transitively — required for Umbraco's schema machinery to pick up the file in consuming apps.

- [ ] **Step 1: Create the targets file**

Create `Cultiv.Hangfire/buildTransitive/Cultiv.Hangfire.targets` with this content:

```xml
<Project>
  <ItemGroup>
    <UmbracoJsonSchemaFiles Include="$(MSBuildThisFileDirectory)..\appsettings-schema.Cultiv.Hangfire.json" Weight="-10" />
  </ItemGroup>
</Project>
```

`$(MSBuildThisFileDirectory)` resolves to the directory containing this `.targets` file (i.e. `buildTransitive/`), so `..\` navigates up to the package root where the schema JSON lives.

`Weight="-10"` places this schema after Umbraco CMS entries in the merged `appsettings-schema.json`, which is the conventional value for third-party packages.

- [ ] **Step 2: Commit**

```bash
git add Cultiv.Hangfire/buildTransitive/Cultiv.Hangfire.targets
git commit -m "feat: add MSBuild targets to register schema with Umbraco"
```

---

### Task 3: Pack both files into the NuGet package

**Files:**
- Modify: `Cultiv.Hangfire/Cultiv.Hangfire.csproj`

The existing `.csproj` already packs `None` items (`README.md`, `LICENSE`, `logo.png`) with `Pack="true"`. Add two more entries following the same pattern.

- [ ] **Step 1: Add packing entries to the csproj**

Open `Cultiv.Hangfire/Cultiv.Hangfire.csproj`. Find the existing `ItemGroup` that contains the `None` items for `README.md`, `LICENSE`, and `logo.png` (lines 35-39):

```xml
<ItemGroup>
    <None Include="../README.md" Pack="true" PackagePath="" />
    <None Include="../LICENSE" Pack="true" PackagePath="" />
    <None Include="../logo.png" Pack="true" PackagePath="" />
</ItemGroup>
```

Add two new `None` entries after `logo.png`:

```xml
<ItemGroup>
    <None Include="../README.md" Pack="true" PackagePath="" />
    <None Include="../LICENSE" Pack="true" PackagePath="" />
    <None Include="../logo.png" Pack="true" PackagePath="" />
    <None Include="appsettings-schema.Cultiv.Hangfire.json" Pack="true" PackagePath="" />
    <None Include="buildTransitive/Cultiv.Hangfire.targets" Pack="true" PackagePath="buildTransitive/" />
</ItemGroup>
```

- [ ] **Step 2: Build the package and verify both files are present**

```bash
cd Cultiv.Hangfire
dotnet pack -c Release -o /tmp/cultiv-hangfire-pack
```

Then inspect the package contents:

```bash
unzip -l /tmp/cultiv-hangfire-pack/Cultiv.Hangfire.*.nupkg | grep -E "appsettings-schema|buildTransitive"
```

Expected output (two lines, paths may vary by version):
```
  ...  appsettings-schema.Cultiv.Hangfire.json
  ...  buildTransitive/Cultiv.Hangfire.targets
```

- [ ] **Step 3: Verify the schema is picked up by the test web project**

Build `Cultiv.Hangfire.Web` and confirm the schema file is copied and referenced:

```bash
cd ..
dotnet build Cultiv.Hangfire.Web
```

After the build, check that `appsettings-schema.Cultiv.Hangfire.json` has been copied to the web project directory and that `appsettings-schema.json` contains a `$ref` to it:

```bash
grep "Cultiv.Hangfire" Cultiv.Hangfire.Web/appsettings-schema.json
```

Expected output:
```
      "$ref": "appsettings-schema.Cultiv.Hangfire.json"
```

- [ ] **Step 4: Commit**

```bash
git add Cultiv.Hangfire/Cultiv.Hangfire.csproj
git commit -m "feat: pack appsettings schema and targets into NuGet package"
```
