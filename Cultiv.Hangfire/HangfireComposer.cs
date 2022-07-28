using System;
using System.Collections.Generic;
using Hangfire;
using Hangfire.Console;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Cultiv.Hangfire
{
    public class HangfireComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.ManifestFilters().Append<CultivHangfireManifestFilter>();

            var connectionString = GetConnectionString(builder);
            if (string.IsNullOrEmpty(connectionString))
            {
                // This might happen when the package is installed before Umbraco is installed
                // https://github.com/nul800sebastiaan/Cultiv.Hangfire/issues/11
                return;
            }
            
            // Configure Hangfire to use our current database and add the option to write console messages
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

        private static readonly List<string> AllowedSqlProviderNames = 
            new() { Umbraco.Cms.Persistence.SqlServer.Constants.ProviderName, "System.Data.SqlClient" };
        
        private static string GetConnectionString(IUmbracoBuilder builder)
        {
            var connectionString = builder.Config.GetUmbracoConnectionString(Constants.System.AlternativeConnectionStringName);
            if (string.IsNullOrWhiteSpace(connectionString) == false)
            {
                return connectionString;
            }
            
            var providerName = builder.Config.GetConnectionStringProviderName(Umbraco.Cms.Core.Constants.System.UmbracoConnectionName);
			if (providerName != null && AllowedSqlProviderNames.InvariantContains(providerName) == false)
			{
				throw new NotSupportedException($"Cultiv.Hangfire only works on providers `{string.Join("`, `", AllowedSqlProviderNames)}`, your current provider ({providerName}) is not supported.");
			}
            
            return builder.Config.GetUmbracoConnectionString();
        }
    }

    internal class CultivHangfireManifestFilter : IManifestFilter
    {
        public void Filter(List<PackageManifest> manifests)
        {
            manifests.Add(new PackageManifest
            {
                PackageName = "Cultiv.Hangfire",
                Dashboards = new[]
                {
                    new ManifestDashboard
                    {
                        Alias = "cultiv.Hangfire",
                        Sections = new[] { Umbraco.Cms.Core.Constants.Applications.Settings },
                        View = "/App_Plugins/Cultiv.Hangfire/dashboard.html"
                    }
                }
            });
        }
    }
}
