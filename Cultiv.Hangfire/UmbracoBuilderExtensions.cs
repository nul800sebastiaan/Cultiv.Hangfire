using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.Membership;
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
                            options: new DashboardOptions { IgnoreAntiforgeryToken = true });
                })
            });
        });
    }
}