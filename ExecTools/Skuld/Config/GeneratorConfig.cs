using System.Text.Json.Serialization;

namespace Skuld.Config
{
    public enum SourceType { ResourceUrl, LocalFile }

    public class GeneratorConfig
    {
        [JsonPropertyName("sources")]
        public List<CategorySource> Sources { get; set; } = DefaultSources();

        private static List<CategorySource> DefaultSources() =>
        [
            new()
            {
                SourceType = SourceType.ResourceUrl,
                Path = "https://neeye0n.github.io/flux/skuld/brewing.json"
            },
            new()
            {
                SourceType = SourceType.ResourceUrl,
                Path = "https://neeye0n.github.io/flux/skuld/cooking.json"
            },
            new()
            {
                SourceType = SourceType.ResourceUrl,
                Path = "https://neeye0n.github.io/flux/skuld/crafterQuest.json"
            },
            new()
            {
                SourceType = SourceType.ResourceUrl,
                Path = "https://neeye0n.github.io/flux/skuld/expTurnInQuest.json"
            },
            new()
            {
                SourceType = SourceType.ResourceUrl,
                Path = "https://neeye0n.github.io/flux/skuld/instances.json"
            },
            new()
            {
                SourceType = SourceType.ResourceUrl,
                Path = "https://neeye0n.github.io/flux/skuld/petEvolution.json"
            }
        ];
    }

    public class CategorySource
    {
        [JsonPropertyName("sourceType")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SourceType SourceType { get; set; } = SourceType.ResourceUrl;
        [JsonPropertyName("path")] public string Path { get; set; } = "";
    }
}