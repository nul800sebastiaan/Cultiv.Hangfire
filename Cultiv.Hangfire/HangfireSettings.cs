namespace Cultiv.Hangfire;

public class HangfireSettings
{
    public Server? Server { get; set; }
}

public class Server
{
    public bool? Disabled { get; set; } 
}