using Microsoft.Extensions.Configuration;

namespace Server.Api.Configuration;

/// <summary>
/// Extension methods for adding .env file configuration.
/// </summary>
public static class DotEnvConfigurationExtensions
{
    /// <summary>
    /// Adds the .env file as a configuration source.
    /// </summary>
    /// <param name="builder">The configuration builder.</param>
    /// <param name="filePath">Path to the .env file (default: ".env" in project root).</param>
    /// <returns>The configuration builder for chaining.</returns>
    public static IConfigurationBuilder AddDotEnvFile(
        this IConfigurationBuilder builder, 
        string filePath = ".env")
    {
        return builder.Add(new DotEnvConfigurationSource { FilePath = filePath });
    }
}
