namespace Cultiv.Hangfire
{
    public static class Constants
    {        
        public static class CultivHangfire
        {
            public const string HangfireDashboard = nameof(HangfireDashboard);
            public const string Endpoint = "/cultiv/hangfire";
            public const string AlternativeConnectionStringName = "hangfireDB";
            
            public const string CookiesScheme = "Cultiv.Hangfire.CookieScheme";
            public const string CookieName = "Cultiv.Hangfire";
            public const string ClaimType = "HangfireAllowed";
        }
    }
}