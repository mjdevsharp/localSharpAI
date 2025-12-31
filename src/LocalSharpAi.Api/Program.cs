using LocalSharpAi.Api.Endpoints.V1.System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// API versioning (Asp.Versioning for minimal APIs)
builder.Services.AddApiVersioning(o =>
{
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    o.ReportApiVersions = true;
    o.ApiVersionReader = new Asp.Versioning.UrlSegmentApiVersionReader();
})
.AddApiExplorer(o =>
{
    o.GroupNameFormat = "'v'VVV"; // v1, v1.0
    o.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "LocalSharpAi.Api", Version = "v1" });
    options.DocInclusionPredicate((docName, apiDesc) => apiDesc.GroupName == null || apiDesc.GroupName == docName);
});

var app = builder.Build();

// Introduce '/api' root group
var api = app.MapGroup("/api");

app.UseHttpsRedirection();

// Configure the HTTP request pipeline.

// Temporary enable swagger for all deployments
//if (app.Environment.IsDevelopment())
{
    var provider = app.Services.GetRequiredService<Asp.Versioning.ApiExplorer.IApiVersionDescriptionProvider>();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach (var desc in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", desc.GroupName.ToUpperInvariant());
        }
    });

    // Map debug endpoints only in Development under '/api/_debug/*'
    api.MapDebugEndpoints();
}

// Map versioned system endpoints under '/api'
api.MapSystemEndpoints();

app.Run();
