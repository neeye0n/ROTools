using System.Text;
using System.Text.Json;

namespace ItemDescAnnotationTableGenerator
{
    internal class Program
    {
        const string ColorReset = "^000000";

        static async Task Main()
        {
            Console.WriteLine("ItemDescAnnotationTableGenerator");
            Console.WriteLine("=================================");

            // ── Load generatorConfig.json ─────────────────────────────────────
            string configPath = Path.Combine(AppContext.BaseDirectory, "generatorConfig.json");
            if (!File.Exists(configPath))
            {
                var defaultConfig = new GeneratorConfig();
                await File.WriteAllTextAsync(configPath, JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true }));
                Console.WriteLine($"Created default generatorConfig.json at:\n  {configPath}");
                Console.WriteLine("Please update resourceUrl and re-run.");
                return;
            }

            GeneratorConfig config;
            try
            {
                var configJson = await File.ReadAllTextAsync(configPath);
                config = JsonSerializer.Deserialize<GeneratorConfig>(configJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading generatorConfig.json: {ex.Message}");
                return;
            }

            Console.WriteLine($"Resource URL  : {config.ResourceUrl}");
            Console.WriteLine();

            // ── Build CategoryConfig list from JSON config ─────────────────────
            var Categories = new List<CategoryConfig>
            {
                CategoryConfig.From("brewingMatsTable.json",  config.BrewingConfig,  isSuffix: false),
                CategoryConfig.From("cookingMatsTable.json",  config.CookingConfig,  isSuffix: false),
                CategoryConfig.From("questMatsTable.json",    config.QuestConfig,    isSuffix: false),
                CategoryConfig.From("instanceMatsTable.json", config.InstanceConfig, isSuffix: true),
                CategoryConfig.From("petEvoMatsTable.json",   config.PetEvoConfig,   isSuffix: false),
            };

            using var http = new HttpClient();

            // ── Fetch and arrange resource data ──────────────────────────────
            var annotations = new Dictionary<int, List<Usage>>();

            foreach (var cat in Categories)
            {
                Console.WriteLine($"Fetching {cat.File}...");
                try
                {
                    var json = await http.GetStringAsync(config.ResourceUrl + cat.File);
                    var table = JsonSerializer.Deserialize<Dictionary<string, JsonElement[]>>(json)!;

                    int count = 0;
                    foreach (var (productName, materials) in table)
                    {
                        foreach (var mat in materials)
                        {
                            if (!mat.TryGetProperty("matId", out var matIdEl)) continue;
                            int matId = matIdEl.GetInt32();
                            int qty = mat.TryGetProperty("qty", out var qtyEl) ? qtyEl.GetInt32() : 1;

                            if (!annotations.ContainsKey(matId))
                                annotations[matId] = [];

                            annotations[matId].Add(new Usage(cat, productName, qty));
                            count++;
                        }
                    }
                    Console.WriteLine($"  Loaded {table.Count} recipes, {count} material entries.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Error loading {cat.File}: {ex.Message}");
                }
            }

            // ── Build itemAnnotations.lua ─────────────────────────────────────
            Console.WriteLine("\nGenerating itemAnnotations.lua...");
            var sb = new StringBuilder();

            sb.AppendLine("-- Processed By ItemDescTableModder.");
            sb.AppendLine();
            sb.AppendLine("itemAnnotations = {");

            foreach (var (matId, usages) in annotations)
            {
                // ── Name tags ─────────────────────────────────────────────────
                var prefixTags = new List<string>();
                var suffixParts = new List<string>();

                foreach (var u in usages)
                {
                    if (!u.Category.EnableTags) continue;

                    if (u.Category.IsSuffix)
                        // ── Custom Instance Tags ─────────────────────────────────────────────────
                        suffixParts.Add($"{u.ProductName}-{u.Qty}");
                    else if (!prefixTags.Contains(u.Category.Tag))
                        prefixTags.Add(u.Category.Tag);
                }

                string nameTagPrefix = string.Join("", prefixTags);
                // ── Custom Instance Tags ─────────────────────────────────────────────────
                string nameTagSuffix = suffixParts.Count > 0
                    ? "(" + string.Join(", ", suffixParts) + ")"
                    : "";

                sb.AppendLine($"    [{matId}] = {{");
                sb.AppendLine($"        nameTagPrefix = \"{nameTagPrefix}\",");
                sb.AppendLine($"        nameTagSuffix = \"{nameTagSuffix}\",");
                sb.AppendLine($"        descLines = {{");

                // ── Desc lines ────────────────────────────────────────────────
                var byCategory = new Dictionary<string, (CategoryConfig Cat, List<Usage> Usages)>();
                foreach (var u in usages)
                {
                    if (!byCategory.ContainsKey(u.Category.DescriptionHeader))
                        byCategory[u.Category.DescriptionHeader] = (u.Category, new List<Usage>());
                    byCategory[u.Category.DescriptionHeader].Usages.Add(u);
                }

                foreach (var (label, entry) in byCategory)
                {
                    string hColor = $"^{entry.Cat.DescriptionHeaderColor}";
                    string rColor = $"^{entry.Cat.DetailedDescriptionColor}";

                    if (entry.Cat.EnableDescription)
                        sb.AppendLine($"            \"{hColor}[{label}]{ColorReset}\",");

                    if (entry.Cat.EnableDetailedDescriptions)
                        foreach (var u in entry.Usages)
                            sb.AppendLine($"            \"{rColor}{u.ProductName} - {u.Qty} ea.{ColorReset}\",");
                }

                sb.AppendLine($"        }},");
                sb.AppendLine($"    }},");
            }

            sb.AppendLine("}");

            string outPath = Path.Combine(AppContext.BaseDirectory, "itemAnnotations.lua");
            await File.WriteAllTextAsync(outPath, sb.ToString());
            Console.WriteLine($"Done! Written to: {outPath}");
            Console.WriteLine($"Total items annotated: {annotations.Count}");
        }
    }

    record CategoryConfig(
        string File,
        bool EnableTags,
        string Tag,
        bool EnableDescription,
        string DescriptionHeader,
        string DescriptionHeaderColor,
        bool EnableDetailedDescriptions,
        string DetailedDescriptionColor,
        bool IsSuffix = false)
    {
        public static CategoryConfig From(string file, CategoryJsonConfig c, bool isSuffix) =>
            new(file, c.EnableTags == 1, $"[{c.TagText}]", c.EnableDescriptions == 1,
                c.HeaderText, c.DescriptionHeaderColor,
                c.EnableDetailedDescriptions == 1, c.DetailedDescriptionColor,
                isSuffix);
    }

    record Usage(CategoryConfig Category, string ProductName, int Qty);
}
