using Skuld.Config;
using Skuld.Diagnostics;
using Skuld.Models;
using System.Text.Json;

namespace Skuld.Sources
{
    public class CategoryDataFetcher(IEnumerable<ISourceProvider> providers, IAppNotifier notifier)
    {
        public async Task<MaterialTableFile?> TryFetchAsync(CategorySource source, CancellationToken ct)
        {
            var label = source.Path;
            try
            {
                var provider = providers.FirstOrDefault(p => p.CanHandle(source))
                    ?? throw new InvalidOperationException($"No provider for source kind '{source.SourceType}'.");

                var raw = await provider.FetchRawAsync(source, ct);

                SchemaValidator.Validate(raw, label);

                var table = JsonSerializer.Deserialize<MaterialTableFile>(raw)
                    ?? throw new InvalidDataException("Deserialized to null.");

                label = string.IsNullOrWhiteSpace(table.SourceName) ? source.Path : table.SourceName;

                MaterialTableValidator.Validate(table, label);
                return table;
            }
            catch (Exception ex)
            {
                notifier.Error($"Error loading '{label}': {ex.Message}");
                return null;
            }
        }
    }
}