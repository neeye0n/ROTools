using ItemDescTableModder.Models;
using ItemDescTableModder.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoonSharp.Interpreter;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ItemDescTableModder
{
    public interface IApp
    {
        Task ProcessFile(string filePath);
    }

    public partial class App : IApp
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<App> _logger;
        private readonly ILuaTableHandler _tableHandler;
        private readonly ILuaTableModifier _tableModifier;
        private readonly Config _config;
        private readonly string _workingDir;
        private readonly string _outputDirectory;

        private readonly Dictionary<string, string> _materialFiles = new()
        {
            { "Brewing", "brewingMatsTable.json" },
            { "Cooking", "cookingMatsTable.json" },
            { "Quest", "questMatsTable.json" },
            { "Instance", "instanceMatsTable.json" },
            { "PetEvo", "petEvoMatsTable.json" },
        };

        public App(IHttpClientFactory factory, ILogger<App> logger, ILuaTableHandler tableHandler, ILuaTableModifier tableModifier, string executableDirectory, IOptions<Config> config)
        {
            _httpClient = factory.CreateClient("ItemDescTableModder");
            _logger = logger;
            _tableHandler = tableHandler;
            _tableModifier = tableModifier;
            _outputDirectory = "System";
            _workingDir = executableDirectory;
            _config = config.Value;
        }

        public async Task ProcessFile(string filePath)
        {
            _logger.LogInformation("Starting to process the file...");
            try
            {
                var table = _tableHandler.LoadFile(filePath);
                if (table is null)
                {
                    _logger.LogInformation("File has already been processed. Aborting...");
                    return;
                }

                _logger.LogInformation("File has been loaded...");
                var allItemIds = _tableModifier.GetItemIds(table);

                // Load material tag mappings
                var materialTags = new Dictionary<string, Dictionary<int, string>>();
                foreach (var kvp in _materialFiles)
                {
                    materialTags[kvp.Key] = await GenerateMaterialTags(kvp.Value);
                }

                _logger.LogInformation("Applying Tags and Descriptions...");
                foreach (var itemId in allItemIds)
                {
                    _tableModifier.ModifyItem(table, itemId, item =>
                    {
                        var originalDisplayName = item.Get("identifiedDisplayName").String;
                        var update = BuildItemUpdate(itemId, originalDisplayName, materialTags);

                        item.Set("identifiedDisplayName", DynValue.NewString(update.DisplayName));

                        var descriptionTable = item.Get("identifiedDescriptionName").Table;
                        AddDescriptionsToTop(ref descriptionTable, update.Descriptions);
                    });
                }

                var fileName = Path.GetFileName(filePath);
                _tableHandler.SaveToFile(table, GetOutputFullPath(fileName));
                _logger.LogInformation("Done! Check the generated file in created System folder");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "An uxexpected error has occurred. The application will terminate.");
            }
        }

        private ItemUpdate BuildItemUpdate(int itemId, string originalDisplayName, Dictionary<string, Dictionary<int, string>> materialTags)
        {
            string updatedName = originalDisplayName?.Trim() ?? string.Empty;

            var tagStack = new List<string>();
            var newDescriptions = new List<DynValue>();
            if (_config.EnableItemId != 0)
            {
                newDescriptions.Add(DynValue.NewString($"^{_config.ItemIdDescTextColor}Item ID:^{_config.ItemIdDescValueColor} {itemId}^000000"));
            }

            bool hasMods = false;
            foreach (var materialType in _materialFiles.Keys)
            {
                var config = GetMaterialConfig(materialType);
                if (config is null) continue;

                if (materialTags[materialType].TryGetValue(itemId, out var materialInfo))
                {
                    hasMods = true;
                    // Collect tags for non-Instance materials
                    if (config.EnableTags != 0 && materialType != "Instance")
                    {
                        tagStack.Add($"[{config.TagText}]");
                    }

                    // Build descriptions
                    if (config.EnableDescriptions != 0)
                    {
                        newDescriptions.Add(DynValue.NewString($"^{config.DescriptionHeaderColor}[{config.HeaderText}]^000000"));

                        if (config.EnableDetailedDescriptions != 0)
                        {
                            foreach (var entry in materialInfo.Split("||"))
                            {
                                var parts = entry.Split("&&&", StringSplitOptions.TrimEntries);
                                if (parts.Length >= 2)
                                {
                                    var detailLine = $"^{config.DetailedDescriptionColor}{parts[0]} - Qty: {parts[1]}^000000";
                                    newDescriptions.Add(DynValue.NewString(detailLine));
                                }
                            }
                        }
                    }

                    // Special handling for Instance tags
                    if (config.EnableTags != 0 && materialType == "Instance")
                    {
                        var instanceTag = string.Join(", ", materialInfo.Split("||").Select(info =>
                        {
                            var parts = info.Split("&&&", StringSplitOptions.TrimEntries);
                            return $"{parts[0]} - {parts[1]}";
                        }));

                        updatedName = $"{updatedName} ({instanceTag})";
                    }
                }
            }

            // Prepend all collected tags in left-to-right order
            if (tagStack.Count > 0)
            {
                var joinedTags = string.Join("", tagStack);
                var startingSpace = TagTextRegEx().IsMatch(updatedName) ? "" : " ";
                updatedName = $"{joinedTags}{startingSpace}{updatedName}".Trim();
            }

            if (hasMods)
            {
                // Blank space after custom descriptions
                newDescriptions.Add(DynValue.NewString(" "));
            }

            return new ItemUpdate(updatedName, newDescriptions);
        }


        private TaggingConfig? GetMaterialConfig(string materialType)
        {
            return materialType switch
            {
                "Brewing" => _config.BrewingConfig,
                "Cooking" => _config.CookingConfig,
                "Quest" => _config.QuestConfig,
                "Instance" => _config.InstanceConfig,
                "PetEvo" => _config.PetEvoConfig,
                _ => null
            };
        }

        private async Task<Dictionary<int, string>> GenerateMaterialTags(string resourceName)
        {
            try
            {
                _logger.LogInformation("Reading from {address}", string.Concat(_httpClient.BaseAddress, resourceName));
                string json = await _httpClient.GetStringAsync(resourceName);
                var materialTable = JsonSerializer.Deserialize<Dictionary<string, List<MaterialInfo>>>(json);

                var materialTags = new Dictionary<int, string>();
                if (materialTable is not null)
                {
                    materialTags = materialTable
                        .SelectMany(kvp => kvp.Value.Select(item => new
                        {
                            Group = kvp.Key,
                            Item = item
                        }))
                        .GroupBy(x => x.Item.MaterialId)
                        .ToDictionary(
                            g => g.Key,
                            g => string.Join("||", g.Select(x => $"{x.Group}&&&{x.Item.Quantity}"))
                        );
                }

                return materialTags;
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to fetch json file '{resourceName}': {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException($"Invalid json format in file '{resourceName}': {ex.Message}", ex);
            }
        }

        private string GetOutputFullPath(string outputFileName)
        {
            var folderPath = Path.Combine(_workingDir, _outputDirectory);
            var fullFilePath = Path.Combine(folderPath, outputFileName);
            // Create the directory if it doesn't exist
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                _logger.LogInformation("System Directory created");
            }

            if (File.Exists(fullFilePath))
            {
                // Delete the existing file
                File.Delete(fullFilePath);
                _logger.LogInformation("Existing file deleted");
            }

            return fullFilePath;
        }

        private void AddDescriptionsToTop(ref Table descTable, List<DynValue> newDescList)
        {
            if (newDescList.Count == 0)
            {
                return;
            }

            // Add the existing descriptions to the new description list
            newDescList.AddRange(descTable.Values);

            // Clear the existing descriptions
            descTable.Clear();

            // ReApply the descriptions
            foreach (var desc in newDescList)
            {
                descTable.Append(desc);
            }
        }

        private record ItemUpdate(string DisplayName, List<DynValue> Descriptions);

        [GeneratedRegex(@"^(?:\[[^\]]*\])+")]
        private static partial Regex TagTextRegEx();
    }
}
