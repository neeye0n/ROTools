using Skuld.Config;

namespace Skuld.Sources
{
    public class LocalFileSourceProvider : ISourceProvider
    {
        public bool CanHandle(CategorySource s) => s.SourceType == SourceType.LocalFile;

        public Task<string> FetchRawAsync(CategorySource s, CancellationToken ct)
            => File.ReadAllTextAsync(s.Path, ct);
    }
}