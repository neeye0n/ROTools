using ItemDescTableModder.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ItemDescTableModder
{
    public interface IConfigLoader
    {
        Config Load();
    }

    public class ConfigLoader(ILogger<ConfigLoader> logger, string executableDirectory) : IConfigLoader
    {
        private readonly ILogger<ConfigLoader> _logger = logger;
        private readonly string _workingDirectory = executableDirectory;
        private readonly JsonSerializerSettings _jsonSerializerSettings = new()
        {
            ContractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            Formatting = Formatting.Indented,
        };

        public Config Load()
        {
            var configFileName = $"{typeof(Program).Namespace}.conf";
            string configPath = Path.Combine(_workingDirectory, configFileName);

            if (!File.Exists(configPath))
            {
                _logger.LogWarning("Config {configFileName} not found. Creating default config file.", configFileName);
                var defaultConfig = GenerateDefaultConfig();
                string json = JsonConvert.SerializeObject(defaultConfig, _jsonSerializerSettings);
                File.WriteAllText(configPath, json);
                return defaultConfig;
            }

            try
            {
                _logger.LogInformation("Loading config {configFileName}", configFileName);
                var json = File.ReadAllText(configPath);
                var config = JsonConvert.DeserializeObject<Config>(json, _jsonSerializerSettings);
                return config ?? GenerateDefaultConfig();
            }
            catch (Exception)
            {
                _logger.LogWarning("Invalid config file detected");
                _logger.LogWarning("Removing invalid config file");
                File.Delete(configPath);
                _logger.LogWarning("Tool is going to use default configurations");
                return GenerateDefaultConfig();
            }
        }

        private Config GenerateDefaultConfig()
        {
            return new Config
            {
                ResourceUrl = "https://neeye0n.github.io/flux/ItemDescTableModder/",
                EnableItemId = 0,
                ItemIdDescTextColor = "007ACC",
                ItemIdDescValueColor = "FFB300",
                BrewingConfig = new TaggingConfig
                {
                    EnableTags = 1,
                    EnableDescriptions = 1,
                    EnableDetailedDescriptions = 0,
                    TagText = "Brew",
                    HeaderText = "Brewing Ingredients",
                    DescriptionHeaderColor = "00897B",
                    DetailedDescriptionColor = "43A047"
                },
                CookingConfig = new TaggingConfig
                {
                    EnableTags = 1,
                    EnableDescriptions = 1,
                    EnableDetailedDescriptions = 0,
                    TagText = "Cook",
                    HeaderText = "Cooking Ingredients",
                    DescriptionHeaderColor = "EF6C00",
                    DetailedDescriptionColor = "6D4C41"
                },
                QuestConfig = new TaggingConfig
                {
                    EnableTags = 1,
                    EnableDescriptions = 1,
                    EnableDetailedDescriptions = 1,
                    TagText = "Quest",
                    HeaderText = "Quest Requirement",
                    DescriptionHeaderColor = "5E35B1",
                    DetailedDescriptionColor = "8E24AA"
                },
                InstanceConfig = new TaggingConfig
                {
                    EnableTags = 1,
                    EnableDescriptions = 1,
                    EnableDetailedDescriptions = 1,
                    TagText = "Instance",
                    HeaderText = "Instance Requirement",
                    DescriptionHeaderColor = "C62828",
                    DetailedDescriptionColor = "D84315"
                },
                PetEvoConfig = new TaggingConfig
                {
                    EnableTags = 1,
                    EnableDescriptions = 1,
                    EnableDetailedDescriptions = 1,
                    TagText = "Pet",
                    HeaderText = "Pet Evo Requirement",
                    DescriptionHeaderColor = "5C6BC0",
                    DetailedDescriptionColor = "7986CB"
                }
            };
        }
    }
}
