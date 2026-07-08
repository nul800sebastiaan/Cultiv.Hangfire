using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Hangfire.States;
using Hangfire.Storage.SQLite;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Cultiv.Hangfire;

public class HangfireComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // Configure Hangfire settings
        builder.Services.Configure<HangfireSettings>(builder.Config.GetSection("Hangfire"));

        var serverDisabled = false;
        string[] queueNames = [EnqueuedState.DefaultQueue];
        var settings = builder.Config.GetSection("Hangfire").Get<HangfireSettings>();
        if (settings != null && settings.Server != null)
        {
            serverDisabled = settings.Server.Disabled.GetValueOrDefault(defaultValue: false);

            if (settings.Server.QueueNames is { Length: > 0 })
            {
                queueNames = settings.Server.QueueNames;
            }
        }

        var provider =
            builder.Config.GetConnectionStringProviderName(Umbraco.Cms.Core.Constants.System.UmbracoConnectionName);
        var connectionString = builder.GetConnectionString();

        // Use SQLite when it's explicitly configured, when there's no connection string yet
        // (the package was installed before Umbraco was set up - issue #11), or when the
        // connection string points at a SQLite database. That last case covers local Umbraco
        // Cloud sites where Deploy restores to SQLite but leaves the provider name unset:
        // https://github.com/nul800sebastiaan/Cultiv.Hangfire/issues/44
        if (provider.InvariantEquals("Microsoft.Data.Sqlite")
            || string.IsNullOrEmpty(connectionString)
            || connectionString.Contains(".sqlite.db", StringComparison.OrdinalIgnoreCase))
        {
            UseSqliteStorage(builder, serverDisabled, queueNames);
        }
        else
        {
            UseSqlServerStorage(builder, connectionString, serverDisabled, queueNames, settings);
        }
    }

    private static void UseSqlServerStorage(IUmbracoBuilder builder, string connectionString, bool serverDisabled, string[] queueNames, HangfireSettings? settings)
    {
        // Explicitly use the SqlConnection in the Microsoft.Data namespace to support extended connection string parameters such as "authentication"
        // https://github.com/HangfireIO/Hangfire/issues/1827
        SqlConnection ConnectionFactory() => new(connectionString);

        // Use provided settings or create defaults
        var storageOptions = settings?.StorageOptions ?? new StorageOptions();

        // Configure Hangfire to use our current database and add the option to write console messages
        builder.Services.AddHangfire(configuration =>
        {
            configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseColouredConsoleLogProvider()
                .UseDashboardMetric(SqlServerStorage.ActiveConnections)
                .UseDashboardMetric(SqlServerStorage.TotalConnections)
                .UseDashboardMetric(DashboardMetrics.AwaitingCount)
                .UseDashboardMetric(DashboardMetrics.FailedCount)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseConsole()
                .UseSqlServerStorage((Func<SqlConnection>)ConnectionFactory, new SqlServerStorageOptions
                {
                    PrepareSchemaIfNecessary = storageOptions.PrepareSchemaIfNecessary,
                    EnableHeavyMigrations = storageOptions.EnableHeavyMigrations,
                    CommandBatchMaxTimeout = storageOptions.CommandBatchMaxTimeout,
                    SlidingInvisibilityTimeout = storageOptions.SlidingInvisibilityTimeout,
                    QueuePollInterval = storageOptions.QueuePollInterval,
                    UseRecommendedIsolationLevel = storageOptions.UseRecommendedIsolationLevel,
                    DisableGlobalLocks = storageOptions.DisableGlobalLocks
                });
        });

        builder.AddHangfireToUmbraco(serverDisabled: serverDisabled, queueNames);

        // Explicitly set the storage parameters - needed if there if this is the first time Hangfire
        // gets initialized and there is already code to schedule jobs
        // Prevents: https://discuss.hangfire.io/t/jobstorage-current-property-value-has-not-been-initialized/884
        JobStorage.Current = new SqlServerStorage((Func<SqlConnection>)ConnectionFactory);
    }

    private static void UseSqliteStorage(IUmbracoBuilder builder, bool serverDisabled, string[] queueNames)
    {
        // Store the db in umbraco/Data to prevent dotnet watch from triggering a reload loop
        // on the SQLite WAL file: https://github.com/nul800sebastiaan/Cultiv.Hangfire/issues/47
        const string dbPath = "umbraco/Data/Hangfire.db";
        Directory.CreateDirectory("umbraco/Data");

        // Migrate from the old root location, but only if there isn't already a
        // database in the new location - otherwise the new location wins and we
        // leave the stale root file alone: https://github.com/nul800sebastiaan/Cultiv.Hangfire/issues/58
        if (File.Exists("Hangfire.db") && File.Exists(dbPath) == false)
        {
            File.Move("Hangfire.db", dbPath, overwrite: false);
        }

        GlobalConfiguration.Configuration.UseSQLiteStorage(dbPath);

        builder.Services.AddHangfire(configuration =>
        {
            configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseColouredConsoleLogProvider()
                .UseDashboardMetric(SqlServerStorage.ActiveConnections)
                .UseDashboardMetric(SqlServerStorage.TotalConnections)
                .UseDashboardMetric(DashboardMetrics.AwaitingCount)
                .UseDashboardMetric(DashboardMetrics.FailedCount)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseConsole();
        });

        builder.AddHangfireToUmbraco(serverDisabled: serverDisabled, queueNames);

        // Explicitly set the storage parameters - needed if there if this is the first time Hangfire
        // gets initialized and there is already code to schedule jobs
        // Prevents: https://discuss.hangfire.io/t/jobstorage-current-property-value-has-not-been-initialized/884
        JobStorage.Current = new SQLiteStorage(dbPath, new SQLiteStorageOptions());
    }
}