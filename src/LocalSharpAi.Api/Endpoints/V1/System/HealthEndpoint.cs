using Microsoft.AspNetCore.Routing;

namespace LocalSharpAi.Api.Endpoints.V1.System;

public static class HealthEndpoints
{
    public static RouteGroupBuilder MapHealthEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/health", () => Results.Ok(new { status = "ok", version = "v1" }))
            .WithName("HealthV1")
            .WithTags("System")
            .WithDescription("Basic health probe for server readiness");
        return group;
    }
}
