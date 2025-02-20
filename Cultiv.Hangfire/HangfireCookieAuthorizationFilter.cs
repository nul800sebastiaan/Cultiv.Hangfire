using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication;

namespace Cultiv.Hangfire;

public class HangfireCookieAuthorizationFilter : IDashboardAsyncAuthorizationFilter
{
    public async Task<bool> AuthorizeAsync(DashboardContext context)
    {
        var result = await context.GetHttpContext().AuthenticateAsync(Constants.CultivHangfire.CookiesScheme);
        return result.Succeeded;
    }
}