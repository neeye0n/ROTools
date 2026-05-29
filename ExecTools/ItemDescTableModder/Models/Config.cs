using Newtonsoft.Json;

namespace ItemDescTableModder.Models
{
    [method: JsonConstructor]
    public class Config()
    {
        [JsonProperty(Required = Required.Always)]
        public required string ResourceUrl { get; set; }
        [JsonProperty(Required = Required.Always)]
        public required int EnableItemId { get; set; }
        [JsonProperty(Required = Required.Always)]
        public required string ItemIdDescTextColor { get; set; }
        [JsonProperty(Required = Required.Always)]
        public required string ItemIdDescValueColor { get; set; }
        [JsonProperty(Required = Required.Always)]
        public required TaggingConfig BrewingConfig { get; set; }
        [JsonProperty(Required = Required.Always)]
        public required TaggingConfig CookingConfig { get; set; }
        [JsonProperty(Required = Required.Always)]
        public required TaggingConfig QuestConfig { get; set; }
        [JsonProperty(Required = Required.Always)]
        public required TaggingConfig InstanceConfig { get; set; }
        [JsonProperty(Required = Required.Always)]
        public required TaggingConfig PetEvoConfig { get; set; }
    }

    public class TaggingConfig
    {
        public required int EnableTags { get; set; }
        public required int EnableDescriptions { get; set; }
        public required int EnableDetailedDescriptions { get; set; }
        public required string TagText { get; set; }
        public required string HeaderText { get; set; }
        public required string DescriptionHeaderColor { get; set; }
        public required string DetailedDescriptionColor { get; set; }

    }
}
