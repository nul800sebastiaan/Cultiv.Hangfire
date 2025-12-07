using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Api.Management.Routing;

namespace Cultiv.Hangfire;

[ApiController]
[VersionedApiBackOfficeRoute("cultiv-hangfire/config")]
[ApiVersion("1.0")]
public class HangfireConfigController : ManagementApiControllerBase
{
    private readonly HangfireSettings _settings;

    public HangfireConfigController(IOptions<HangfireSettings> settings)
    {
        _settings = settings.Value;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    public IActionResult GetConfig()
    {
        return Ok(new { useStandaloneSection = _settings.UseStandaloneSection });
    }
}
