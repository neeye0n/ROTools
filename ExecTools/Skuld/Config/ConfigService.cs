using Skuld.Diagnostics;
using System.Text.Json;

namespace Skuld.Config
{
    public class ConfigService(IAppNotifier notifier)
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public async Task<GeneratorConfig?> LoadOrCreateDefaultAsync(string configPath)
        {
            if (!File.Exists(configPath))
            {
                await WriteDefaultAsync(configPath);
                return null;
            }
            notifier.Info($"Reading {configPath}");
            GeneratorConfig? config;
            try
            {
                var json = await File.ReadAllTextAsync(configPath);
                config = JsonSerializer.Deserialize<GeneratorConfig>(json, JsonOptions);
            }
            catch (JsonException ex)
            {
                notifier.Warn($"Config file is corrupted: {ex.Message}");
                await BackupAndRecreateAsync(configPath);
                return null;
            }

            if (config?.Sources is null || config.Sources.Count == 0)
            {
                notifier.Warn("Config is empty or malformed.");
                await BackupAndRecreateAsync(configPath);
                return null;
            }

            var (valid, dropped) = ConfigValidator.Validate(config);

            foreach (var (src, errors) in dropped)
                notifier.Warn($"Dropping source '{src.Path}': {string.Join("; ", errors)}");

            if (valid.Count == 0)
            {
                notifier.Error("No valid sources remain after validation.");
                await BackupAndRecreateAsync(configPath);
                return null;
            }

            config.Sources = valid;
            if (dropped.Count != 0)
            {
                notifier.Warn($"{dropped.Count} resources dropped.");
            }
            notifier.Success($"Found {valid.Count} sources.");
            return config;
        }

        private async Task WriteDefaultAsync(string configPath)
        {
            var defaultConfig = new GeneratorConfig();
            await File.WriteAllTextAsync(configPath, JsonSerializer.Serialize(defaultConfig, JsonOptions));
            notifier.Warn($"Created default config at: {configPath}");
            notifier.Warn("Edit it if needed, then re-run the app.");
        }

        private async Task BackupAndRecreateAsync(string configPath)
        {
            var backupPath = configPath + $".broken-{DateTime.Now:yyyyMMdd-HHmmss}";
            File.Move(configPath, backupPath, overwrite: true);
            notifier.Warn($"Backed up broken config to: {backupPath}");
            await WriteDefaultAsync(configPath);
        }
    }
}