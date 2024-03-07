using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Hangfire.Storage.SQLite;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Cultiv.Hangfire;

public class HangfireComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
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
        
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("Cultiv.Hangfire", policy =>
            {
                policy
                    .AddAuthenticationSchemes(Umbraco.Cms.Core.Constants.Security.BackOfficeAuthenticationType)
                    .RequireClaim(Umbraco.Cms.Core.Constants.Security.AllowedApplicationsClaimType, Umbraco.Cms.Core.Constants.Applications.Settings)
                    .RequireAuthenticatedUser();
            });
        builder.Services.AddHangfireServer();
        
        // Add the dashboard and make sure it's authorized with the named policy above
        builder.Services.Configure<UmbracoPipelineOptions>(options =>
        {
            options.AddFilter(new UmbracoPipelineFilter("HangfireDashboard")
            {
                Endpoints = app => app.UseEndpoints(endpoints =>
                {
                    endpoints.MapHangfireDashboard(
                            pattern: "/umbraco/hangfire",
                            options: new DashboardOptions { IgnoreAntiforgeryToken = true })
                        .RequireAuthorization("Cultiv.Hangfire");
                })
            });
        });
    }
}