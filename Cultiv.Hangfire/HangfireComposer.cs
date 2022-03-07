using System;
using Hangfire;
using Hangfire.Console;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Cms.Web.Common.Authorization;

namespace Cultiv.Hangfire
{
    public class HangfireComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            // Configure Hangfire to use our current database and add the option to write console messages
            var connectionString = builder.Config.GetConnectionString(Umbraco.Cms.Core.Constants.System.UmbracoConnectionName);
            if (string.IsNullOrEmpty(connectionString) == false)
            {
                builder.Services.AddHangfire(configuration =>
                {
                    configuration
                        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                        .UseSimpleAssemblyNameTypeSerializer()
                        .UseRecommendedSerializerSettings()
                        .UseConsole()
                        .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
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
                JobStorage.Current = new SqlServerStorage(connectionString);
            }
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
                                pattern: "/umbraco/backoffice/hangfire",
                                options: new DashboardOptions(),
                                authorizationPolicyName: AuthorizationPolicies.SectionAccessSettings);
                    }).UseHangfireDashboard()
                });
            });
        }
    }
}
