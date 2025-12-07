using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Hangfire.Storage.SQLite;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Umbraco.Cms.Core.Composing;

namespace Cultiv.Hangfire;

public class HangfireComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        var serverDisabled = false;
        var settings = builder.Config.GetSection("Hangfire").Get<HangfireSettings>();
        if (settings != null && settings.Server != null)
        {
            serverDisabled = settings.Server.Disabled.GetValueOrDefault(defaultValue: false);
        }

        var provider =
            builder.Config.GetConnectionStringProviderName(Umbraco.Cms.Core.Constants.System.UmbracoConnectionName);

        if (provider.InvariantEquals("Microsoft.Data.SQLite"))
        {
            UseSqliteStorage(builder, serverDisabled);
        }
        else
        {
            var connectionString = builder.GetConnectionString();
            if (string.IsNullOrEmpty(connectionString))
            {
                // This might happen when the package is installed before Umbraco is installed
                // https://github.com/nul800sebastiaan/Cultiv.Hangfire/issues/11
                // also happens on Umbraco Cloud sites that have been cloned locally - use SQLite instead
                UseSqliteStorage(builder, serverDisabled);
            }
            else
            {
                UseSqlServerStorage(builder, connectionString, serverDisabled, settings);
            }
        }
    }

    private static void UseSqlServerStorage(IUmbracoBuilder builder, string connectionString, bool serverDisabled, HangfireSettings settings)
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

        builder.AddHangfireToUmbraco(serverDisabled: serverDisabled);

        // Explicitly set the storage parameters - needed if there if this is the first time Hangfire
        // gets initialized and there is already code to schedule jobs
        // Prevents: https://discuss.hangfire.io/t/jobstorage-current-property-value-has-not-been-initialized/884
        JobStorage.Current = new SqlServerStorage((Func<SqlConnection>)ConnectionFactory);
    }

    private static void UseSqliteStorage(IUmbracoBuilder builder, bool serverDisabled)
    {
        GlobalConfiguration.Configuration.UseSQLiteStorage();

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

        builder.AddHangfireToUmbraco(serverDisabled: serverDisabled);


        // Explicitly set the storage parameters - needed if there if this is the first time Hangfire
        // gets initialized and there is already code to schedule jobs
        // Prevents: https://discuss.hangfire.io/t/jobstorage-current-property-value-has-not-been-initialized/884
        JobStorage.Current = new SQLiteStorage("Hangfire.db", new SQLiteStorageOptions());
    }
}