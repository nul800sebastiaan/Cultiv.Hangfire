using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Server;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Cultiv.Hangfire;

public class OpenIddictServerEventsComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<OpenIddictServerEventsHandler>();

        // register event handlers with OpenIddict
        builder.Services.Configure<OpenIddictServerOptions>(options =>
        {
            options.Handlers.Add(
                OpenIddictServerHandlerDescriptor
                    .CreateBuilder<OpenIddictServerEvents.GenerateTokenContext>()
                    .UseSingletonHandler<OpenIddictServerEventsHandler>()
                    .Build()
            );
            options.Handlers.Add(
                OpenIddictServerHandlerDescriptor
                    .CreateBuilder<OpenIddictServerEvents.ApplyRevocationResponseContext>()
                    .UseSingletonHandler<OpenIddictServerEventsHandler>()
                    .Build()
            );
        });
    }
}