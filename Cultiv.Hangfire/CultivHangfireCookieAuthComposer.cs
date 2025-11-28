using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;

namespace Cultiv.Hangfire;

public class CultivHangfireCookieAuthComposer : IComposer
{
    // configure a custom authentication scheme (cookie based)
    public void Compose(IUmbracoBuilder builder)
        => builder.Services
            .AddAuthentication()
            .AddCookie(
                Constants.CultivHangfire.CookiesScheme,
                options =>
                {
                    options.Cookie.Name = Constants.CultivHangfire.CookieName;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.SlidingExpiration = true;
                });
}