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
        // Define API version set for minimal APIs
        var v1 = app.NewApiVersionSet()
            .HasApiVersion(new Asp.Versioning.ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var systemGroup = app
            .MapGroup("/api/v{version:apiVersion}/system")
            .WithApiVersionSet(v1)
            .MapToApiVersion(new Asp.Versioning.ApiVersion(1, 0))
            .WithGroupName("v1");

        systemGroup.MapHealthEndpoint();
        return app;
    }

    /// <summary>
    /// Map system endpoints under a provided '/api' root group
    /// </summary>
    /// <param name="api">The '/api' route group</param>
    /// <returns></returns>
    public static RouteGroupBuilder MapSystemEndpoints(this RouteGroupBuilder api)
    {
        // Define API version set for minimal APIs
        var v1 = api.NewApiVersionSet()
            .HasApiVersion(new Asp.Versioning.ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var systemGroup = api
            .MapGroup("/v{version:apiVersion}/system")
            .WithApiVersionSet(v1)
            .MapToApiVersion(new Asp.Versioning.ApiVersion(1, 0))
            .WithGroupName("v1");

        systemGroup.MapHealthEndpoint();
        return api;
    }
}
