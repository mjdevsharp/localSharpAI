using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace LocalSharpAi.Api.Endpoints.V1.System;

public static class SystemGroupEndpoints
{
    /// <summary>
    /// Group system endpoints including health checks
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static WebApplication MapSystemEndpoints(this WebApplication app)
    {
        var systemGroup = app.MapGroup("/system");
        systemGroup.MapHealthEndpoint();
        return app;
    }
}
