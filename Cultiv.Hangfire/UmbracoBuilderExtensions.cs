using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Cultiv.Hangfire;

public static class UmbracoBuilderExtensions
{
    public static string? GetConnectionString(this IUmbracoBuilder builder)
    {
        var connectionString =
            builder.Config.GetUmbracoConnectionString(Constants.CultivHangfire.AlternativeConnectionStringName);
        return string.IsNullOrWhiteSpace(connectionString) ? builder.Config.GetUmbracoConnectionString() : connectionString;
    }
    
    internal static void AddHangfireToUmbraco(this IUmbracoBuilder builder, bool serverDisabled, string[] queueNames)
    {
        if (!serverDisabled)
        {
            builder.Services.AddHangfireServer(options =>
            {
                options.Queues = queueNames;
            });
        }
        else
        {
            return;
        }

        builder.Services.Configure<UmbracoPipelineOptions>(options =>
        {
            options.AddFilter(new UmbracoPipelineFilter(Constants.CultivHangfire.HangfireDashboard)
            {
                Endpoints = app => app.UseEndpoints(endpoints =>
                {
                    endpoints.MapHangfireDashboard(
                        pattern: Constants.CultivHangfire.Endpoint,
                        options: new DashboardOptions
                        {
                            AsyncAuthorization = [ new HangfireCookieAuthorizationFilter() ]
                        });
                })
            });
        });
    }
}