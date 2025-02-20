namespace Cultiv.Hangfire
{
    public static class Constants
    {
        [Obsolete("Use Constants.Cultiv instead")]
        public static class System
        {
            [Obsolete("Use Constants.CultivHangfire.HangfireDashboard instead")]
            public const string HangfireDashboard = CultivHangfire.HangfireDashboard;
            [Obsolete("Use Constants.CultivHangfire.Endpoint instead")]
            public const string Endpoint = CultivHangfire.Endpoint;
            [Obsolete("Use Constants.CultivHangfire.AlternativeConnectionStringName instead")]
            public const string AlternativeConnectionStringName = CultivHangfire.AlternativeConnectionStringName;
            
            [Obsolete("Authentication policies are no longer used, moved to cookie authentication")]
            public const string AuthenticationPolicyName = "Cultiv.Hangfire.AuthenticationPolicy";
        }
        
        public static class CultivHangfire
        {
            public const string HangfireDashboard = nameof(HangfireDashboard);
            public const string Endpoint = "/umbraco/hangfire";
            public const string AlternativeConnectionStringName = "hangfireDB";
            
            public const string CookiesScheme = "Cultiv.Hangfire.CookieScheme";
            public const string CookieName = "Cultiv.Hangfire";
            public const string ClaimType = "HangfireAllowed";
        }
    }
}