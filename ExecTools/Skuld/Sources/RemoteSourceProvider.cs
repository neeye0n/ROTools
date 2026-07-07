using Skuld.Config;

namespace Skuld.Sources
{
    public class RemoteSourceProvider(HttpClient http) : ISourceProvider
    {
        public bool CanHandle(CategorySource s) => s.SourceType == SourceType.ResourceUrl;

        public async Task<string> FetchRawAsync(CategorySource s, CancellationToken ct)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(15));

            return await http.GetStringAsync(s.Path, cts.Token);
        }
    }
}