using System.Collections.Generic;
using System.Diagnostics;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Semver;
using Umbraco.Extensions;

namespace Cultiv.Hangfire;

internal class ManifestFilter : IManifestFilter
{
    public void Filter(List<PackageManifest> manifests)
    {
        manifests.Add(new PackageManifest
        {
            PackageName = "Cultiv.Hangfire",
            Version = GetVersion(),
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
    
    private static string GetVersion()
    {
        var assembly = typeof(global::Cultiv.Hangfire.ManifestFilter).Assembly;
        try
        {
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.GetAssemblyFile().FullName);
            var productVersion = SemVersion.Parse(fileVersionInfo.ProductVersion);
            return productVersion.ToSemanticStringWithoutBuild();
        }
        catch
        {
            return assembly.GetName().Version.ToString(3);
        }
    }
}