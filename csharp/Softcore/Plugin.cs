using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Utils;
using System.Reflection;
using System.Text.Json;
using Softcore.Config;
using Softcore.Changers;

namespace Softcore;

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class Plugin : IOnLoad
{
    private readonly ISptLogger<Plugin> _logger;
    private readonly EconomyOptionsChanger _economyChanger;
    private Configuration? _config;

    public Plugin(
        ISptLogger<Plugin> logger,
        EconomyOptionsChanger economyChanger)
    {
        _logger = logger;
        _economyChanger = economyChanger;
    }

    public async Task OnLoad()
    {
        // Load configuration
        _config = await LoadConfiguration();

        if (_config == null || !_config.General.Enabled)
        {
            _logger.Warning("[Softcore] Mod is disabled in config");
            return;
        }

        _logger.Success("[Softcore] Configuration loaded successfully");
        _logger.Info($"[Softcore] Economy enabled: {_config.EconomyOptions.Enabled}");
        _logger.Info($"[Softcore] Barter economy enabled: {_config.EconomyOptions.BarterEconomy.Enabled}");

        // Apply economy changes
        _economyChanger.Apply(_config.EconomyOptions);

        _logger.Success("[Softcore] All changes applied successfully");
    }

    private async Task<Configuration?> LoadConfiguration()
    {
        try
        {
            // Get the mod's directory from the assembly location
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var modDirectory = Path.GetDirectoryName(assemblyLocation);
            var configPath = Path.Combine(modDirectory!, "config", "config.json");

            if (!File.Exists(configPath))
            {
                _logger.Error($"[Softcore] Config file not found at: {configPath}");
                return CreateDefaultConfig();
            }

            var jsonString = await File.ReadAllTextAsync(configPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            return JsonSerializer.Deserialize<Configuration>(jsonString, options);
        }
        catch (Exception ex)
        {
            _logger.Error($"[Softcore] Failed to load config: {ex.Message}");
            _logger.Info("[Softcore] Using default configuration");
            return CreateDefaultConfig();
        }
    }

    private Configuration CreateDefaultConfig()
    {
        return new Configuration();  // Uses default values from class initializers
    }
}
