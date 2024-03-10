namespace Cultiv.Hangfire
{
    public static class Constants
    {
        public static class System
        {
            public const string HangfireDashboard = nameof(HangfireDashboard);
            public const string Endpoint = "/umbraco/backoffice";
            public const string AlternativeConnectionStringName = "hangfireDB";
            public const string AuthenticationPolicyName = "Cultiv.Hangfire.AuthenticationPolicy";
        }
    }
}