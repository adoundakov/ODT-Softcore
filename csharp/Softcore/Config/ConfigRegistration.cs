using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using SPTarkov.Server.Core.DI;

namespace Softcore.Config;

/// <summary>
/// Loads config/config.json and registers the resulting <see cref="Configuration"/> as a singleton
/// so any class in the mod can take it as a constructor parameter.
/// No logger is available here (the container isn't built yet); <see cref="Plugin"/> reports
/// <see cref="Configuration.LoadedFromDisk"/> once it runs.
/// </summary>
public class ConfigRegistration : IOnDIConstruct
{
    public static async Task OnDIConstructAsync(IServiceCollection serviceCollection, CancellationToken cancellationToken)
    {
        serviceCollection.AddSingleton(await LoadAsync(cancellationToken));
    }

    private static async Task<Configuration> LoadAsync(CancellationToken cancellationToken)
    {
        var modDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            ?? throw new InvalidOperationException("Could not determine Softcore mod directory");
        var configPath = Path.Combine(modDirectory, "config", "config.json");

        if (!File.Exists(configPath))
        {
            return new Configuration { LoadedFromDisk = false, ConfigPath = configPath };
        }

        await using var stream = File.OpenRead(configPath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        var config = await JsonSerializer.DeserializeAsync<Configuration>(stream, options, cancellationToken)
            ?? new Configuration();
        config.LoadedFromDisk = true;
        config.ConfigPath = configPath;
        return config;
    }
}
