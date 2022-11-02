using System;
using Hangfire;
using Hangfire.Console;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Cms.Web.Common.Authorization;

namespace Cultiv.Hangfire; 

public class HangfireComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.ManifestFilters().Append<ManifestFilter>();

        var connectionString = builder.GetConnectionString();
        if (string.IsNullOrEmpty(connectionString))
        {
            // This might happen when the package is installed before Umbraco is installed
            // https://github.com/nul800sebastiaan/Cultiv.Hangfire/issues/11
            return;
        }

        // Explicitly use the SqlConnection in the Microsoft.Data namespace to support extended connection string parameters such as "authentication"
        // https://github.com/HangfireIO/Hangfire/issues/1827
        var dbConnFunc = () => new Microsoft.Data.SqlClient.SqlConnection(connectionString);

        // Configure Hangfire to use our current database and add the option to write console messages
        builder.Services.AddHangfire(configuration =>
        {
            configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseConsole()
                .UseSqlServerStorage(dbConnFunc, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true,
                });
        });

        // Run the required server so your queued jobs will get executed
        builder.Services.AddHangfireServer();

        AddAuthorizedUmbracoDashboard(builder);

        // For some reason we need to give it the connection string again, else we get this error:
        // https://discuss.hangfire.io/t/jobstorage-current-property-value-has-not-been-initialized/884
        JobStorage.Current = new SqlServerStorage(dbConnFunc);
    }

    private static void AddAuthorizedUmbracoDashboard(IUmbracoBuilder builder)
    {
        // Add the dashboard and make sure it's authorized with the named policy above
        builder.Services.Configure<UmbracoPipelineOptions>(options =>
        {
            options.AddFilter(new UmbracoPipelineFilter(Constants.System.HangfireDashboard)
            {
                Endpoints = app => app.UseEndpoints(endpoints =>
                {
                    endpoints.MapHangfireDashboardWithAuthorizationPolicy(
                        pattern: Constants.System.Endpoint,
                        options: new DashboardOptions(),
                        authorizationPolicyName: AuthorizationPolicies.SectionAccessSettings);
                })
            });
        });
    }
}