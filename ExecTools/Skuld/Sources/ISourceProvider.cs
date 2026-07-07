using Skuld.Config;

namespace Skuld.Sources
{
    public interface ISourceProvider
    {
        bool CanHandle(CategorySource source);
        Task<string> FetchRawAsync(CategorySource source, CancellationToken ct);
    }
}