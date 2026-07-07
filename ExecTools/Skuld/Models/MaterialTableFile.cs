using System.Text.Json.Serialization;

namespace Skuld.Models
{
    public class MaterialTableFile
    {
        [JsonPropertyName("schemaVersion")] public int SchemaVersion { get; init; }
        [JsonPropertyName("sourceName")] public string SourceName { get; init; } = "";
        [JsonPropertyName("description")] public string Description { get; init; } = "";
        [JsonPropertyName("display")] public CategoryDisplayConfig Display { get; init; } = new();
        [JsonPropertyName("entries")] public List<MaterialTableEntry> Entries { get; init; } = [];
    }

    public class CategoryDisplayConfig
    {
        [JsonPropertyName("enableTags")] public bool EnableTags { get; init; } = true;
        [JsonPropertyName("tagText")] public string TagText { get; init; } = "";
        [JsonPropertyName("isSuffix")] public bool IsSuffix { get; init; } = false;
        [JsonPropertyName("enableDescriptions")] public bool EnableDescriptions { get; init; } = true;
        [JsonPropertyName("headerText")] public string HeaderText { get; init; } = "";
        [JsonPropertyName("descriptionHeaderColor")] public string DescriptionHeaderColor { get; init; } = "000000";
        [JsonPropertyName("enableDetailedDescriptions")] public bool EnableDetailedDescriptions { get; init; } = false;
        [JsonPropertyName("detailedDescriptionColor")] public string DetailedDescriptionColor { get; init; } = "000000";
    }

    public class MaterialTableEntry
    {
        [JsonPropertyName("entryId")] public int EntryId { get; init; }
        [JsonPropertyName("label")] public string Label { get; init; } = "";
        [JsonPropertyName("alt-label")] public string? AltLabel { get; init; }
        [JsonPropertyName("materials")] public List<MaterialEntry> Materials { get; init; } = [];
    }

    public class MaterialEntry
    {
        [JsonPropertyName("matId")] public int MatId { get; init; }
        [JsonPropertyName("matName")] public string MatName { get; init; } = "";
        [JsonPropertyName("qty")] public int Qty { get; init; } = 1;
    }
}