using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace LocalSharpAi.Api.Endpoints.V1.System;

public static class DebugEndpoints
{
    public static RouteGroupBuilder MapDebugEndpoints(this RouteGroupBuilder api)
    {
        var debugRoot = api.MapGroup("/_debug").WithGroupName("v1");

        debugRoot.MapGet("/endpoints", (Microsoft.AspNetCore.Routing.EndpointDataSource ds) =>
        {
            return Results.Ok(new
            {
                count = ds.Endpoints.Count,
                endpoints = ds.Endpoints.Select(e => new
                {
                    displayName = e.DisplayName,
                    route = (e as Microsoft.AspNetCore.Routing.RouteEndpoint)?.RoutePattern?.RawText,
                    metadata = e.Metadata.Select(m => m.GetType().FullName).ToArray()
                })
            });
        })
        .WithName("Debug_Endpoints")
        .WithTags("System")
        .WithDescription("Lists all registered endpoints in the application")
        .WithOpenApi();

        debugRoot.MapGet("/apidescriptions", (IApiDescriptionGroupCollectionProvider provider) =>
        {
            return Results.Ok(provider.ApiDescriptionGroups.Items.Select(g => new
            {
                groupName = g.GroupName,
                descriptions = g.Items.Select(d => new { d.RelativePath, d.HttpMethod, d.ActionDescriptor?.DisplayName })
            }));
        })
        .WithName("Debug_ApiDescriptions")
        .WithTags("System")
        .WithDescription("Lists all API descriptions known to the application")
        .WithOpenApi();

        return debugRoot;
    }
}
