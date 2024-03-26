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

## Umbraco Cloud
Note, if you are using Umbraco Cloud, you will need to add a connection string to your `appsettings.json` file. Without this, you will get errors. 
Simply put the following in to your `appsettings.json` file.

```
 "umbracoDbDSN": "Data Source=|DataDirectory|/Umbraco.sqlite.db;Cache=Private;Foreign Keys=True;Pooling=True",
    "umbracoDbDSN_ProviderName": "Microsoft.Data.SQLite"
```
The important thing to note is that the key for the connection string should be "umbracoDbDSN". It is also important to know that this section is outside the Umbraco.CMS section, and is therefore in the root of the config.
