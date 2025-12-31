using Microsoft.AspNetCore.Routing;

namespace LocalSharpAi.Api.Endpoints.V1.System;

public static class HealthEndpoints
{
    public static RouteGroupBuilder MapHealthEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/health", (Microsoft.AspNetCore.Http.HttpRequest req) =>
        {
            var version = req.RouteValues["version"]?.ToString() ?? "v1";
            return Results.Ok(new { status = "ok", version = version });
        })
            .WithName("Health")
            .WithTags("System")
            .WithDescription("Basic health probe for server readiness")
            .WithOpenApi();
        return group;
    }
}
