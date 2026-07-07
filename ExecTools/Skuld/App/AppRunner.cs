using Skuld.Annotations;
using Skuld.Config;
using Skuld.Diagnostics;
using Skuld.Models;
using Skuld.Packaging;
using Skuld.Sources;
using System.Text;

namespace Skuld.App
{
    public class AppRunner(
        ConfigService configService,
        CategoryDataFetcher fetcher,
        IAppNotifier notifier)
    {
        private const string ConfigFile = "skuldConf.json";
        private const string ItemInfoLua = "itemInfo_f.lua";
        private const string SchemaFile = "materialTable.schema.json";

        public async Task RunAsync()
        {
            var assembly = typeof(AppRunner).Assembly;
            var itemInfoFile = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith(ItemInfoLua, StringComparison.OrdinalIgnoreCase));
            var schema = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith(SchemaFile, StringComparison.OrdinalIgnoreCase));

            if (itemInfoFile is null || schema is null)
            {
                notifier.Error("Embedded resource file not found. Terminating...");
                return;
            }

            var configPath = Path.Combine(AppContext.BaseDirectory, ConfigFile);
            var config = await configService.LoadOrCreateDefaultAsync(configPath);
            if (config is null) return;

            var loaded = await notifier.StatusAsync("🔃 Fetching sources...", async ctx =>
            {
                var results = new List<(CategorySource, MaterialTableFile)>();
                foreach (var source in config.Sources)
                {
                    ctx.SetStatus($"🔍 Reading {source.Path}...");
                    var table = await fetcher.TryFetchAsync(source, CancellationToken.None);
                    if (table is not null) results.Add((source, table));
                    Task.Delay(300).Wait();
                }
                return results;
            });

            if (loaded.Count == 0)
            {
                notifier.Error("No categories loaded successfully. Nothing to generate.");
                return;
            }

            var annotations = AnnotationBuilder.Build(loaded);
            var lua = LuaWriter.Build(annotations);

            Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var win1252 = Encoding.GetEncoding(1252);

            using var resourceStream = assembly.GetManifestResourceStream(itemInfoFile)!;
            var zipBytes = await PackageBuilder.BuildAsync(lua, resourceStream, win1252);

            var zipPath = Path.Combine(AppContext.BaseDirectory, "System.zip");
            if (File.Exists(zipPath)) File.Delete(zipPath);
            await File.WriteAllBytesAsync(zipPath, zipBytes);

            notifier.Success($"Written to: {zipPath}");
            notifier.Success($"Total items annotated: {annotations.Count}");
        }
    }
}