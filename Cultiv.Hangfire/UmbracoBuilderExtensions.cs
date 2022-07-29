using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Cultiv.Hangfire;

public static class UmbracoBuilderExtensions
{
    public static string GetConnectionString(this IUmbracoBuilder builder)
    {
        var connectionString =
            builder.Config.GetUmbracoConnectionString(Constants.System.AlternativeConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString) == false)
        {
            return connectionString;
        }

        var providerName =
            builder.Config.GetConnectionStringProviderName(Umbraco.Cms.Core.Constants.System.UmbracoConnectionName);
        if (providerName != null && AllowedSqlProviderNames.InvariantContains(providerName) == false)
        {
            throw new NotSupportedException(
                $"Cultiv.Hangfire only works on providers `{string.Join("`, `", AllowedSqlProviderNames)}`, your current provider ({providerName}) is not supported.");
        }

        return builder.Config.GetUmbracoConnectionString();
    }


    private static readonly List<string> AllowedSqlProviderNames =
        new() { Umbraco.Cms.Persistence.SqlServer.Constants.ProviderName, "System.Data.SqlClient" };
}