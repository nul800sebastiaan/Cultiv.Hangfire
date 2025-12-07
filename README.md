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

## Configuration

### Custom Queue Names

By default, Hangfire processes jobs from the `"default"` queue. For scenarios where multiple applications or sites share the same Hangfire database, you can configure specific queues for each server to process:

```json
{
  "Hangfire": {
    "Server": {
      "QueueNames": ["app1", "shared", "default"]
    }
  }
}
```

Each Hangfire server will only process jobs from its configured queues. Queues are processed in the order specified, so jobs in earlier queues have higher priority.

To enqueue jobs to a specific queue:

```csharp
BackgroundJob.Enqueue(() => DoWork(), new EnqueuedState("app1"));
```

If no queue names are configured, the server will process the `"default"` queue.

### Disable Hangfire Server

If you only want to access the Hangfire dashboard without running background job processing on a particular server:

```json
{
  "Hangfire": {
    "Server": {
      "Disabled": true
    }
  }
}
```

In the Umbraco backoffice it will look a little something like this:

![Screenshot of Cultiv.Hangfire installed in Umbraco](https://raw.githubusercontent.com/nul800sebastiaan/Cultiv.Hangfire/main/screenshot.png)
