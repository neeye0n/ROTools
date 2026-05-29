using System.Text.Json.Serialization;

namespace Skuld
{
    public class GeneratorConfig
    {
        [JsonPropertyName("resourceUrl")]
        public string ResourceUrl { get; set; } = "https://neeye0n.github.io/flux/ItemDescTableModder/";
        [JsonPropertyName("brewingConfig")]
        public CategoryJsonConfig BrewingConfig { get; set; } = new("Brew", "Brewing Material", "00897B", "43A047");

        [JsonPropertyName("cookingConfig")]
        public CategoryJsonConfig CookingConfig { get; set; } = new("Cook", "Cooking Ingredient", "EF6C00", "6D4C41");

        [JsonPropertyName("questConfig")]
        public CategoryJsonConfig QuestConfig { get; set; } = new("Quest", "Quest Item", "5E35B1", "8E24AA", enableDetailed: 1);

        [JsonPropertyName("instanceConfig")]
        public CategoryJsonConfig InstanceConfig { get; set; } = new("Inst", "Instance Requirement", "C62828", "D84315", enableDetailed: 1);

        [JsonPropertyName("petEvoConfig")]
        public CategoryJsonConfig PetEvoConfig { get; set; } = new("Pet", "Pet Evolution", "5C6BC0", "7986CB", enableDetailed: 1);
    }

    public class CategoryJsonConfig
    {
        public CategoryJsonConfig() { }

        public CategoryJsonConfig(string tagText, string headerText,
            string descHeaderColor, string detailedDescColor,
            int enableTags = 1, int enableDesc = 1, int enableDetailed = 0)
        {
            TagText = tagText;
            HeaderText = headerText;
            DescriptionHeaderColor = descHeaderColor;
            DetailedDescriptionColor = detailedDescColor;
            EnableTags = enableTags;
            EnableDescriptions = enableDesc;
            EnableDetailedDescriptions = enableDetailed;
        }

        [JsonPropertyName("enableTags")]
        public int EnableTags { get; set; } = 1;

        [JsonPropertyName("enableDescriptions")]
        public int EnableDescriptions { get; set; } = 1;

        [JsonPropertyName("enableDetailedDescriptions")]
        public int EnableDetailedDescriptions { get; set; } = 0;

        [JsonPropertyName("tagText")]
        public string TagText { get; set; } = "";

        [JsonPropertyName("headerText")]
        public string HeaderText { get; set; } = "";

        [JsonPropertyName("descriptionHeaderColor")]
        public string DescriptionHeaderColor { get; set; } = "";

        [JsonPropertyName("detailedDescriptionColor")]
        public string DetailedDescriptionColor { get; set; } = "";
    }
}
