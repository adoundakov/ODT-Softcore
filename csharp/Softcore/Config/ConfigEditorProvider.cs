using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Web.Models.Configs;
using SPTarkov.Server.Web.Services;

namespace Softcore.Config;

/// <summary>
/// Registers config.json with the server dashboard's config editor so it can be viewed and edited
/// there. Edit-only: the dashboard exists only while the server runs, so every edit is post-load —
/// flea offers are already generated and our changers are not idempotent. Flow is edit → save →
/// restart. ApplyToRuntimeAsync is left at its default (property copy into the injected singleton,
/// harmless); the callback only reminds the user to restart.
/// </summary>
[Injectable(InjectionType.Singleton)]
public class ConfigEditorProvider(ISptLogger<ConfigEditorProvider> logger, Configuration config) : IConfigEditorConfigProvider
{
    public IEnumerable<ConfigEditorConfigRegistration> GetConfigs()
    {
        var metadata = new ModMetadata();
        yield return ConfigEditorConfigRegistration.Create(
            metadata.ModGuid,
            $"{metadata.Name} (restart server to apply)",
            config,
            config.ConfigPath
        ) with
        {
            OnAppliedToRuntimeAsync = OnAppliedAsync
        };
    }

    private ValueTask OnAppliedAsync(object modifiedConfig, CancellationToken cancellationToken)
    {
        logger.Warning("[Softcore] Config saved. Restart the server to apply changes.");
        return ValueTask.CompletedTask;
    }
}
