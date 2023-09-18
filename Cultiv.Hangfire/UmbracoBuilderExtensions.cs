using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Cultiv.Hangfire;

public static class UmbracoBuilderExtensions
{
    public static string GetConnectionString(this IUmbracoBuilder builder)
    {
        var connectionString =
            builder.Config.GetUmbracoConnectionString(Constants.System.AlternativeConnectionStringName);
        return string.IsNullOrWhiteSpace(connectionString) == false ? connectionString : builder.Config.GetUmbracoConnectionString();
    }
}