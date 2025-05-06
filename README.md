# Cultiv.Hangfire &middot; [![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE) [![NuGet version (Cultiv.Hangfire)](https://img.shields.io/nuget/v/Cultiv.Hangfire.svg)](https://www.nuget.org/packages/Cultiv.Hangfire/) [![Twitter](https://img.shields.io/twitter/follow/cultiv.svg?style=social&label=Follow)](https://twitter.com/intent/follow?screen_name=cultiv)

Hangfire dashboard for Umbraco

This installs Hangfire and a dashboard in Umbraco, the dashboard is secured and is only available for users with access to the Settings section of Umbraco.

After installing this, you can add a `Composer` to start running scheduled tasks, for example:

```csharp
using System.Threading;
using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace MyNamespace
{
    public class Composer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {            
            RecurringJob.AddOrUpdate(() => DoIt(null), Cron.Hourly());
        }
        
        public void DoIt(PerformContext context)
        {
            var progressBar =  context.WriteProgressBar();
            var items = new int[10]{ 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 };

            foreach (var item in items.WithProgress(progressBar, items.Length))
            {
                context.WriteLine($"Number: {item}");
                Thread.Sleep(1000);
            }
        }
    }
}
```

In the Umbraco backoffice it will look a little something like this:

![Screenshot of Cultiv.Hangfire installed in Umbraco](https://raw.githubusercontent.com/nul800sebastiaan/Cultiv.Hangfire/main/screenshot.png)

## Notes for specific versions this pacakge and Umbraco

- Version 4+ of this package works with Umbraco versions 14 and above
- Version 3 and below works with Umbraco 13 and below

### Umbraco 8

- You might need to change your `web.config` to make sure the `~/hangfire` path is ignored: `<add key="Umbraco.Core.ReservedPaths" value="~/hangfire" />`

### Umbraco 13

- You might need to add the following above your `IComposer`s that schedule the jobs: `[ComposeAfter(typeof(HangfireComposer))]` ([see this issue](https://github.com/nul800sebastiaan/Cultiv.Hangfire/issues/3#issuecomment-2823912302))
 
