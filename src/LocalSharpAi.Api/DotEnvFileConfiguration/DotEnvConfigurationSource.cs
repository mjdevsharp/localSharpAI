using Microsoft.Extensions.Configuration;

namespace Server.Api.Configuration;

/// <summary>
/// Configuration source for .env files.
/// </summary>
public class DotEnvConfigurationSource : IConfigurationSource
{
    public string FilePath { get; set; } = ".env";

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new DotEnvConfigurationProvider(FilePath);
    }
}
