using System.Collections.Generic;
using Umbraco.Cms.Core.Manifest;

namespace Cultiv.Hangfire;

internal class ManifestFilter : IManifestFilter
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