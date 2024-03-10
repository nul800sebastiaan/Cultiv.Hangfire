using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Extensions;

namespace Cultiv.Hangfire;

public static class UmbracoBuilderExtensions
{
    public static string? GetConnectionString(this IUmbracoBuilder builder)
    {
        var connectionString =
            builder.Config.GetUmbracoConnectionString(Constants.System.AlternativeConnectionStringName);
        return string.IsNullOrWhiteSpace(connectionString) ? builder.Config.GetUmbracoConnectionString() : connectionString;
    }
    
    internal static void AddHangfireToUmbraco(this IUmbracoBuilder builder, bool serverDisabled)
    {
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(Constants.System.AuthenticationPolicyName, policy =>
            {
                policy
                    .AddAuthenticationSchemes(Umbraco.Cms.Core.Constants.Security.BackOfficeAuthenticationType)
                    .RequireClaim(Umbraco.Cms.Core.Constants.Security.AllowedApplicationsClaimType, Umbraco.Cms.Core.Constants.Applications.Settings)
                    .RequireAuthenticatedUser();
            });

        if (!serverDisabled)
        {
            builder.Services.AddHangfireServer();
        }

        builder.Services.Configure<UmbracoPipelineOptions>(options =>
        {
            options.AddFilter(new UmbracoPipelineFilter(Constants.System.HangfireDashboard)
            {
                Endpoints = app => app.UseEndpoints(endpoints =>
                {
                    endpoints.MapHangfireDashboard(
                            pattern: Constants.System.Endpoint,
                            options: new DashboardOptions { IgnoreAntiforgeryToken = true })
                        .RequireAuthorization(Constants.System.AuthenticationPolicyName);
                })
            });
        });
    }
}