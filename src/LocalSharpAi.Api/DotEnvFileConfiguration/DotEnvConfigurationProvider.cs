using Microsoft.Extensions.Configuration;

namespace Server.Api.Configuration;

/// <summary>
/// Configuration provider that loads environment variables from a .env file.
/// Supports standard .env format: KEY=value with comments (#) and empty lines.
/// </summary>
public class DotEnvConfigurationProvider : ConfigurationProvider
{
    private readonly string _filePath;

    public DotEnvConfigurationProvider(string filePath)
    {
        _filePath = filePath;
    }

    public override void Load()
    {
        if (!File.Exists(_filePath))
        {
            // .env file is optional - don't throw if missing
            return;
        }

        var lines = File.ReadAllLines(_filePath);
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith('#'))
            {
                continue;
            }

            // Parse KEY=value format
            var separatorIndex = trimmedLine.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue; // Skip malformed lines
            }

            var key = trimmedLine[..separatorIndex].Trim();
            var value = trimmedLine[(separatorIndex + 1)..].Trim();

            // Remove surrounding quotes if present
            if (value.Length >= 2 && 
                ((value.StartsWith('"') && value.EndsWith('"')) || 
                 (value.StartsWith('\'') && value.EndsWith('\''))))
            {
                value = value[1..^1];
            }

            // Replace __ with : for nested configuration (e.g., LLM__ApiKey -> LLM:ApiKey)
            var configKey = key.Replace("__", ":");

            Data[configKey] = value;
        }
    }
}
